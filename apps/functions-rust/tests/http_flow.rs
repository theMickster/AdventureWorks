//! End-to-end behavior through the real router with in-memory infrastructure.

mod common;

use std::sync::atomic::Ordering;

use axum::http::StatusCode;
use common::*;
use rust_decimal_macros::dec;

fn num(value: &serde_json::Value) -> rust_decimal::Decimal {
    rust_decimal::Decimal::try_from(value.as_f64().expect("number")).unwrap()
}

#[tokio::test]
async fn returns_cost_tree_and_feasibility() {
    let app = TestApp::new();

    let (response, body) = app.get("/api/bom-cost/1?quantity=2").await;

    assert_eq!(response.status(), StatusCode::OK);
    assert_eq!(header(&response, "x-cache"), "miss");
    assert_eq!(body["product"]["id"], 1);
    assert_eq!(body["quantity"], 2);
    assert_eq!(num(&body["unitCost"]["material"]), dec!(36));
    assert_eq!(num(&body["unitCost"]["labor"]), dec!(17));
    assert_eq!(num(&body["batchCost"]["total"]), dec!(106));
    assert_eq!(body["feasibility"]["feasible"], true);
    assert_eq!(body["tree"]["children"][0]["children"][0]["cumulativeQty"], 6.0);
    assert!(body.get("whatIf").is_none());
}

#[tokio::test]
async fn second_identical_request_is_served_from_the_cache() {
    let app = TestApp::new();

    let (first, first_body) = app.get("/api/bom-cost/1?quantity=2").await;
    let queries_after_first = app.repo.component_queries.load(Ordering::SeqCst);
    let (second, second_body) = app.get("/api/bom-cost/1?quantity=2").await;

    assert_eq!(header(&first, "x-cache"), "miss");
    assert_eq!(header(&second, "x-cache"), "hit");
    assert_eq!(
        app.repo.component_queries.load(Ordering::SeqCst),
        queries_after_first,
        "no database work on a hit"
    );
    assert_eq!(first_body["tree"], second_body["tree"]);
    assert_eq!(first_body["unitCost"], second_body["unitCost"]);
    assert!(app.cache.entries.lock().unwrap().contains_key("bom:1:2"));
    assert_eq!(*app.cache.last_ttl.lock().unwrap(), Some(TEST_TTL));
}

#[tokio::test]
async fn cache_key_includes_quantity() {
    let app = TestApp::new();

    app.get("/api/bom-cost/1?quantity=2").await;
    let (other, _) = app.get("/api/bom-cost/1?quantity=3").await;

    assert_eq!(header(&other, "x-cache"), "miss");
    assert!(app.cache.entries.lock().unwrap().contains_key("bom:1:3"));
}

#[tokio::test]
async fn no_cache_header_bypasses_the_read_and_refreshes_the_entry() {
    let app = TestApp::new();
    app.get("/api/bom-cost/1?quantity=2").await;
    app.repo.set_stock(PART_A, dec!(10));

    let (stale, stale_body) = app.get("/api/bom-cost/1?quantity=2").await;
    let (refreshed, refreshed_body) = app
        .get_with("/api/bom-cost/1?quantity=2", ("cache-control", "no-cache"))
        .await;
    let (after, after_body) = app.get("/api/bom-cost/1?quantity=2").await;

    assert_eq!(header(&stale, "x-cache"), "hit");
    assert_eq!(
        stale_body["feasibility"]["feasible"], true,
        "TTL-only invalidation: stale until refreshed"
    );
    assert_eq!(header(&refreshed, "x-cache"), "refresh");
    assert_eq!(refreshed_body["feasibility"]["feasible"], false);
    assert_eq!(header(&after, "x-cache"), "hit");
    assert_eq!(
        after_body["feasibility"]["feasible"], false,
        "refresh overwrote the cached entry"
    );
}

#[tokio::test]
async fn what_if_returns_a_delta_and_never_touches_the_cache() {
    let app = TestApp::new();
    app.get("/api/bom-cost/1?quantity=2").await;
    let (reads, writes) = (
        app.cache.reads.load(Ordering::SeqCst),
        app.cache.writes.load(Ordering::SeqCst),
    );

    let (response, body) = app.get("/api/bom-cost/1?quantity=2&override=3:6").await;

    assert_eq!(response.status(), StatusCode::OK);
    assert_eq!(header(&response, "x-cache"), "skipped");
    assert_eq!(body["cache"], "skipped");
    assert_eq!(num(&body["whatIf"]["deltaUnit"]["total"]), dec!(12));
    assert_eq!(num(&body["whatIf"]["deltaBatch"]["total"]), dec!(24));
    assert_eq!(num(&body["whatIf"]["baselineUnit"]["total"]), dec!(53));
    assert_eq!(num(&body["unitCost"]["total"]), dec!(65));
    assert_eq!(app.cache.reads.load(Ordering::SeqCst), reads);
    assert_eq!(app.cache.writes.load(Ordering::SeqCst), writes);

    let (baseline, baseline_body) = app.get("/api/bom-cost/1?quantity=2").await;
    assert_eq!(header(&baseline, "x-cache"), "hit");
    assert_eq!(
        num(&baseline_body["unitCost"]["total"]),
        dec!(53),
        "baseline entry is unaffected by what-if runs"
    );
}

#[tokio::test]
async fn what_if_reports_ignored_overrides() {
    let app = TestApp::new();

    let (_, body) = app.get("/api/bom-cost/1?override=42:1&override=1:1").await;

    let ignored = body["whatIf"]["ignored"].as_array().unwrap();
    assert_eq!(ignored.len(), 2);
    assert_eq!(ignored[0]["productId"], 1);
}

#[tokio::test]
async fn shortage_is_reported_with_required_available_and_deficit() {
    let app = TestApp::new();
    app.repo.set_stock(PART_A, dec!(10));

    let (_, body) = app.get("/api/bom-cost/1?quantity=2").await;

    let shortage = &body["feasibility"]["shortages"][0];
    assert_eq!(body["feasibility"]["feasible"], false);
    assert_eq!(shortage["productId"], PART_A);
    assert_eq!(shortage["required"], 12.0);
    assert_eq!(shortage["available"], 10.0);
    assert_eq!(shortage["deficit"], 2.0);
}

#[tokio::test]
async fn inbound_correlation_id_is_echoed_in_header_and_body() {
    let app = TestApp::new();

    let (response, body) = app.get_with("/api/bom-cost/1", ("x-correlation-id", "trace-42")).await;

    assert_eq!(header(&response, "x-correlation-id"), "trace-42");
    assert_eq!(body["correlationId"], "trace-42");
}

#[tokio::test]
async fn missing_or_unsafe_correlation_id_is_replaced_with_a_generated_one() {
    let app = TestApp::new();

    let (generated, body) = app.get("/api/bom-cost/1").await;
    let (replaced, _) = app
        .get_with("/api/bom-cost/1", ("x-correlation-id", "bad value;"))
        .await;

    let id = header(&generated, "x-correlation-id");
    assert_eq!(id.len(), 36);
    assert_eq!(body["correlationId"], id);
    assert_ne!(header(&replaced, "x-correlation-id"), "bad value;");
    assert_eq!(header(&replaced, "x-correlation-id").len(), 36);
}

#[tokio::test]
async fn invalid_input_yields_problem_details_with_the_correlation_id() {
    let app = TestApp::new();
    for uri in [
        "/api/bom-cost/0",
        "/api/bom-cost/abc",
        "/api/bom-cost/1?quantity=0",
        "/api/bom-cost/1?quantity=10001",
        "/api/bom-cost/1?override=bad",
        "/api/bom-cost/1?unexpected=1",
    ] {
        let (response, body) = app.get_with(uri, ("x-correlation-id", "cid-1")).await;

        assert_eq!(response.status(), StatusCode::BAD_REQUEST, "{uri}");
        assert_eq!(header(&response, "content-type"), "application/problem+json");
        assert_eq!(body["correlationId"], "cid-1", "{uri}");
        assert_eq!(body["status"], 400);
    }
    assert_eq!(
        app.repo.component_queries.load(Ordering::SeqCst),
        0,
        "rejected before any database work"
    );
}

#[tokio::test]
async fn too_many_overrides_are_rejected() {
    let app = TestApp::new();
    let query: String = (1..=51)
        .map(|i| format!("override={i}:1"))
        .collect::<Vec<_>>()
        .join("&");

    let (response, _) = app.get(&format!("/api/bom-cost/1?{query}")).await;

    assert_eq!(response.status(), StatusCode::BAD_REQUEST);
}

#[tokio::test]
async fn unknown_product_is_404_and_empty_bom_is_422() {
    let app = TestApp::new();

    let (missing, _) = app.get("/api/bom-cost/999").await;
    let (empty, body) = app.get(&format!("/api/bom-cost/{PART_A}")).await;

    assert_eq!(missing.status(), StatusCode::NOT_FOUND);
    assert_eq!(empty.status(), StatusCode::UNPROCESSABLE_ENTITY);
    assert!(
        body["detail"]
            .as_str()
            .unwrap()
            .contains("no current bill of materials")
    );
}

#[tokio::test]
async fn circular_bom_is_422_with_the_cycle_path() {
    let mut repo = FakeRepo::fixture();
    repo.edges.push(aw_bom_cost::domain::model::BomEdge {
        assembly_id: PART_A,
        component_id: ROOT,
        per_assembly_qty: dec!(1),
    });
    let app = TestApp::with_repo(repo);

    let (response, body) = app.get("/api/bom-cost/1").await;

    assert_eq!(response.status(), StatusCode::UNPROCESSABLE_ENTITY);
    assert!(body["detail"].as_str().unwrap().contains("1 -> 2 -> 3 -> 1"));
}

#[tokio::test]
async fn database_outage_is_503_without_leaking_details() {
    let app = TestApp::new();
    app.repo.down.store(true, Ordering::SeqCst);

    let (response, body) = app.get("/api/bom-cost/1").await;

    assert_eq!(response.status(), StatusCode::SERVICE_UNAVAILABLE);
    assert!(!body.to_string().contains("connection refused"));
    assert!(body["correlationId"].is_string());
}

#[tokio::test]
async fn cache_outage_degrades_to_a_fresh_computation() {
    let app = TestApp::new();
    app.cache.down.store(true, Ordering::SeqCst);

    let (response, body) = app.get("/api/bom-cost/1").await;

    assert_eq!(response.status(), StatusCode::OK);
    assert_eq!(header(&response, "x-cache"), "unavailable");
    assert_eq!(body["cache"], "unavailable");
}

#[tokio::test]
async fn unreadable_cache_entry_is_recomputed() {
    let app = TestApp::new();
    app.cache
        .entries
        .lock()
        .unwrap()
        .insert("bom:1:1".to_owned(), "{not json".to_owned());

    let (response, _) = app.get("/api/bom-cost/1").await;

    assert_eq!(response.status(), StatusCode::OK);
    assert_eq!(header(&response, "x-cache"), "miss");
}

#[tokio::test]
async fn deep_chain_loads_one_query_per_level() {
    let app = TestApp::new();

    app.get("/api/bom-cost/1").await;

    // Levels: root, sub-assembly + part C, leaves (no rows). Three rounds, never per node.
    assert_eq!(app.repo.component_queries.load(Ordering::SeqCst), 3);
}

#[tokio::test]
async fn unknown_route_is_a_problem_with_a_correlation_id() {
    let app = TestApp::new();

    let (response, body) = app.get("/nope").await;

    assert_eq!(response.status(), StatusCode::NOT_FOUND);
    assert!(body["correlationId"].is_string());
    assert!(response.headers().contains_key("x-correlation-id"));
}

#[tokio::test]
async fn health_reports_healthy_degraded_and_unhealthy() {
    let app = TestApp::new();
    let (ok, body) = app.get("/api/health").await;
    assert_eq!(
        (ok.status(), body["status"].as_str()),
        (StatusCode::OK, Some("healthy"))
    );

    app.cache.down.store(true, Ordering::SeqCst);
    let (degraded, body) = app.get("/api/health").await;
    assert_eq!(
        (degraded.status(), body["status"].as_str()),
        (StatusCode::OK, Some("degraded"))
    );

    app.repo.down.store(true, Ordering::SeqCst);
    let (down, body) = app.get("/api/health").await;
    assert_eq!(
        (down.status(), body["status"].as_str()),
        (StatusCode::SERVICE_UNAVAILABLE, Some("unhealthy"))
    );
    assert!(body["correlationId"].is_string());
}

#[tokio::test]
async fn malformed_query_encoding_is_a_problem_with_a_correlation_id() {
    let app = TestApp::new();

    let (response, body) = app
        .get_with("/api/bom-cost/1?quantity=%FF%FE", ("x-correlation-id", "enc-1"))
        .await;

    assert_eq!(response.status(), StatusCode::BAD_REQUEST);
    assert_eq!(body["correlationId"], "enc-1");
}

#[tokio::test]
async fn unparseable_invocation_body_is_a_problem_with_a_correlation_id() {
    let app = TestApp::new();
    let request = axum::http::Request::post("/bom-batch")
        .header("content-type", "application/json")
        .header("x-correlation-id", "inv-1")
        .body(axum::body::Body::from("not json"))
        .unwrap();

    let (response, body) = app.send(request).await;

    assert!(response.status().is_client_error());
    assert_eq!(body["correlationId"], "inv-1");
}

#[tokio::test]
async fn wrong_method_is_a_problem_with_a_correlation_id() {
    let app = TestApp::new();
    let request = axum::http::Request::post("/api/bom-cost/1")
        .body(axum::body::Body::empty())
        .unwrap();

    let (response, body) = app.send(request).await;

    assert_eq!(response.status(), StatusCode::METHOD_NOT_ALLOWED);
    assert!(body["correlationId"].is_string());
}

#[tokio::test]
async fn failed_cache_write_on_a_refresh_is_reported_as_unavailable() {
    let app = TestApp::new();
    app.cache.down.store(true, Ordering::SeqCst);

    let (response, body) = app.get_with("/api/bom-cost/1", ("cache-control", "no-cache")).await;

    assert_eq!(response.status(), StatusCode::OK);
    assert_eq!(header(&response, "x-cache"), "unavailable");
    assert_eq!(body["cache"], "unavailable");
}
