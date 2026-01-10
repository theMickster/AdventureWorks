//! Live integration tests against the local SQL Server, Redis and the real Cosmos DB account.
//!
//! Ignored by default. Run with `scripts/test-live.sh`, which loads `local.settings.json` into the environment.

use std::{sync::Arc, time::Instant};

use aw_bom_cost::{
    batch::ResultDocument,
    http::{AppState, router},
    infra::{cosmos::CosmosResultStore, key_vault, redis_cache::RedisCache, sql::SqlBomRepository},
    service::BomService,
};
use axum::{
    Router,
    body::Body,
    http::{Request, Response, StatusCode},
};
use http_body_util::BodyExt;
use serde_json::{Value, json};
use tower::ServiceExt;

/// Mountain-100 Silver, 38: 14 direct components, 48 distinct leaf parts.
const MOUNTAIN_100: i32 = 771;
/// Roots tied for the deepest tree in AdventureWorks (4 levels, 89 nodes).
const DEEPEST_TREE: i32 = 971;
/// Unit total for [`MOUNTAIN_100`] cross-checked against a recursive CTE over the same tables.
const MOUNTAIN_100_MATERIAL: f64 = 1748.2544;
const MOUNTAIN_100_LABOR: f64 = 49.0;
const LATENCY_BUDGET_MS: u128 = 500;
/// Quantities unlikely to collide with manual runs. Each cache-sensitive test owns one, because tests run in
/// parallel and share Redis; the key is deleted before the test starts.
const TEST_QUANTITY: u32 = 7;
const WHAT_IF_QUANTITY: u32 = 6;

struct Live {
    router: Router,
    store: CosmosResultStore,
    redis: redis::Client,
}

impl Live {
    async fn start() -> Self {
        let settings = key_vault::settings_from_env()
            .await
            .expect("run via scripts/test-live.sh so settings are in the environment");
        let repository = SqlBomRepository::connect(&settings.sql_connection_string)
            .await
            .unwrap();
        let cache = RedisCache::new(&settings.redis_url).unwrap();
        let store = CosmosResultStore::new(&settings.cosmos).unwrap();
        let state = Arc::new(AppState {
            service: BomService::new(repository, cache, settings.cache_ttl),
            store: store.clone(),
        });
        Self {
            router: router(state),
            store,
            redis: redis::Client::open(settings.redis_url).unwrap(),
        }
    }

    async fn forget(&self, product_id: i32, quantity: u32) {
        let mut connection = self.redis.get_multiplexed_async_connection().await.unwrap();
        redis::cmd("DEL")
            .arg(format!("bom:{product_id}:{quantity}"))
            .query_async::<i64>(&mut connection)
            .await
            .unwrap();
    }

    async fn send(&self, request: Request<Body>) -> (Response<Body>, Value) {
        let response = self.router.clone().oneshot(request).await.unwrap();
        let (parts, body) = response.into_parts();
        let bytes = body.collect().await.unwrap().to_bytes();
        (
            Response::from_parts(parts, Body::empty()),
            serde_json::from_slice(&bytes).unwrap_or(Value::Null),
        )
    }

    async fn get(&self, uri: &str, no_cache: bool) -> (Response<Body>, Value) {
        let mut request = Request::get(uri);
        if no_cache {
            request = request.header("cache-control", "no-cache");
        }
        self.send(request.body(Body::empty()).unwrap()).await
    }
}

fn cache_header(response: &Response<Body>) -> &str {
    response.headers()["x-cache"].to_str().unwrap()
}

#[tokio::test]
#[ignore = "needs local SQL Server, Redis, Key Vault access and the real Cosmos DB account"]
async fn http_result_matches_the_database_and_is_served_from_cache_second_time() {
    let live = Live::start().await;
    live.forget(MOUNTAIN_100, TEST_QUANTITY).await;
    let uri = format!("/api/bom-cost/{MOUNTAIN_100}?quantity={TEST_QUANTITY}");

    let (first, body) = live.get(&uri, false).await;
    let (second, cached) = live.get(&uri, false).await;
    let (refreshed, _) = live.get(&uri, true).await;
    let (after, _) = live.get(&uri, false).await;

    assert_eq!(first.status(), StatusCode::OK);
    assert_eq!(body["unitCost"]["material"], MOUNTAIN_100_MATERIAL);
    assert_eq!(body["unitCost"]["labor"], MOUNTAIN_100_LABOR);
    assert_eq!(body["feasibility"]["componentsChecked"], 48);
    assert_eq!(body["feasibility"]["feasible"], true);
    assert_eq!(body["tree"]["children"].as_array().unwrap().len(), 14);
    assert_eq!(cache_header(&first), "miss");
    assert_eq!(cache_header(&second), "hit");
    assert_eq!(cached["tree"], body["tree"]);
    assert_eq!(cache_header(&refreshed), "refresh");
    assert_eq!(cache_header(&after), "hit");
}

#[tokio::test]
#[ignore = "needs local SQL Server, Redis, Key Vault access and the real Cosmos DB account"]
async fn shortage_uses_the_aggregate_quantity_across_locations() {
    let live = Live::start().await;

    // Paint - Silver (494): 8 per bike, 65 on hand in total across all locations.
    let (_, ok) = live
        .get(&format!("/api/bom-cost/{MOUNTAIN_100}?quantity=8"), true)
        .await;
    let (_, short) = live
        .get(&format!("/api/bom-cost/{MOUNTAIN_100}?quantity=9"), true)
        .await;

    assert_eq!(ok["feasibility"]["feasible"], true);
    assert_eq!(
        short["feasibility"]["shortages"],
        json!([{ "productId": 494, "name": "Paint - Silver", "required": 72.0, "available": 65.0, "deficit": 7.0 }])
    );
}

#[tokio::test]
#[ignore = "needs local SQL Server, Redis, Key Vault access and the real Cosmos DB account"]
async fn what_if_is_a_delta_from_baseline_and_is_never_cached() {
    let live = Live::start().await;
    live.forget(MOUNTAIN_100, WHAT_IF_QUANTITY).await;

    let (response, body) = live
        .get(
            &format!("/api/bom-cost/{MOUNTAIN_100}?quantity={WHAT_IF_QUANTITY}&override=748:800"),
            false,
        )
        .await;
    let (baseline, _) = live
        .get(
            &format!("/api/bom-cost/{MOUNTAIN_100}?quantity={WHAT_IF_QUANTITY}"),
            false,
        )
        .await;

    assert_eq!(cache_header(&response), "skipped");
    assert_eq!(
        cache_header(&baseline),
        "miss",
        "the what-if run left nothing in the cache"
    );
    // Frame 748 moves from 747.2002 to 800.
    assert_eq!(body["whatIf"]["deltaUnit"]["total"], 52.7998);
}

#[tokio::test]
#[ignore = "needs local SQL Server, Redis, Key Vault access and the real Cosmos DB account"]
async fn deepest_tree_is_costed_within_the_latency_budget() {
    let live = Live::start().await;
    let uri = format!("/api/bom-cost/{DEEPEST_TREE}?quantity=3");
    live.get(&uri, true).await; // warm the SQL pool

    let mut slowest = 0;
    for _ in 0..10 {
        let started = Instant::now();
        let (response, body) = live.get(&uri, true).await; // no-cache: full database path every time
        slowest = slowest.max(started.elapsed().as_millis());
        assert_eq!(response.status(), StatusCode::OK);
        assert!(!body["tree"]["children"].as_array().unwrap().is_empty());
    }

    println!("deepest tree ({DEEPEST_TREE}): slowest of 10 uncached requests = {slowest} ms");
    assert!(
        slowest < LATENCY_BUDGET_MS,
        "{slowest} ms exceeds the {LATENCY_BUDGET_MS} ms budget"
    );
}

#[tokio::test]
#[ignore = "needs local SQL Server, Redis, Key Vault access and the real Cosmos DB account"]
async fn batch_message_upserts_each_pair_into_cosmos_with_deterministic_ids() {
    let live = Live::start().await;
    let message = json!({ "items": [
        { "productId": MOUNTAIN_100, "quantity": 2 },
        { "productId": DEEPEST_TREE, "quantity": 1 },
    ]})
    .to_string();
    let envelope = json!({ "Data": { "message": message }, "Metadata": {} });
    let invoke = || {
        Request::post("/bom-batch")
            .header("content-type", "application/json")
            .header("x-correlation-id", "live-batch")
            .body(Body::from(envelope.to_string()))
            .unwrap()
    };

    let (first, _) = live.send(invoke()).await;
    let (second, _) = live.send(invoke()).await;

    assert_eq!(first.status(), StatusCode::OK);
    assert_eq!(second.status(), StatusCode::OK, "redelivery is idempotent");
    let document: ResultDocument = live.store.read(MOUNTAIN_100, "bom:771:2").await.unwrap();
    assert_eq!(document.correlation_id, "live-batch");
    assert_eq!(document.result.unit_cost.material.to_string(), "1748.2544");
    assert!(live.store.read(DEEPEST_TREE, "bom:971:1").await.is_ok());
}

#[tokio::test]
#[ignore = "needs local SQL Server, Redis, Key Vault access and the real Cosmos DB account"]
async fn batch_with_one_bad_pair_fails_the_message_after_storing_the_good_one() {
    let live = Live::start().await;
    let message = json!({ "items": [
        { "productId": MOUNTAIN_100, "quantity": 3 },
        { "productId": 999_999, "quantity": 1 },
    ]})
    .to_string();
    let request = Request::post("/bom-batch")
        .header("content-type", "application/json")
        .body(Body::from(
            json!({ "Data": { "message": message }, "Metadata": {} }).to_string(),
        ))
        .unwrap();

    let (response, body) = live.send(request).await;

    assert_eq!(response.status(), StatusCode::INTERNAL_SERVER_ERROR);
    assert!(body["detail"].as_str().unwrap().starts_with("1 of 2 items failed"));
    assert!(live.store.read(MOUNTAIN_100, "bom:771:3").await.is_ok());
}
