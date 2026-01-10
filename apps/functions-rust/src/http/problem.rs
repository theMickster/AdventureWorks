//! RFC 9457 problem details responses that always carry the correlation id.

use axum::{
    Json,
    http::{StatusCode, header},
    response::{IntoResponse, Response},
};
use serde::Serialize;

use crate::{domain::error::DomainError, error::ServiceError};

const PROBLEM_CONTENT_TYPE: &str = "application/problem+json";
const DEPENDENCY_DETAIL: &str = "A required dependency is unavailable. Retry shortly.";

/// An error response.
#[derive(Debug, Serialize)]
#[serde(rename_all = "camelCase")]
pub struct Problem {
    #[serde(skip)]
    status: StatusCode,
    title: &'static str,
    detail: String,
    correlation_id: String,
}

#[derive(Serialize)]
#[serde(rename_all = "camelCase")]
struct ProblemBody<'a> {
    #[serde(rename = "type")]
    kind: &'static str,
    status: u16,
    #[serde(flatten)]
    problem: &'a Problem,
}

impl Problem {
    /// Creates a problem with an explicit status, title and detail.
    pub fn new(status: StatusCode, title: &'static str, detail: impl Into<String>, correlation_id: &str) -> Self {
        Self {
            status,
            title,
            detail: detail.into(),
            correlation_id: correlation_id.to_owned(),
        }
    }

    /// Maps a service failure to a status. Dependency failures are logged but not echoed.
    pub fn from_service(error: &ServiceError, correlation_id: &str) -> Self {
        match error {
            ServiceError::Validation(error) => Self::new(
                StatusCode::BAD_REQUEST,
                "Invalid request",
                error.to_string(),
                correlation_id,
            ),
            ServiceError::Domain(error @ DomainError::ProductNotFound(_)) => Self::new(
                StatusCode::NOT_FOUND,
                "Product not found",
                error.to_string(),
                correlation_id,
            ),
            ServiceError::Domain(error) => Self::new(
                StatusCode::UNPROCESSABLE_ENTITY,
                "Bill of materials cannot be costed",
                error.to_string(),
                correlation_id,
            ),
            ServiceError::Infra(error) => {
                tracing::error!(%error, "dependency failure");
                Self::new(
                    StatusCode::SERVICE_UNAVAILABLE,
                    "Dependency unavailable",
                    DEPENDENCY_DETAIL,
                    correlation_id,
                )
            }
        }
    }
}

impl IntoResponse for Problem {
    fn into_response(self) -> Response {
        let body = ProblemBody {
            kind: "about:blank",
            status: self.status.as_u16(),
            problem: &self,
        };
        let mut response = (self.status, Json(&body)).into_response();
        response.headers_mut().insert(
            header::CONTENT_TYPE,
            header::HeaderValue::from_static(PROBLEM_CONTENT_TYPE),
        );
        response
    }
}
