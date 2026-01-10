//! Reads the Cosmos DB account endpoint from Azure Key Vault, the Rust counterpart of
//! `AzureKeyVaultDataHelper` and `SecretHelper` in the AdventureWorks API.
//!
//! The vault is reached with Microsoft Entra ID (managed identity in Azure, developer login locally), and the
//! Cosmos account key stored in the same vault is deliberately never read.

use std::collections::HashMap;

use azure_security_keyvault_secrets::SecretClient;

use crate::{
    config::{ENV_COSMOS_ENDPOINT, Settings},
    error::InfraError,
    infra::cosmos::entra_credential,
};

/// Setting that holds the vault address, the same key (`KeyVault:VaultUri`) the .NET apps use.
pub const ENV_VAULT_URI: &str = "KeyVault__VaultUri";

/// Vault secret name, and the setting its value supplies.
const SECRET_SETTINGS: [(&str, &str); 1] = [("AzureCosmosDbAccountUri", ENV_COSMOS_ENDPOINT)];

/// Reads [`Settings`] from the environment. When `KeyVault__VaultUri` is set, the Cosmos endpoint comes from the
/// vault and takes precedence over the environment.
pub async fn settings_from_env() -> Result<Settings, InfraError> {
    let vault_uri = std::env::var(ENV_VAULT_URI).ok().filter(|uri| !uri.trim().is_empty());
    let vault_values = match &vault_uri {
        Some(uri) => cosmos_settings(uri).await?,
        None => HashMap::new(),
    };
    Settings::from_lookup(|key| vault_values.get(key).cloned().or_else(|| std::env::var(key).ok()))
}

/// Fetches the Cosmos secrets from the vault at `vault_uri`, keyed by the setting each one supplies.
pub async fn cosmos_settings(vault_uri: &str) -> Result<HashMap<String, String>, InfraError> {
    if !vault_uri.starts_with("https://") {
        return Err(InfraError::Config(format!("{ENV_VAULT_URI} must be an https URL")));
    }
    let client = SecretClient::new(vault_uri, entra_credential()?, None)
        .map_err(|error| InfraError::Config(format!("{ENV_VAULT_URI} is not usable: {error}")))?;
    let mut values = HashMap::new();
    for (secret_name, setting) in SECRET_SETTINGS {
        values.insert(setting.to_owned(), required_secret(&client, secret_name).await?);
    }
    Ok(values)
}

async fn required_secret(client: &SecretClient, name: &str) -> Result<String, InfraError> {
    let secret = client
        .get_secret(name, None)
        .await
        .map_err(|error| InfraError::Config(format!("could not read Key Vault secret '{name}': {error}")))?
        .into_model()
        .map_err(|error| InfraError::Config(format!("could not read Key Vault secret '{name}': {error}")))?;
    secret
        .value
        .filter(|value| !value.trim().is_empty())
        .ok_or_else(|| InfraError::Config(format!("Key Vault secret '{name}' is missing or empty")))
}

#[cfg(test)]
mod tests {
    use super::*;

    #[tokio::test]
    async fn rejects_a_vault_uri_that_is_not_https() {
        let error = cosmos_settings("http://vault.example").await.unwrap_err();
        assert!(matches!(error, InfraError::Config(message) if message.contains("https")));
    }
}
