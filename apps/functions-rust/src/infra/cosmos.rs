//! Cosmos DB writer for batch results, built on `azure_data_cosmos` 1.0.0-beta.1.

use std::sync::Arc;

use azure_core::credentials::TokenCredential;
use azure_data_cosmos::{AccountEndpoint, AccountReference, ContainerClient, CosmosClient, RoutingStrategy};
use azure_identity::{DeveloperToolsCredential, ManagedIdentityCredential};
use tokio::sync::OnceCell;

use crate::{
    batch::ResultDocument, config::CosmosSettings, domain::model::ProductId, error::InfraError, ports::ResultStore,
};

/// Set by the Functions host when a managed identity is enabled.
const ENV_IDENTITY_ENDPOINT: &str = "IDENTITY_ENDPOINT";

/// Upserts [`ResultDocument`]s into one container.
///
/// Construction only validates settings and builds the credential. The client is built on first use, because
/// building it contacts the account. The container must already exist (see `infra/AZURE_FUNCTIONS_SETUP.md`). A Cosmos outage therefore fails batch messages (which the host retries) instead of the handler.
#[derive(Clone)]
pub struct CosmosResultStore {
    inner: Arc<Inner>,
}

struct Inner {
    account: AccountReference,
    database: String,
    container_name: String,
    preferred_region: String,
    container: OnceCell<ContainerClient>,
}

impl CosmosResultStore {
    /// Validates `settings` and prepares the store without contacting Cosmos DB.
    pub fn new(settings: &CosmosSettings) -> Result<Self, InfraError> {
        let endpoint: AccountEndpoint = settings
            .endpoint
            .parse()
            .map_err(|error| InfraError::Config(format!("Cosmos__Endpoint is not a valid endpoint: {error}")))?;
        let account = AccountReference::with_credential(endpoint, entra_credential()?);
        Ok(Self {
            inner: Arc::new(Inner {
                account,
                database: settings.database.clone(),
                container_name: settings.container.clone(),
                preferred_region: settings.preferred_region.clone(),
                container: OnceCell::new(),
            }),
        })
    }

    /// The container client, built on first successful call. A failed attempt is retried on the next call.
    async fn container(&self) -> Result<&ContainerClient, InfraError> {
        let inner = &*self.inner;
        inner
            .container
            .get_or_try_init(|| async {
                let client = CosmosClient::builder()
                    .build(
                        inner.account.clone(),
                        RoutingStrategy::ProximityTo(inner.preferred_region.clone().into()),
                    )
                    .await
                    .map_err(store_error)?;
                client
                    .database_client(inner.database.as_str())
                    .container_client(inner.container_name.as_str(), None)
                    .await
                    .map_err(store_error)
            })
            .await
    }
}

pub(crate) fn entra_credential() -> Result<Arc<dyn TokenCredential>, InfraError> {
    let credential: Arc<dyn TokenCredential> = if std::env::var_os(ENV_IDENTITY_ENDPOINT).is_some() {
        ManagedIdentityCredential::new(None).map_err(|error| InfraError::Config(error.to_string()))?
    } else {
        DeveloperToolsCredential::new(None).map_err(|error| InfraError::Config(error.to_string()))?
    };
    Ok(credential)
}

fn store_error(error: azure_data_cosmos::CosmosError) -> InfraError {
    InfraError::Store(error.to_string())
}

impl CosmosResultStore {
    /// Reads one document back by its partition key and id, for verification and diagnostics.
    pub async fn read(&self, product_id: ProductId, id: &str) -> Result<ResultDocument, InfraError> {
        self.container()
            .await?
            .read_item(product_id, id, None)
            .await
            .map_err(store_error)?
            .into_model()
            .map_err(store_error)
    }
}

impl ResultStore for CosmosResultStore {
    async fn upsert(&self, document: &ResultDocument) -> Result<(), InfraError> {
        self.container()
            .await?
            .upsert_item(document.product_id, &document.id, document, None)
            .await
            .map_err(store_error)?;
        Ok(())
    }
}
