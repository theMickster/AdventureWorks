//! Service Bus batch processing: cost every `(productId, quantity)` pair and upsert the results.

use serde::{Deserialize, Serialize};
use time::{OffsetDateTime, format_description::well_known::Rfc3339};

use crate::{
    domain::model::{BomCostResult, ProductId},
    error::ServiceError,
    limits::MAX_BATCH_ITEMS,
    ports::{BomRepository, ResultCache, ResultStore},
    service::{BomService, cache_key},
    validation::{CostRequest, ValidationError, check_quantity},
};

/// Body of a `bom-batch-requests` message.
#[derive(Debug, Clone, PartialEq, Deserialize)]
#[serde(rename_all = "camelCase", deny_unknown_fields)]
pub struct BatchRequest {
    /// Pairs to cost, 1 to 100 of them.
    pub items: Vec<BatchItem>,
}

/// One pair to cost.
#[derive(Debug, Clone, PartialEq, Deserialize)]
#[serde(rename_all = "camelCase", deny_unknown_fields)]
pub struct BatchItem {
    /// Product to cost; must be positive.
    pub product_id: ProductId,
    /// Units to build, 1 to 10,000.
    pub quantity: u32,
}

/// Cosmos DB document holding one costed pair. The id is deterministic, so redelivery overwrites.
#[derive(Debug, Clone, PartialEq, Serialize, Deserialize)]
#[serde(rename_all = "camelCase")]
pub struct ResultDocument {
    /// Deterministic id, `bom:{productId}:{quantity}`.
    pub id: String,
    /// Partition key.
    pub product_id: ProductId,
    /// Units the result was costed for.
    pub quantity: u32,
    /// UTC time of the computation, RFC 3339.
    pub computed_at: String,
    /// Correlation id of the message that produced the document.
    pub correlation_id: String,
    /// The cost and feasibility result.
    pub result: BomCostResult,
}

/// A pair that could not be processed.
#[derive(Debug)]
pub struct BatchFailure {
    /// Position of the pair in the message.
    pub index: usize,
    /// Why the pair failed.
    pub error: ServiceError,
}

/// Outcome of one message.
#[derive(Debug, Default)]
pub struct BatchOutcome {
    /// Pairs costed and stored.
    pub succeeded: usize,
    /// Pairs that failed; empty means the message succeeded.
    pub failures: Vec<BatchFailure>,
}

/// Parses and size-checks a message body.
pub fn parse_batch(body: &str) -> Result<BatchRequest, BatchParseError> {
    let request: BatchRequest = serde_json::from_str(body).map_err(BatchParseError::Json)?;
    if request.items.is_empty() || request.items.len() > MAX_BATCH_ITEMS {
        return Err(BatchParseError::Invalid(ValidationError::BatchSize));
    }
    Ok(request)
}

/// A message body that is not a valid batch.
#[derive(Debug, thiserror::Error)]
pub enum BatchParseError {
    /// The body is not JSON of the expected shape.
    #[error("message body is not a valid batch: {0}")]
    Json(serde_json::Error),
    /// The body parsed but breaks a limit.
    #[error(transparent)]
    Invalid(#[from] ValidationError),
}

/// Costs every pair and upserts each success. A failing pair never stops the others; the caller
/// fails the message when `failures` is not empty so the host redelivers it.
pub async fn process_batch<R, C, S>(
    service: &BomService<R, C>,
    store: &S,
    request: &BatchRequest,
    correlation_id: &str,
) -> BatchOutcome
where
    R: BomRepository,
    C: ResultCache,
    S: ResultStore,
{
    let mut outcome = BatchOutcome::default();
    for (index, item) in request.items.iter().enumerate() {
        match process_item(service, store, item, correlation_id).await {
            Ok(()) => outcome.succeeded += 1,
            Err(error) => {
                tracing::error!(index, product_id = item.product_id, quantity = item.quantity, %error, "batch item failed");
                outcome.failures.push(BatchFailure { index, error });
            }
        }
    }
    outcome
}

async fn process_item<R, C, S>(
    service: &BomService<R, C>,
    store: &S,
    item: &BatchItem,
    correlation_id: &str,
) -> Result<(), ServiceError>
where
    R: BomRepository,
    C: ResultCache,
    S: ResultStore,
{
    if item.product_id <= 0 {
        return Err(ValidationError::ProductId.into());
    }
    let request = CostRequest {
        product_id: item.product_id,
        quantity: check_quantity(item.quantity)?,
        overrides: Default::default(),
    };
    let result = service.compute(&request).await?;
    let computed_at = OffsetDateTime::now_utc()
        .format(&Rfc3339)
        .map_err(|error| crate::error::InfraError::Config(error.to_string()))?;
    let document = ResultDocument {
        id: cache_key(item.product_id, item.quantity),
        product_id: item.product_id,
        quantity: item.quantity,
        computed_at,
        correlation_id: correlation_id.to_owned(),
        result,
    };
    store.upsert(&document).await?;
    Ok(())
}
