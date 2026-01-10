//! Seams between the service and its infrastructure, so the engine can be tested with in-memory fakes.

use std::{collections::HashMap, future::Future, sync::Arc, time::Duration};

use rust_decimal::Decimal;

use crate::{
    domain::model::{BomEdge, ProductId, ProductInfo},
    error::InfraError,
};

/// Read-only access to the AdventureWorks tables the engine needs.
pub trait BomRepository: Send + Sync {
    /// Cheap round trip proving the database is reachable.
    fn ping(&self) -> impl Future<Output = Result<(), InfraError>> + Send;
    /// Current BOM rows whose assembly is one of `assemblies`.
    fn components_of(&self, assemblies: &[ProductId]) -> impl Future<Output = Result<Vec<BomEdge>, InfraError>> + Send;
    /// Catalogue data for `ids`; unknown ids are absent from the result.
    fn products(&self, ids: &[ProductId]) -> impl Future<Output = Result<Vec<ProductInfo>, InfraError>> + Send;
    /// Planned routing labor per unit for `ids`; products without routing are absent.
    fn labor_per_unit(
        &self,
        ids: &[ProductId],
    ) -> impl Future<Output = Result<HashMap<ProductId, Decimal>, InfraError>> + Send;
    /// Quantity on hand summed over all locations; products without stock are absent.
    fn on_hand(
        &self,
        ids: &[ProductId],
    ) -> impl Future<Output = Result<HashMap<ProductId, Decimal>, InfraError>> + Send;
}

/// String cache with a per-entry time-to-live.
pub trait ResultCache: Send + Sync {
    /// Cheap round trip proving the cache is reachable.
    fn ping(&self) -> impl Future<Output = Result<(), InfraError>> + Send;
    /// Returns the value for `key`, if present.
    fn get(&self, key: &str) -> impl Future<Output = Result<Option<String>, InfraError>> + Send;
    /// Stores `value` under `key` for `ttl`.
    fn set(&self, key: &str, value: &str, ttl: Duration) -> impl Future<Output = Result<(), InfraError>> + Send;
}

/// Durable sink for batch results.
pub trait ResultStore: Send + Sync {
    /// Inserts or replaces the document with `document.id`.
    fn upsert(&self, document: &crate::batch::ResultDocument) -> impl Future<Output = Result<(), InfraError>> + Send;
}

/// Shared handles to a repository are repositories, so one adapter can serve several owners.
impl<T: BomRepository> BomRepository for Arc<T> {
    fn ping(&self) -> impl Future<Output = Result<(), InfraError>> + Send {
        (**self).ping()
    }

    fn components_of(&self, assemblies: &[ProductId]) -> impl Future<Output = Result<Vec<BomEdge>, InfraError>> + Send {
        (**self).components_of(assemblies)
    }

    fn products(&self, ids: &[ProductId]) -> impl Future<Output = Result<Vec<ProductInfo>, InfraError>> + Send {
        (**self).products(ids)
    }

    fn labor_per_unit(
        &self,
        ids: &[ProductId],
    ) -> impl Future<Output = Result<HashMap<ProductId, Decimal>, InfraError>> + Send {
        (**self).labor_per_unit(ids)
    }

    fn on_hand(
        &self,
        ids: &[ProductId],
    ) -> impl Future<Output = Result<HashMap<ProductId, Decimal>, InfraError>> + Send {
        (**self).on_hand(ids)
    }
}

/// Shared handles to a cache are caches.
impl<T: ResultCache> ResultCache for Arc<T> {
    fn ping(&self) -> impl Future<Output = Result<(), InfraError>> + Send {
        (**self).ping()
    }

    fn get(&self, key: &str) -> impl Future<Output = Result<Option<String>, InfraError>> + Send {
        (**self).get(key)
    }

    fn set(&self, key: &str, value: &str, ttl: Duration) -> impl Future<Output = Result<(), InfraError>> + Send {
        (**self).set(key, value, ttl)
    }
}

/// Shared handles to a store are stores.
impl<T: ResultStore> ResultStore for Arc<T> {
    fn upsert(&self, document: &crate::batch::ResultDocument) -> impl Future<Output = Result<(), InfraError>> + Send {
        (**self).upsert(document)
    }
}
