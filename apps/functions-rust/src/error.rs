//! Error types for infrastructure and the service layer.

use thiserror::Error;

use crate::{domain::error::DomainError, validation::ValidationError};

/// A dependency (SQL Server, Redis, Cosmos DB) or its configuration failed.
#[derive(Debug, Error)]
pub enum InfraError {
    /// Query or connection failure.
    #[error("SQL Server: {0}")]
    Sql(String),
    /// Redis failure.
    #[error("Redis: {0}")]
    Cache(String),
    /// Cosmos DB failure.
    #[error("Cosmos DB: {0}")]
    Store(String),
    /// Missing or malformed setting.
    #[error("configuration: {0}")]
    Config(String),
}

/// Anything that can fail while serving a costing request.
#[derive(Debug, Error)]
pub enum ServiceError {
    /// The request broke a boundary rule.
    #[error(transparent)]
    Validation(#[from] ValidationError),
    /// The BOM cannot be costed.
    #[error(transparent)]
    Domain(#[from] DomainError),
    /// A dependency failed.
    #[error(transparent)]
    Infra(#[from] InfraError),
}
