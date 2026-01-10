//! Runtime configuration read from environment variables (app settings in Azure, `local.settings.json` locally).

use std::time::Duration;

use crate::{
    error::InfraError,
    limits::{DEFAULT_CACHE_TTL, DEFAULT_COSMOS_PREFERRED_REGION, DEFAULT_PORT},
};

const ENV_PORT: &str = "FUNCTIONS_CUSTOMHANDLER_PORT";
const ENV_SQL: &str = "ConnectionStrings__AdventureWorks";
const ENV_REDIS: &str = "ConnectionStrings__Redis";
const ENV_CACHE_TTL: &str = "BomCache__TtlSeconds";
/// Cosmos account endpoint setting.
pub const ENV_COSMOS_ENDPOINT: &str = "Cosmos__Endpoint";
/// Cosmos database name setting.
const ENV_COSMOS_DATABASE: &str = "Cosmos__Database";
const ENV_COSMOS_CONTAINER: &str = "Cosmos__Container";
const ENV_COSMOS_REGION: &str = "Cosmos__PreferredRegion";

/// Cosmos DB settings.
pub struct CosmosSettings {
    /// Account endpoint, for example `https://<account>.documents.azure.com:443/`. Always reached with Microsoft Entra ID.
    pub endpoint: String,
    /// Database name.
    pub database: String,
    /// Container name.
    pub container: String,
    /// Region the SDK prefers for routing, for example `West US 3`. Should be a region of the account.
    pub preferred_region: String,
}

/// All settings the handler needs. Deliberately not `Debug`: it holds secrets.
pub struct Settings {
    /// Port the Functions host assigned to this handler.
    pub port: u16,
    /// ADO.NET connection string for AdventureWorks.
    pub sql_connection_string: String,
    /// Redis URL, for example `redis://localhost:6380`.
    pub redis_url: String,
    /// Time-to-live of cached results.
    pub cache_ttl: Duration,
    /// Cosmos DB settings.
    pub cosmos: CosmosSettings,
}

impl Settings {
    /// Reads settings from the process environment.
    pub fn from_env() -> Result<Self, InfraError> {
        Self::from_lookup(|key| std::env::var(key).ok())
    }

    /// Reads settings through `lookup`; split out so tests need no real environment.
    pub fn from_lookup(lookup: impl Fn(&str) -> Option<String>) -> Result<Self, InfraError> {
        let required = |key: &str| {
            lookup(key)
                .filter(|value| !value.trim().is_empty())
                .ok_or_else(|| InfraError::Config(format!("{key} is not set")))
        };
        let parsed = |key: &str| -> Result<Option<u64>, InfraError> {
            lookup(key)
                .map(|raw| {
                    raw.parse()
                        .map_err(|_| InfraError::Config(format!("{key} must be a whole number")))
                })
                .transpose()
        };

        let port = match parsed(ENV_PORT)? {
            Some(port) => u16::try_from(port).map_err(|_| InfraError::Config(format!("{ENV_PORT} is out of range")))?,
            None => DEFAULT_PORT,
        };
        let cache_ttl = match parsed(ENV_CACHE_TTL)? {
            Some(0) => return Err(InfraError::Config(format!("{ENV_CACHE_TTL} must be greater than zero"))),
            Some(seconds) => Duration::from_secs(seconds),
            None => DEFAULT_CACHE_TTL,
        };

        Ok(Self {
            port,
            sql_connection_string: required(ENV_SQL)?,
            redis_url: required(ENV_REDIS)?,
            cache_ttl,
            cosmos: CosmosSettings {
                endpoint: required(ENV_COSMOS_ENDPOINT)?,
                database: required(ENV_COSMOS_DATABASE)?,
                container: required(ENV_COSMOS_CONTAINER)?,
                preferred_region: lookup(ENV_COSMOS_REGION)
                    .filter(|region| !region.trim().is_empty())
                    .unwrap_or_else(|| DEFAULT_COSMOS_PREFERRED_REGION.to_owned()),
            },
        })
    }
}

#[cfg(test)]
mod tests {
    use std::collections::HashMap;

    use super::*;

    fn lookup(extra: &[(&str, &str)]) -> impl Fn(&str) -> Option<String> {
        let mut values: HashMap<String, String> = [
            (ENV_SQL, "Server=tcp:db,1433;Database=AdventureWorks"),
            (ENV_REDIS, "redis://localhost:6380"),
            (ENV_COSMOS_ENDPOINT, "https://example.documents.azure.com:443/"),
            (ENV_COSMOS_DATABASE, "PlatformDatabases"),
            (ENV_COSMOS_CONTAINER, "bom-cost-results-local"),
        ]
        .into_iter()
        .map(|(k, v)| (k.to_owned(), v.to_owned()))
        .collect();
        values.extend(extra.iter().map(|(k, v)| ((*k).to_owned(), (*v).to_owned())));
        move |key| values.get(key).cloned()
    }

    #[test]
    fn defaults_apply_when_optional_values_are_absent() {
        let settings = Settings::from_lookup(lookup(&[])).unwrap();

        assert_eq!(settings.port, DEFAULT_PORT);
        assert_eq!(settings.cache_ttl, DEFAULT_CACHE_TTL);
        assert_eq!(settings.cosmos.database, "PlatformDatabases");
        assert_eq!(settings.cosmos.container, "bom-cost-results-local");
        assert_eq!(settings.cosmos.preferred_region, DEFAULT_COSMOS_PREFERRED_REGION);
    }

    #[test]
    fn preferred_region_is_configurable_and_blank_falls_back_to_the_default() {
        let set = Settings::from_lookup(lookup(&[(ENV_COSMOS_REGION, "West US 3")])).unwrap();
        let blank = Settings::from_lookup(lookup(&[(ENV_COSMOS_REGION, "  ")])).unwrap();

        assert_eq!(set.cosmos.preferred_region, "West US 3");
        assert_eq!(blank.cosmos.preferred_region, DEFAULT_COSMOS_PREFERRED_REGION);
    }

    #[test]
    fn explicit_values_override_defaults() {
        let settings = Settings::from_lookup(lookup(&[(ENV_PORT, "7075"), (ENV_CACHE_TTL, "60")])).unwrap();

        assert_eq!(settings.port, 7075);
        assert_eq!(settings.cache_ttl, Duration::from_secs(60));
    }

    #[test]
    fn missing_required_values_name_the_setting() {
        for key in [
            ENV_SQL,
            ENV_REDIS,
            ENV_COSMOS_ENDPOINT,
            ENV_COSMOS_DATABASE,
            ENV_COSMOS_CONTAINER,
        ] {
            let lookup = lookup(&[]);
            let error = Settings::from_lookup(|k| if k == key { None } else { lookup(k) })
                .err()
                .unwrap();
            assert!(error.to_string().contains(key), "{error}");
        }
    }

    #[test]
    fn malformed_numbers_are_rejected() {
        for (key, value) in [
            (ENV_PORT, "abc"),
            (ENV_PORT, "70000"),
            (ENV_CACHE_TTL, "-1"),
            (ENV_CACHE_TTL, "0"),
        ] {
            assert!(Settings::from_lookup(lookup(&[(key, value)])).is_err(), "{key}={value}");
        }
    }
}
