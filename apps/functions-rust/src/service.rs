//! Orchestrates loading the BOM graph, costing it and caching the result.

use std::{
    collections::{HashMap, HashSet},
    time::Duration,
};

use serde::Serialize;

use crate::{
    domain::{
        error::DomainError,
        explode::{explode, explode_what_if},
        model::{BomCostResult, BomGraph, ProductId},
    },
    error::ServiceError,
    limits::{KEY_PREFIX, MAX_BOM_DEPTH},
    ports::{BomRepository, ResultCache},
    validation::CostRequest,
};

/// How the caller wants the cache used.
#[derive(Debug, Clone, Copy, PartialEq, Eq)]
pub enum CacheMode {
    /// Serve from the cache when possible.
    Use,
    /// Skip the read, recompute and overwrite the entry (`Cache-Control: no-cache`).
    Refresh,
}

/// What happened to the cache for one request.
#[derive(Debug, Clone, Copy, PartialEq, Eq, Serialize)]
#[serde(rename_all = "camelCase")]
pub enum CacheStatus {
    /// Served from the cache.
    Hit,
    /// Not cached; computed and stored.
    Miss,
    /// Cache read skipped and the entry rewritten.
    Refresh,
    /// What-if results are never cached.
    Skipped,
    /// Redis failed; the result was computed fresh.
    Unavailable,
}

impl CacheStatus {
    /// Header value for `X-Cache`.
    pub fn as_header(self) -> &'static str {
        match self {
            Self::Hit => "hit",
            Self::Miss => "miss",
            Self::Refresh => "refresh",
            Self::Skipped => "skipped",
            Self::Unavailable => "unavailable",
        }
    }
}

/// Reachability of the two read-path dependencies.
#[derive(Debug, Clone, Copy, PartialEq, Eq, Serialize)]
pub struct HealthReport {
    /// SQL Server answered.
    pub sql: bool,
    /// Redis answered.
    pub cache: bool,
}

/// Cache key for a baseline result.
pub fn cache_key(product_id: ProductId, quantity: u32) -> String {
    format!("{KEY_PREFIX}:{product_id}:{quantity}")
}

/// Costing service over a repository and a cache.
pub struct BomService<R, C> {
    repository: R,
    cache: C,
    cache_ttl: Duration,
}

impl<R: BomRepository, C: ResultCache> BomService<R, C> {
    /// Creates a service; `cache_ttl` applies to every entry it writes.
    pub fn new(repository: R, cache: C, cache_ttl: Duration) -> Self {
        Self {
            repository,
            cache,
            cache_ttl,
        }
    }

    /// Pings both dependencies concurrently; `true` means reachable.
    pub async fn health(&self) -> HealthReport {
        let (sql, cache) = tokio::join!(self.repository.ping(), self.cache.ping());
        for (name, outcome) in [("sql", &sql), ("cache", &cache)] {
            if let Err(error) = outcome {
                tracing::warn!(dependency = name, %error, "health check failed");
            }
        }
        HealthReport {
            sql: sql.is_ok(),
            cache: cache.is_ok(),
        }
    }

    /// Costs a request, using the cache for baseline (no-override) requests only.
    pub async fn cost(
        &self,
        request: &CostRequest,
        mode: CacheMode,
    ) -> Result<(BomCostResult, CacheStatus), ServiceError> {
        if !request.overrides.is_empty() {
            return Ok((self.compute(request).await?, CacheStatus::Skipped));
        }

        let key = cache_key(request.product_id, request.quantity);
        let mut status = match mode {
            CacheMode::Refresh => CacheStatus::Refresh,
            CacheMode::Use => CacheStatus::Miss,
        };
        if mode == CacheMode::Use {
            match self.cache.get(&key).await {
                Ok(Some(raw)) => match serde_json::from_str(&raw) {
                    Ok(result) => return Ok((result, CacheStatus::Hit)),
                    Err(error) => tracing::warn!(%key, %error, "discarding unreadable cache entry"),
                },
                Ok(None) => {}
                Err(error) => {
                    tracing::warn!(%key, %error, "cache read failed; computing fresh");
                    status = CacheStatus::Unavailable;
                }
            }
        }

        let result = self.compute(request).await?;
        if status != CacheStatus::Unavailable && !self.store(&key, &result).await {
            status = CacheStatus::Unavailable;
        }
        Ok((result, status))
    }

    /// Costs a request straight from the database, bypassing the cache entirely.
    pub async fn compute(&self, request: &CostRequest) -> Result<BomCostResult, ServiceError> {
        let graph = self.load_graph(request.product_id).await?;
        let result = if request.overrides.is_empty() {
            explode(&graph, request.quantity)?
        } else {
            explode_what_if(&graph, request.quantity, &request.overrides)?
        };
        Ok(result)
    }

    /// Writes `result` to the cache; `false` means it could not be stored.
    async fn store(&self, key: &str, result: &BomCostResult) -> bool {
        let raw = match serde_json::to_string(result) {
            Ok(raw) => raw,
            Err(error) => {
                tracing::error!(%key, %error, "could not serialize result for the cache");
                return false;
            }
        };
        match self.cache.set(key, &raw, self.cache_ttl).await {
            Ok(()) => true,
            Err(error) => {
                tracing::warn!(%key, %error, "cache write failed");
                false
            }
        }
    }

    /// Loads the BOM one level per query (no N+1), then the catalogue, labor and stock for every node.
    async fn load_graph(&self, root: ProductId) -> Result<BomGraph, ServiceError> {
        let mut edges: HashMap<ProductId, Vec<_>> = HashMap::new();
        let mut loaded: HashSet<ProductId> = HashSet::new();
        let mut frontier = vec![root];

        for _ in 0..=MAX_BOM_DEPTH {
            if frontier.is_empty() {
                break;
            }
            loaded.extend(frontier.iter().copied());
            let fetched = self.repository.components_of(&frontier).await?;
            let mut next: Vec<ProductId> = Vec::new();
            for edge in fetched {
                if !loaded.contains(&edge.component_id) && !next.contains(&edge.component_id) {
                    next.push(edge.component_id);
                }
                edges.entry(edge.assembly_id).or_default().push(edge);
            }
            frontier = next;
        }
        if !frontier.is_empty() {
            return Err(DomainError::DepthExceeded { max: MAX_BOM_DEPTH }.into());
        }

        let mut ids: Vec<ProductId> = loaded.into_iter().collect();
        ids.sort_unstable();
        let (products, labor, on_hand) = tokio::try_join!(
            self.repository.products(&ids),
            self.repository.labor_per_unit(&ids),
            self.repository.on_hand(&ids),
        )?;

        Ok(BomGraph {
            root,
            edges,
            products: products.into_iter().map(|p| (p.id, p)).collect(),
            labor,
            on_hand,
        })
    }
}
