//! Service Bus batch invocation through the real router with in-memory infrastructure.

mod common;

use std::sync::atomic::Ordering;

use axum::http::StatusCode;
use common::*;

#[tokio::test]
async fn every_pair_is_costed_and_upserted_with_a_deterministic_id() {
    let app = TestApp::new();

    let (response, body) = app
        .invoke_batch(r#"{"items":[{"productId":1,"quantity":2},{"productId":1,"quantity":5}]}"#)
        .await;

    assert_eq!(response.status(), StatusCode::OK);
    assert!(body["Outputs"].is_object());
    let documents = app.store.documents.lock().unwrap();
    assert_eq!(documents.len(), 2);
    let document = &documents["bom:1:5"];
    assert_eq!(document.product_id, 1);
    assert_eq!(document.quantity, 5);
    assert_eq!(document.result.quantity, 5);
    assert!(document.computed_at.ends_with('Z'));
    assert!(!document.correlation_id.is_empty());
}

#[tokio::test]
async fn redelivery_overwrites_instead_of_duplicating() {
    let app = TestApp::new();
    let message = r#"{"items":[{"productId":1,"quantity":2}]}"#;

    app.invoke_batch(message).await;
    app.invoke_batch(message).await;

    assert_eq!(app.store.documents.lock().unwrap().len(), 1);
    assert_eq!(app.store.upserts.load(Ordering::SeqCst), 2);
}

#[tokio::test]
async fn batch_bypasses_the_cache() {
    let app = TestApp::new();

    app.invoke_batch(r#"{"items":[{"productId":1,"quantity":2}]}"#).await;

    assert_eq!(app.cache.reads.load(Ordering::SeqCst), 0);
    assert_eq!(app.cache.writes.load(Ordering::SeqCst), 0);
}

#[tokio::test]
async fn one_failing_pair_fails_the_message_but_the_rest_are_still_stored() {
    let app = TestApp::new();

    let (response, body) = app
        .invoke_batch(r#"{"items":[{"productId":1,"quantity":2},{"productId":999,"quantity":1},{"productId":1,"quantity":0},{"productId":1,"quantity":3}]}"#)
        .await;

    assert_eq!(
        response.status(),
        StatusCode::INTERNAL_SERVER_ERROR,
        "non-2xx makes the host retry"
    );
    assert!(body["detail"].as_str().unwrap().starts_with("2 of 4 items failed"));
    let documents = app.store.documents.lock().unwrap();
    assert_eq!(
        documents.keys().cloned().collect::<std::collections::BTreeSet<_>>(),
        ["bom:1:2".to_owned(), "bom:1:3".to_owned()].into()
    );
}

#[tokio::test]
async fn store_failure_fails_the_message() {
    let app = TestApp::new();
    app.store.fail_for.lock().unwrap().insert(1);

    let (response, _) = app.invoke_batch(r#"{"items":[{"productId":1,"quantity":2}]}"#).await;

    assert_eq!(response.status(), StatusCode::INTERNAL_SERVER_ERROR);
    assert!(app.store.documents.lock().unwrap().is_empty());
}

#[tokio::test]
async fn malformed_oversized_or_empty_messages_are_rejected_before_any_work() {
    let app = TestApp::new();
    let too_many = format!(
        r#"{{"items":[{}]}}"#,
        vec![r#"{"productId":1,"quantity":1}"#; 101].join(",")
    );

    for body in [
        "not json",
        r#"{"items":[]}"#,
        r#"{"items":[{"productId":1}]}"#,
        r#"{"items":[],"extra":1}"#,
        too_many.as_str(),
    ] {
        let (response, _) = app.invoke_batch(body).await;
        assert_eq!(response.status(), StatusCode::BAD_REQUEST, "{body}");
    }
    assert_eq!(app.repo.component_queries.load(Ordering::SeqCst), 0);
}

#[tokio::test]
async fn invocation_without_the_message_binding_is_rejected() {
    let app = TestApp::new();
    let request = axum::http::Request::post("/bom-batch")
        .header("content-type", "application/json")
        .body(axum::body::Body::from(r#"{"Data":{"other":"x"},"Metadata":{}}"#))
        .unwrap();

    let (response, _) = app.send(request).await;

    assert_eq!(response.status(), StatusCode::BAD_REQUEST);
}

#[tokio::test]
async fn batch_echoes_the_correlation_header() {
    let app = TestApp::new();
    let request = axum::http::Request::post("/bom-batch")
        .header("content-type", "application/json")
        .header("x-correlation-id", "batch-7")
        .body(axum::body::Body::from(
            r#"{"Data":{"message":"{\"items\":[{\"productId\":1,\"quantity\":1}]}"},"Metadata":{}}"#,
        ))
        .unwrap();

    let (response, _) = app.send(request).await;

    assert_eq!(header(&response, "x-correlation-id"), "batch-7");
    assert_eq!(app.store.documents.lock().unwrap()["bom:1:1"].correlation_id, "batch-7");
}
