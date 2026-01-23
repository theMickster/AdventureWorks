//! Custom handler entry point: wires the infrastructure and serves HTTP on the port the Functions host assigns.

use std::{net::Ipv4Addr, sync::Arc};

use aw_bom_cost::{
    config::Settings,
    http::{AppState, router},
    infra::{
        cosmos::CosmosResultStore, key_vault, memory_cache::MemoryCache, redis_cache::RedisCache, sql::SqlBomRepository,
    },
    limits::MEMORY_CACHE_MAX_ENTRIES,
    ports::ResultCache,
    service::BomService,
};
use tokio::{net::TcpListener, signal};
use tracing_subscriber::EnvFilter;

#[tokio::main]
async fn main() -> Result<(), Box<dyn std::error::Error>> {
    tracing_subscriber::fmt()
        .with_env_filter(EnvFilter::try_from_default_env().unwrap_or_else(|_| EnvFilter::new("info")))
        .json()
        .init();

    let settings = key_vault::settings_from_env().await?;
    let repository = SqlBomRepository::connect(&settings.sql_connection_string).await?;
    let store = CosmosResultStore::new(&settings.cosmos)?;

    match &settings.redis_url {
        Some(url) => {
            tracing::info!("caching results in Redis");
            serve(repository, RedisCache::new(url)?, store, &settings).await
        }
        None => {
            tracing::info!("no Redis configured; caching results in process");
            serve(repository, MemoryCache::new(MEMORY_CACHE_MAX_ENTRIES), store, &settings).await
        }
    }
}

/// Serves the handler over `cache`, which is either Redis or the in-process cache.
async fn serve<C: ResultCache + 'static>(
    repository: SqlBomRepository,
    cache: C,
    store: CosmosResultStore,
    settings: &Settings,
) -> Result<(), Box<dyn std::error::Error>> {
    let state = Arc::new(AppState {
        service: BomService::new(repository, cache, settings.cache_ttl),
        store,
    });
    let listener = TcpListener::bind((Ipv4Addr::LOCALHOST, settings.port)).await?;
    tracing::info!(port = settings.port, "custom handler listening");
    axum::serve(listener, router(state))
        .with_graceful_shutdown(shutdown_signal())
        .await?;
    Ok(())
}

async fn shutdown_signal() {
    if let Err(error) = signal::ctrl_c().await {
        tracing::error!(%error, "could not listen for shutdown signal");
    }
}
