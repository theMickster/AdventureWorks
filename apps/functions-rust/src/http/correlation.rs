//! Correlation id propagation: echo a valid inbound `x-correlation-id`, otherwise generate one.

use axum::{
    extract::Request,
    http::{
        HeaderValue,
        header::{self, HeaderName},
    },
    middleware::Next,
    response::{IntoResponse, Response},
};
use tracing::Instrument;
use uuid::Uuid;

use super::problem::Problem;
use crate::{limits::CORRELATION_HEADER, validation::sanitize_correlation_id};

const FRAMEWORK_REJECTION_DETAIL: &str = "The request could not be processed.";

/// Correlation id of the current request, available as a request extension.
#[derive(Debug, Clone)]
pub struct CorrelationId(pub String);

/// Middleware that assigns the id, opens a tracing span with it and stamps the response header. Error responses
/// produced by the framework itself (bad JSON body, wrong method) are rewritten as problem details so no error
/// leaves without a correlation id in its body.
pub async fn propagate(mut request: Request, next: Next) -> Response {
    let id = request
        .headers()
        .get(CORRELATION_HEADER)
        .and_then(|value| value.to_str().ok())
        .and_then(sanitize_correlation_id)
        .unwrap_or_else(|| Uuid::new_v4().to_string());
    request.extensions_mut().insert(CorrelationId(id.clone()));

    let span =
        tracing::info_span!("request", correlation_id = %id, method = %request.method(), path = %request.uri().path());
    let mut response = as_problem(next.run(request).instrument(span).await, &id);
    if let Ok(value) = HeaderValue::from_str(&id) {
        response
            .headers_mut()
            .insert(HeaderName::from_static(CORRELATION_HEADER), value);
    }
    response
}

fn as_problem(response: Response, correlation_id: &str) -> Response {
    let status = response.status();
    // The handlers always answer with JSON; only the framework's own rejections are plain text or empty.
    let framework_rejection = response
        .headers()
        .get(header::CONTENT_TYPE)
        .is_none_or(|value| value.as_bytes().starts_with(b"text/plain"));
    if !(status.is_client_error() || status.is_server_error()) || !framework_rejection {
        return response;
    }

    let title = status.canonical_reason().unwrap_or("Error");
    let mut rewritten = Problem::new(status, title, FRAMEWORK_REJECTION_DETAIL, correlation_id).into_response();
    for (name, value) in response.headers() {
        if name != header::CONTENT_TYPE && name != header::CONTENT_LENGTH {
            rewritten.headers_mut().append(name.clone(), value.clone());
        }
    }
    rewritten
}
