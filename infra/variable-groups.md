# ADO Variable Groups — AdventureWorks

Four variable groups supply environment-specific configuration to Azure Pipelines. ADO does not support mixing plain text and Key Vault–linked variables in a single group, so each environment uses two groups: one for plain text config, one for Key Vault secrets.

Create these manually in Azure DevOps under **Pipelines → Library**.

---

## `AdventureWorks-Dev` (Plain Text)

| Variable                     | Value                                                   |
| ---------------------------- | ------------------------------------------------------- |
| `appServiceNameApi`          | `mick-adventureworks-api-dev`                           |
| `appServiceNameWeb`          | `mick-adventureworks-web-dev`                           |
| `resourceGroup`              | `AdventureWorks-West-US-3`                              |
| `environmentName`            | `Development`                                           |
| `keyVaultUri`                | `https://MickKeyVaultWestUS.vault.azure.net/`                   |
| `entraAuthority`             | `https://login.microsoftonline.com/{tenantId}`          |
| `entraRedirectUri`           | `https://mick-adventureworks-web-dev.azurewebsites.net` |
| `entraPostLogoutRedirectUri` | `https://mick-adventureworks-web-dev.azurewebsites.net` |
| `entraApiScope`              | `api://{clientId}/access_via_group_assignments`         |

> Replace `{tenantId}` and `{clientId}` with actual values when creating the group.

## `AdventureWorks-Dev-Secrets` (Key Vault Linked)

| Variable                      | Key Vault Secret |
| ----------------------------- | ---------------- |
| `appInsightsConnectionString` | *(see Key Vault mapping below)* |
| `sqlConnectionString`         | *(see Key Vault mapping below)* |
| `entraClientId`               | *(see Key Vault mapping below)* |
| `autoMapperLicenseKey`        | *(see Key Vault mapping below)* |

---

## `AdventureWorks-Prod` (Plain Text)

| Variable                     | Value                                               |
| ---------------------------- | --------------------------------------------------- |
| `appServiceNameApi`          | `mick-adventureworks-api`                           |
| `appServiceNameWeb`          | `mick-adventureworks-web`                           |
| `resourceGroup`              | `AdventureWorks-West-US-3`                          |
| `environmentName`            | `Production`                                        |
| `keyVaultUri`                | `https://MickKeyVaultWestUS.vault.azure.net/`               |
| `entraAuthority`             | `https://login.microsoftonline.com/{tenantId}`      |
| `entraRedirectUri`           | `https://mick-adventureworks-web.azurewebsites.net` |
| `entraPostLogoutRedirectUri` | `https://mick-adventureworks-web.azurewebsites.net` |
| `entraApiScope`              | `api://{clientId}/access_via_group_assignments`     |

> Production App Service names have no `-dev` suffix. Replace `{tenantId}` and `{clientId}` with production values.

## `AdventureWorks-Prod-Secrets` (Key Vault Linked)

| Variable                      | Key Vault Secret |
| ----------------------------- | ---------------- |
| `appInsightsConnectionString` | *(see Key Vault mapping below)* |
| `sqlConnectionString`         | *(see Key Vault mapping below)* |
| `entraClientId`               | *(see Key Vault mapping below)* |
| `autoMapperLicenseKey`        | *(see Key Vault mapping below)* |

---

## Key Vault Secret Names → ADO Variable Mapping

Key Vault: `MickKeyVaultWestUS`

| ADO Variable Name (in `-Secrets` groups) | Key Vault Secret Name |
| ---------------------------------------- | --------------------- |
| `sqlConnectionString` | `adventureworks-sql-connection-string` |
| `appInsightsConnectionString` | `adventureworks-aplication-insights-connection-string` |
| `entraClientId` | `adventureworks-entra-client-id` |
| `autoMapperLicenseKey` | `automapper-license-key` |

When linking the `-Secrets` groups, ADO will show the Key Vault secret names. Select each one and ADO auto-creates a variable with the secret name. **Rename** the variable to match the ADO variable name in the left column (the pipeline YAML references these exact names).

## Key Vault Link Setup

For both `AdventureWorks-Dev-Secrets` and `AdventureWorks-Prod-Secrets`:

1. Toggle **Link secrets from an Azure key vault as variables**
2. Select the `MickKeyVaultWestUS` Key Vault
3. Add the 4 secrets listed above
4. Rename each variable to match the ADO variable name (e.g., `adventureworks-sql-connection-string` → `sqlConnectionString`)
5. The pipeline service connection needs **Key Vault Secrets User** role on the vault (already granted by Bicep for App Service identities — the pipeline service principal needs a separate role assignment)

## Pipeline Authorization

After creating all 4 groups, authorize the pipeline to access each one: **Library → group → Pipeline permissions → add pipeline**.
