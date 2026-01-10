//! Axum handlers. Each one reads the correlation id, validates at the boundary, then delegates.

use std::{collections::HashMap, sync::Arc};

use axum::{
    Extension, Json,
    extract::{Path, Query, State},
    http::{HeaderMap, HeaderValue, StatusCode, header},
    response::{IntoResponse, Response},
};
use serde::Serialize;
use serde_json::{Value, json};

use super::{AppState, BATCH_BINDING, CorrelationId, Problem};
use crate::{
    batch::{parse_batch, process_batch},
    domain::model::BomCostResult,
    limits::CACHE_HEADER,
    ports::{BomRepository, ResultCache, ResultStore},
    service::{CacheMode, CacheStatus},
    validation::parse_cost_request,
};

type SharedState<R, C, S> = State<Arc<AppState<R, C, S>>>;

#[derive(Serialize)]
#[serde(rename_all = "camelCase")]
struct CostResponse<'a> {
    correlation_id: &'a str,
    cache: CacheStatus,
    #[serde(flatten)]
    result: &'a BomCostResult,
}

/// `GET /api/bom-cost/{productId}?quantity=&override=componentId:price`
pub async fn get_bom_cost<R, C, S>(
    State(state): SharedState<R, C, S>,
    Extension(CorrelationId(correlation_id)): Extension<CorrelationId>,
    Path(product_id): Path<String>,
    Query(query): Query<Vec<(String, String)>>,
    headers: HeaderMap,
) -> Response
where
    R: BomRepository + 'static,
    C: ResultCache + 'static,
    S: ResultStore + 'static,
{
    let request = match parse_cost_request(&product_id, &query) {
        Ok(request) => request,
        Err(error) => return Problem::from_service(&error.into(), &correlation_id).into_response(),
    };
    let mode = if wants_no_cache(&headers) {
        CacheMode::Refresh
    } else {
        CacheMode::Use
    };

    match state.service.cost(&request, mode).await {
        Ok((result, cache)) => {
            let body = CostResponse {
                correlation_id: &correlation_id,
                cache,
                result: &result,
            };
            let mut response = Json(body).into_response();
            response
                .headers_mut()
                .insert(CACHE_HEADER, HeaderValue::from_static(cache.as_header()));
            response
        }
        Err(error) => Problem::from_service(&error, &correlation_id).into_response(),
    }
}

/// `GET /api/health`: 200 when SQL Server answers, 503 otherwise. A Redis outage degrades but does not fail it.
pub async fn get_health<R, C, S>(
    State(state): SharedState<R, C, S>,
    Extension(CorrelationId(correlation_id)): Extension<CorrelationId>,
) -> Response
where
    R: BomRepository + 'static,
    C: ResultCache + 'static,
    S: ResultStore + 'static,
{
    let report = state.service.health().await;
    let status = match (report.sql, report.cache) {
        (true, true) => ("healthy", StatusCode::OK),
        (true, false) => ("degraded", StatusCode::OK),
        (false, _) => ("unhealthy", StatusCode::SERVICE_UNAVAILABLE),
    };
    (
        status.1,
        Json(json!({ "status": status.0, "sql": report.sql, "cache": report.cache, "correlationId": correlation_id })),
    )
        .into_response()
}

/// Service Bus invocation: `POST /bom-batch`. Any failed pair fails the invocation so the host retries
/// and eventually dead-letters the message.
pub async fn post_batch<R, C, S>(
    State(state): SharedState<R, C, S>,
    Extension(CorrelationId(correlation_id)): Extension<CorrelationId>,
    Json(invocation): Json<Invocation>,
) -> Response
where
    R: BomRepository + 'static,
    C: ResultCache + 'static,
    S: ResultStore + 'static,
{
    let body = match message_body(&invocation) {
        Some(body) => body,
        None => {
            return Problem::new(
                StatusCode::BAD_REQUEST,
                "Invalid invocation",
                format!("invocation data has no '{BATCH_BINDING}' binding"),
                &correlation_id,
            )
            .into_response();
        }
    };
    let batch = match parse_batch(&body) {
        Ok(batch) => batch,
        Err(error) => {
            tracing::error!(%error, "rejecting batch message");
            return Problem::new(
                StatusCode::BAD_REQUEST,
                "Invalid batch message",
                error.to_string(),
                &correlation_id,
            )
            .into_response();
        }
    };

    let outcome = process_batch(&state.service, &state.store, &batch, &correlation_id).await;
    if outcome.failures.is_empty() {
        tracing::info!(items = outcome.succeeded, "batch processed");
        return Json(json!({ "Outputs": {}, "Logs": [], "ReturnValue": null })).into_response();
    }
    let failed: Vec<String> = outcome
        .failures
        .iter()
        .map(|f| format!("#{}: {}", f.index, f.error))
        .collect();
    Problem::new(
        StatusCode::INTERNAL_SERVER_ERROR,
        "Batch partially failed",
        format!(
            "{} of {} items failed ({})",
            failed.len(),
            batch.items.len(),
            failed.join("; ")
        ),
        &correlation_id,
    )
    .into_response()
}

/// Fallback for unknown routes.
pub async fn not_found(Extension(CorrelationId(correlation_id)): Extension<CorrelationId>) -> Response {
    Problem::new(StatusCode::NOT_FOUND, "Not found", "No such route.", &correlation_id).into_response()
}

/// The Functions host invocation envelope; only the fields this handler reads.
#[derive(Debug, serde::Deserialize)]
#[serde(rename_all = "PascalCase")]
pub struct Invocation {
    data: HashMap<String, Value>,
}

/// The message body as JSON text. The host hands over a string; when that string is itself a JSON-encoded
/// string (as the admin invoke API produces) it is decoded once more.
fn message_body(invocation: &Invocation) -> Option<String> {
    match invocation.data.get(BATCH_BINDING)? {
        Value::String(raw) => match serde_json::from_str::<Value>(raw) {
            Ok(Value::String(inner)) => Some(inner),
            _ => Some(raw.clone()),
        },
        other => Some(other.to_string()),
    }
}

/// True when the request carries `Cache-Control: no-cache`.
fn wants_no_cache(headers: &HeaderMap) -> bool {
    headers
        .get_all(header::CACHE_CONTROL)
        .iter()
        .filter_map(|value| value.to_str().ok())
        .flat_map(|value| value.split(','))
        .any(|directive| directive.trim().eq_ignore_ascii_case("no-cache"))
}

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn no_cache_directive_is_detected_among_others() {
        let mut headers = HeaderMap::new();
        headers.insert(header::CACHE_CONTROL, HeaderValue::from_static("max-age=0, No-Cache"));
        assert!(wants_no_cache(&headers));
    }

    fn invocation(message: Value) -> Invocation {
        Invocation {
            data: HashMap::from([(BATCH_BINDING.to_owned(), message)]),
        }
    }

    #[test]
    fn message_body_accepts_plain_double_encoded_and_object_forms() {
        let body = r#"{"items":[]}"#;
        assert_eq!(message_body(&invocation(json!(body))).as_deref(), Some(body));
        assert_eq!(
            message_body(&invocation(json!(json!(body).to_string()))).as_deref(),
            Some(body)
        );
        assert_eq!(message_body(&invocation(json!({"items": []}))).as_deref(), Some(body));
        assert_eq!(message_body(&Invocation { data: HashMap::new() }), None);
    }

    #[test]
    fn absent_or_other_directives_do_not_bypass() {
        let mut headers = HeaderMap::new();
        assert!(!wants_no_cache(&headers));
        headers.insert(header::CACHE_CONTROL, HeaderValue::from_static("max-age=60"));
        assert!(!wants_no_cache(&headers));
    }
}
