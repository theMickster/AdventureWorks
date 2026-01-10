//! Boundary limits and fixed names shared by validation, configuration and infrastructure.

use std::time::Duration;

/// Deepest BOM level the engine will expand. AdventureWorks tops out at 4; the headroom guards against bad data.
pub const MAX_BOM_DEPTH: usize = 10;
/// Cap on expanded tree nodes. Shared sub-assemblies are expanded once per use, so fan-out can multiply.
pub const MAX_TREE_NODES: usize = 5_000;
/// Smallest build quantity accepted.
pub const MIN_QUANTITY: u32 = 1;
/// Largest build quantity accepted.
pub const MAX_QUANTITY: u32 = 10_000;
/// Maximum number of what-if price overrides in one request.
pub const MAX_OVERRIDES: usize = 50;
/// Largest unit price accepted in a what-if override.
pub const MAX_OVERRIDE_PRICE: u32 = 1_000_000;
/// Maximum number of `(productId, quantity)` pairs in one batch message.
pub const MAX_BATCH_ITEMS: usize = 100;
/// Longest inbound correlation ID that is echoed back.
pub const MAX_CORRELATION_ID_LEN: usize = 64;
/// Decimal places kept on every monetary and quantity figure.
pub const DECIMAL_PLACES: u32 = 4;
/// Rows per `IN (...)` list. Stays far below SQL Server's 2,100 parameter ceiling.
pub const SQL_IN_CHUNK: usize = 500;
/// Upper bound on pooled SQL connections.
pub const SQL_POOL_MAX_SIZE: u32 = 8;
/// How long a caller waits for a pooled SQL connection.
pub const SQL_POOL_TIMEOUT: Duration = Duration::from_secs(5);
/// How long an idle pooled SQL connection is kept before it is closed.
pub const SQL_POOL_IDLE_TIMEOUT: Duration = Duration::from_secs(300);
/// Cache time-to-live applied when `BomCache__TtlSeconds` is not set.
pub const DEFAULT_CACHE_TTL: Duration = Duration::from_secs(300);
/// Port used when `FUNCTIONS_CUSTOMHANDLER_PORT` is not set (standalone runs).
pub const DEFAULT_PORT: u16 = 7071;

/// Prefix of every Redis and Cosmos key for a cost result.
pub const KEY_PREFIX: &str = "bom";
/// Query-string parameter carrying one what-if override as `componentId:price`.
pub const OVERRIDE_PARAM: &str = "override";
/// Query-string parameter carrying the build quantity.
pub const QUANTITY_PARAM: &str = "quantity";
/// Query-string parameter the Functions host uses for function keys; consumed by the host, ignored by validation.
pub const FUNCTION_KEY_PARAM: &str = "code";
/// Inbound and outbound correlation header.
pub const CORRELATION_HEADER: &str = "x-correlation-id";
/// Response header reporting how the cache was used.
pub const CACHE_HEADER: &str = "x-cache";
/// Region hint used when `Cosmos__PreferredRegion` is not set. Set it to the account's region in Azure.
pub const DEFAULT_COSMOS_PREFERRED_REGION: &str = "East US";
