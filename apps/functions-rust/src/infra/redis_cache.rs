//! Redis-backed [`ResultCache`].

use std::time::Duration;

use redis::{
    AsyncCommands,
    aio::{ConnectionManager, ConnectionManagerConfig},
};

use crate::{error::InfraError, ports::ResultCache};

/// Cache over a multiplexed, auto-reconnecting Redis connection.
#[derive(Clone)]
pub struct RedisCache {
    connection: ConnectionManager,
}

impl RedisCache {
    /// Prepares a cache for `url` (for example `redis://localhost:6380`) without touching the network, so the
    /// handler starts even when Redis is down. The connection is made, and re-made, on use.
    pub fn new(url: &str) -> Result<Self, InfraError> {
        let client = redis::Client::open(url).map_err(cache_error)?;
        let connection =
            ConnectionManager::new_lazy_with_config(client, ConnectionManagerConfig::new()).map_err(cache_error)?;
        Ok(Self { connection })
    }
}

fn cache_error(error: redis::RedisError) -> InfraError {
    InfraError::Cache(error.to_string())
}

impl ResultCache for RedisCache {
    async fn ping(&self) -> Result<(), InfraError> {
        let mut connection = self.connection.clone();
        redis::cmd("PING")
            .query_async::<String>(&mut connection)
            .await
            .map_err(cache_error)?;
        Ok(())
    }

    async fn get(&self, key: &str) -> Result<Option<String>, InfraError> {
        let mut connection = self.connection.clone();
        connection.get(key).await.map_err(cache_error)
    }

    async fn set(&self, key: &str, value: &str, ttl: Duration) -> Result<(), InfraError> {
        let mut connection = self.connection.clone();
        connection
            .set_ex::<_, _, ()>(key, value, ttl.as_secs())
            .await
            .map_err(cache_error)
    }
}
