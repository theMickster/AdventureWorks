//! Adapters for SQL Server, Redis (or the in-process cache that stands in for it) and Cosmos DB.

pub mod cosmos;
pub mod key_vault;
pub mod memory_cache;
pub mod redis_cache;
pub mod sql;
