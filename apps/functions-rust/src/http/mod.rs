//! HTTP surface of the custom handler: costing, health, and the Service Bus invocation endpoint.

mod correlation;
mod handlers;
mod problem;

use std::sync::Arc;

use axum::{
    Router, middleware,
    routing::{get, post},
};

use crate::{
    ports::{BomRepository, ResultCache, ResultStore},
    service::BomService,
};

pub use correlation::CorrelationId;
pub use problem::Problem;

/// Route of the costing endpoint, as declared in `bom-cost/function.json`.
pub const BOM_COST_ROUTE: &str = "/api/bom-cost/{product_id}";
/// Route of the health probe, as declared in `health/function.json`.
pub const HEALTH_ROUTE: &str = "/api/health";
/// Invocation route the Functions host posts Service Bus messages to (the function name).
pub const BATCH_ROUTE: &str = "/bom-batch";
/// Binding name in `bom-batch/function.json` that carries the message body.
pub const BATCH_BINDING: &str = "message";

/// Shared handler state.
pub struct AppState<R, C, S> {
    /// Costing service over the SQL repository and Redis cache.
    pub service: BomService<R, C>,
    /// Destination for batch results.
    pub store: S,
}

/// Builds the router around `state`.
pub fn router<R, C, S>(state: Arc<AppState<R, C, S>>) -> Router
where
    R: BomRepository + 'static,
    C: ResultCache + 'static,
    S: ResultStore + 'static,
{
    Router::new()
        .route(BOM_COST_ROUTE, get(handlers::get_bom_cost::<R, C, S>))
        .route(HEALTH_ROUTE, get(handlers::get_health::<R, C, S>))
        .route(BATCH_ROUTE, post(handlers::post_batch::<R, C, S>))
        .fallback(handlers::not_found)
        .layer(middleware::from_fn(correlation::propagate))
        .with_state(state)
}
