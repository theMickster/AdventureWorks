//! Service-level behavior over in-memory infrastructure: depth limits and an unreachable Cosmos DB.

mod common;

use std::{sync::atomic::Ordering, time::Duration};

use aw_bom_cost::{
    config::CosmosSettings,
    domain::{
        error::DomainError,
        model::{BomEdge, ProductId, ProductInfo},
    },
    error::ServiceError,
    infra::{cosmos::CosmosResultStore, memory_cache::MemoryCache},
    limits::{MAX_BOM_DEPTH, MEMORY_CACHE_MAX_ENTRIES},
    service::{BomService, CacheMode, CacheStatus},
    validation::CostRequest,
};
use axum::http::StatusCode;
use common::*;
use rust_decimal_macros::dec;

/// A straight chain `0 -> 1 -> ... -> levels`, each node costing 1 except the root.
fn chain(levels: usize) -> FakeRepo {
    let mut repo = FakeRepo::default();
    for id in 0..=levels as ProductId {
        repo.products.push(ProductInfo {
            id,
            name: format!("Product {id}"),
            product_number: format!("PN-{id}"),
            standard_cost: if id == 0 { dec!(0) } else { dec!(1) },
        });
        if id > 0 {
            repo.edges.push(BomEdge {
                assembly_id: id - 1,
                component_id: id,
                per_assembly_qty: dec!(1),
            });
        }
    }
    repo
}

fn request(product_id: ProductId) -> CostRequest {
    CostRequest {
        product_id,
        quantity: 1,
        overrides: Default::default(),
    }
}

#[tokio::test]
async fn in_process_cache_serves_a_repeat_request_without_querying_sql() {
    let app = TestApp::with_repo(chain(2));
    let cache = MemoryCache::new(MEMORY_CACHE_MAX_ENTRIES);
    let service = BomService::new(app.repo.clone(), cache, TEST_TTL);

    let (first, first_status) = service.cost(&request(0), CacheMode::Use).await.unwrap();
    let queries_after_first = app.repo.component_queries.load(Ordering::SeqCst);
    let (second, second_status) = service.cost(&request(0), CacheMode::Use).await.unwrap();

    assert_eq!(first_status, CacheStatus::Miss);
    assert_eq!(second_status, CacheStatus::Hit);
    assert_eq!(second.unit_cost.material, first.unit_cost.material);
    assert!(queries_after_first > 0, "the first request must have queried SQL");
    assert_eq!(app.repo.component_queries.load(Ordering::SeqCst), queries_after_first);
}

#[tokio::test]
async fn bom_exactly_at_the_depth_limit_is_costed() {
    let app = TestApp::with_repo(chain(MAX_BOM_DEPTH));
    let service = BomService::new(app.repo.clone(), app.cache.clone(), TEST_TTL);

    let (result, _) = service.cost(&request(0), CacheMode::Use).await.unwrap();

    assert_eq!(result.unit_cost.material, dec!(1));
}

#[tokio::test]
async fn bom_one_level_past_the_limit_is_rejected() {
    let app = TestApp::with_repo(chain(MAX_BOM_DEPTH + 1));
    let service = BomService::new(app.repo.clone(), app.cache.clone(), TEST_TTL);

    let error = service.cost(&request(0), CacheMode::Use).await.unwrap_err();

    assert!(
        matches!(
            error,
            ServiceError::Domain(DomainError::DepthExceeded { max: MAX_BOM_DEPTH })
        ),
        "{error:?}"
    );
}

#[tokio::test]
async fn unreachable_cosmos_fails_batches_only_and_leaves_the_handler_serving() {
    let repo = std::sync::Arc::new(FakeRepo::fixture());
    let cache = std::sync::Arc::new(FakeCache::default());
    let settings = CosmosSettings {
        endpoint: "http://localhost:1".to_owned(),
        database: "db".to_owned(),
        container: "c".to_owned(),
        preferred_region: "East US".to_owned(),
    };
    // Construction must succeed with nothing listening: it may not contact the account.
    let store = CosmosResultStore::new(&settings).expect("store builds without network");
    let state = std::sync::Arc::new(aw_bom_cost::http::AppState {
        service: BomService::new(repo, cache, TEST_TTL),
        store,
    });
    let app = aw_bom_cost::http::router(state);
    let send = |request: axum::http::Request<axum::body::Body>| {
        let app = app.clone();
        async move { tower::ServiceExt::oneshot(app, request).await.unwrap().status() }
    };

    let health = send(
        axum::http::Request::get("/api/health")
            .body(axum::body::Body::empty())
            .unwrap(),
    )
    .await;
    let cost = send(
        axum::http::Request::get("/api/bom-cost/1")
            .body(axum::body::Body::empty())
            .unwrap(),
    )
    .await;
    let batch = tokio::time::timeout(
        Duration::from_secs(120),
        send(
            axum::http::Request::post("/bom-batch")
                .header("content-type", "application/json")
                .body(axum::body::Body::from(
                    r#"{"Data":{"message":"{\"items\":[{\"productId\":1,\"quantity\":1}]}"},"Metadata":{}}"#,
                ))
                .unwrap(),
        ),
    )
    .await
    .expect("batch against an unreachable Cosmos DB must fail, not hang");

    assert_eq!(health, StatusCode::OK);
    assert_eq!(cost, StatusCode::OK);
    assert_eq!(batch, StatusCode::INTERNAL_SERVER_ERROR);
}
