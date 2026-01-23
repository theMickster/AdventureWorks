//! In-process [`ResultCache`], used when no Redis is configured.
//!
//! Entries live in the handler's memory, so they are per instance and lost on restart. Like Redis here, expiry is
//! TTL-only: a result can be one TTL stale.

use std::{
    collections::HashMap,
    sync::{Mutex, MutexGuard, PoisonError},
    time::{Duration, Instant},
};

use crate::{error::InfraError, ports::ResultCache};

struct Entry {
    value: String,
    expires_at: Instant,
}

/// Bounded string cache with a per-entry time-to-live.
pub struct MemoryCache {
    entries: Mutex<HashMap<String, Entry>>,
    max_entries: usize,
}

impl MemoryCache {
    /// Creates an empty cache that holds at most `max_entries` live entries.
    pub fn new(max_entries: usize) -> Self {
        Self {
            entries: Mutex::new(HashMap::new()),
            max_entries,
        }
    }

    /// The lock is never held across an await, and cached results are disposable, so a poisoned lock is recovered
    /// instead of failing every later request.
    fn entries(&self) -> MutexGuard<'_, HashMap<String, Entry>> {
        self.entries.lock().unwrap_or_else(PoisonError::into_inner)
    }
}

impl ResultCache for MemoryCache {
    async fn ping(&self) -> Result<(), InfraError> {
        Ok(())
    }

    async fn get(&self, key: &str) -> Result<Option<String>, InfraError> {
        let now = Instant::now();
        let mut entries = self.entries();
        let expired = match entries.get(key) {
            Some(entry) if entry.expires_at > now => return Ok(Some(entry.value.clone())),
            Some(_) => true,
            None => false,
        };
        if expired {
            entries.remove(key);
        }
        Ok(None)
    }

    async fn set(&self, key: &str, value: &str, ttl: Duration) -> Result<(), InfraError> {
        let now = Instant::now();
        let expires_at = now
            .checked_add(ttl)
            .ok_or_else(|| InfraError::Cache("cache TTL is too large".to_owned()))?;
        let mut entries = self.entries();
        entries.retain(|_, entry| entry.expires_at > now);
        if !entries.contains_key(key) && entries.len() >= self.max_entries {
            let soonest = entries
                .iter()
                .min_by_key(|(_, entry)| entry.expires_at)
                .map(|(key, _)| key.clone());
            if let Some(soonest) = soonest {
                entries.remove(&soonest);
            }
        }
        entries.insert(
            key.to_owned(),
            Entry {
                value: value.to_owned(),
                expires_at,
            },
        );
        Ok(())
    }
}

#[cfg(test)]
mod tests {
    use super::*;

    const LONG: Duration = Duration::from_secs(3_600);

    #[tokio::test]
    async fn a_stored_value_is_returned_until_it_expires() {
        let cache = MemoryCache::new(10);

        cache.set("bom:1:1", "result", LONG).await.unwrap();

        assert_eq!(cache.get("bom:1:1").await.unwrap().as_deref(), Some("result"));
    }

    #[tokio::test]
    async fn an_unknown_key_is_a_miss() {
        let cache = MemoryCache::new(10);

        assert_eq!(cache.get("bom:1:1").await.unwrap(), None);
    }

    #[tokio::test]
    async fn an_expired_entry_is_a_miss_and_is_dropped() {
        let cache = MemoryCache::new(10);

        cache.set("bom:1:1", "stale", Duration::ZERO).await.unwrap();

        assert_eq!(cache.get("bom:1:1").await.unwrap(), None);
        assert!(cache.entries().is_empty());
    }

    #[tokio::test]
    async fn setting_a_key_again_replaces_the_value() {
        let cache = MemoryCache::new(10);

        cache.set("bom:1:1", "first", LONG).await.unwrap();
        cache.set("bom:1:1", "second", LONG).await.unwrap();

        assert_eq!(cache.get("bom:1:1").await.unwrap().as_deref(), Some("second"));
    }

    #[tokio::test]
    async fn a_full_cache_evicts_the_entry_that_expires_soonest() {
        let cache = MemoryCache::new(2);

        cache.set("soonest", "a", Duration::from_secs(10)).await.unwrap();
        cache.set("middle", "b", Duration::from_secs(20)).await.unwrap();
        cache.set("latest", "c", Duration::from_secs(30)).await.unwrap();

        assert_eq!(cache.get("soonest").await.unwrap(), None);
        assert_eq!(cache.get("middle").await.unwrap().as_deref(), Some("b"));
        assert_eq!(cache.get("latest").await.unwrap().as_deref(), Some("c"));
    }

    #[tokio::test]
    async fn overwriting_a_key_in_a_full_cache_evicts_nothing() {
        let cache = MemoryCache::new(2);

        cache.set("a", "1", LONG).await.unwrap();
        cache.set("b", "2", LONG).await.unwrap();
        cache.set("a", "3", LONG).await.unwrap();

        assert_eq!(cache.get("a").await.unwrap().as_deref(), Some("3"));
        assert_eq!(cache.get("b").await.unwrap().as_deref(), Some("2"));
    }

    #[tokio::test]
    async fn a_ttl_too_large_to_represent_is_an_error_not_a_panic() {
        let cache = MemoryCache::new(10);

        let error = cache.set("bom:1:1", "result", Duration::MAX).await.unwrap_err();

        assert!(matches!(error, InfraError::Cache(message) if message.contains("TTL")));
    }

    #[tokio::test]
    async fn it_is_always_reachable() {
        assert!(MemoryCache::new(10).ping().await.is_ok());
    }
}
