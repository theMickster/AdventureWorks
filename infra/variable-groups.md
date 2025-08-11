# ADO Variable Groups — AdventureWorks

Two variable groups supply environment-specific configuration to Azure Pipelines. Create these manually in Azure DevOps under **Pipelines → Library**.

Key Vault–linked variables pull secrets at pipeline run time — no secrets stored in ADO.

---

## `AdventureWorks-Dev`

| Variable                      | Value                                                   | Source           |
| ----------------------------- | ------------------------------------------------------- | ---------------- |
| `appServiceNameApi`           | `mick-adventureworks-api-dev`                           | Plain text       |
| `appServiceNameWeb`           | `mick-adventureworks-web-dev`                           | Plain text       |
| `resourceGroup`               | `AdventureWorks-West-US-3`                              | Plain text       |
| `environmentName`             | `Development`                                           | Plain text       |
| `keyVaultUri`                 | `https://mick-aw-kv.vault.azure.net/`                   | Plain text       |
| `appInsightsConnectionString` | _(from Key Vault)_                                      | Key Vault linked |
| `sqlConnectionString`         | _(from Key Vault)_                                      | Key Vault linked |
| `entraAuthority`              | `https://login.microsoftonline.com/{tenantId}`          | Plain text       |
| `entraClientId`               | _(from Key Vault)_                                      | Key Vault linked |
| `entraRedirectUri`            | `https://mick-adventureworks-web-dev.azurewebsites.net` | Plain text       |
| `entraPostLogoutRedirectUri`  | `https://mick-adventureworks-web-dev.azurewebsites.net` | Plain text       |
| `entraApiScope`               | `api://{clientId}/access_via_group_assignments`         | Plain text       |
| `autoMapperLicenseKey`        | _(from Key Vault)_                                      | Key Vault linked |

> Replace `{tenantId}` and `{clientId}` with actual values when creating the group.

---

## `AdventureWorks-Prod`

| Variable                      | Value                                               | Source           |
| ----------------------------- | --------------------------------------------------- | ---------------- |
| `appServiceNameApi`           | `mick-adventureworks-api`                           | Plain text       |
| `appServiceNameWeb`           | `mick-adventureworks-web`                           | Plain text       |
| `resourceGroup`               | `AdventureWorks-West-US-3`                          | Plain text       |
| `environmentName`             | `Production`                                        | Plain text       |
| `keyVaultUri`                 | `https://mick-aw-kv.vault.azure.net/`               | Plain text       |
| `appInsightsConnectionString` | _(from Key Vault)_                                  | Key Vault linked |
| `sqlConnectionString`         | _(from Key Vault)_                                  | Key Vault linked |
| `entraAuthority`              | `https://login.microsoftonline.com/{tenantId}`      | Plain text       |
| `entraClientId`               | _(from Key Vault)_                                  | Key Vault linked |
| `entraRedirectUri`            | `https://mick-adventureworks-web.azurewebsites.net` | Plain text       |
| `entraPostLogoutRedirectUri`  | `https://mick-adventureworks-web.azurewebsites.net` | Plain text       |
| `entraApiScope`               | `api://{clientId}/access_via_group_assignments`     | Plain text       |
| `autoMapperLicenseKey`        | _(from Key Vault)_                                  | Key Vault linked |

> Production App Service names have no `-dev` suffix. Replace `{tenantId}` and `{clientId}` with production values.

---

## Key Vault Link Setup

1. In each variable group, click **Link secrets from an Azure key vault as variables**
2. Select the `mick-aw-kv` Key Vault
3. Map the 4 Key Vault–linked variables above to their corresponding secret names
4. The pipeline service connection needs **Key Vault Secrets User** role on the vault (already granted by Bicep for App Service identities — the pipeline service principal needs a separate role assignment)
