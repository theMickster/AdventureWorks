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

| Variable (Key Vault Secret Name)                         |
| -------------------------------------------------------- |
| `adventureworks-sql-connection-string`                   |
| `adventureworks-aplication-insights-connection-string`   |
| `adventureworks-entra-client-id`                         |
| `automapper-license-key`                                 |

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

| Variable (Key Vault Secret Name)                         |
| -------------------------------------------------------- |
| `adventureworks-sql-connection-string`                   |
| `adventureworks-aplication-insights-connection-string`   |
| `adventureworks-entra-client-id`                         |
| `automapper-license-key`                                 |

---

## Key Vault Secret Names

Key Vault: `MickKeyVaultWestUS`

The pipeline YAML references these secrets by their **exact Key Vault names** — no renaming needed in ADO.

| Key Vault Secret Name                                    | Used For |
| -------------------------------------------------------- | -------- |
| `adventureworks-sql-connection-string`                   | DbUp migrations, API connection strings |
| `adventureworks-aplication-insights-connection-string`    | Angular token replacement |
| `adventureworks-entra-client-id`                         | Angular token replacement |
| `automapper-license-key`                                 | API app setting |

## Key Vault Link Setup

For both `AdventureWorks-Dev-Secrets` and `AdventureWorks-Prod-Secrets`:

1. Toggle **Link secrets from an Azure key vault as variables**
2. Select the `MickKeyVaultWestUS` Key Vault
3. Add the 4 secrets listed above — ADO creates variables with the Key Vault secret names
4. No renaming needed — the pipeline YAML uses the Key Vault names directly
5. The pipeline service connection needs **Key Vault Secrets User** role on the vault

## Pipeline Authorization

After creating all 4 groups, authorize the pipeline to access each one: **Library → group → Pipeline permissions → add pipeline**.
