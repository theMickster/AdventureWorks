//! Custom handler entry point: wires the infrastructure and serves HTTP on the port the Functions host assigns.

use std::{net::Ipv4Addr, sync::Arc};

use aw_bom_cost::{
    http::{AppState, router},
    infra::{cosmos::CosmosResultStore, key_vault, redis_cache::RedisCache, sql::SqlBomRepository},
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
    let cache = RedisCache::new(&settings.redis_url)?;
    let store = CosmosResultStore::new(&settings.cosmos)?;

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
