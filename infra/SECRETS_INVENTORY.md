# Secrets Inventory — AdventureWorks

Single reference for how every configuration value flows across environments.

## API Configuration (.NET)

| Config Key                     | Local Dev                     | Docker                             | ADO Pipeline (Dev)                | ADO Pipeline (Prod)              | Azure Runtime                                             |
| ------------------------------ | ----------------------------- | ---------------------------------- | --------------------------------- | -------------------------------- | --------------------------------------------------------- |
| SQL Connection String          | User Secrets                  | `CONNECTION_STRING` env var        | Key Vault linked var              | Key Vault linked var             | Pipeline `AzureAppServiceSettings@1` (from Key Vault linked var group) |
| Key Vault URI                  | User Secrets                  | Dummy in `appsettings.Docker.json` | `keyVaultUri` pipeline var        | `keyVaultUri` pipeline var       | `KeyVault__VaultUri` Bicep app setting                    |
| App Insights Connection String | User Secrets                  | Dummy in `appsettings.Docker.json` | Key Vault linked var              | Key Vault linked var             | `APPLICATIONINSIGHTS_CONNECTION_STRING` Bicep app setting |
| ASPNETCORE_ENVIRONMENT         | VS Code `launch.json` env var | `Docker` (docker-compose)          | Bicep app setting (`Development`) | Bicep app setting (`Production`) | Bicep app setting                                         |
| AutoMapper License Key         | User Secrets                  | N/A                                | Key Vault linked var / GH Secret  | Key Vault linked var             | Pipeline `AzureAppServiceSettings@1` (from Key Vault linked var group) |

## Sales Order Saga Functions Configuration (`apps/functions-dotnet`)

No secrets — Managed Identity only. This is intentional: the Function App's system-assigned
identity is granted RBAC roles directly on Service Bus (Data Receiver on the topic) and its
storage account (Blob/Queue/Table Data Contributor), and reads nothing from Key Vault.

| Config Key                                | Local Dev                                | Azure Runtime                                                          |
| ------------------------------------------ | ----------------------------------------- | ----------------------------------------------------------------------- |
| `AzureWebJobsStorage`                       | `UseDevelopmentStorage=true` (Azurite)    | `AzureWebJobsStorage__accountName` + `__credential=managedidentity` (Bicep) |
| `DurableStorage`                            | `UseDevelopmentStorage=true` (Azurite)    | `DurableStorage__accountName` + `__credential=managedidentity` (Bicep)     |
| `ConnectionStrings:AdventureWorks` (SQL)    | `dotnet user-secrets` (`ConnectionStrings:AdventureWorks`) | `Authentication=Active Directory Managed Identity` connection string, app setting |
| `ConnectionStrings:ServiceBus`              | `local.settings.json` (`ConnectionStrings__ServiceBus`) — emulator fixed dev connection string | `ServiceBusConnection__fullyQualifiedNamespace` (Bicep, MI) — identity-based, no connection string |
| `ServiceBusSalesOrderEventsTopicName`       | `local.settings.json`                     | Bicep app setting                                                       |
| `ServiceBusSalesOrderSagaSubscriptionName`  | `local.settings.json`                     | Bicep app setting                                                       |

Both SQL and Service Bus are resolved through the shared `AdventureWorks.Connections` `IConnectionCatalog`
(`ConnectionNames.AdventureWorks` / `ConnectionNames.ServiceBus`), read from the standard `ConnectionStrings:*`
config section — same on-disk/env-var shape as `apps/api-dotnet`, just under the app-local key names above.

**SQL access is wired** (`SalesOrderSagaDbContext`, used by `CheckInventoryActivity`, `ReserveStockActivity`,
`ConfirmOrderActivity`, `ReleaseStockActivity`, `EnqueueSagaEventActivity`) — this table previously said
otherwise; that was stale. **Entra admin configuration on the real Azure SQL Server is still an open infra
follow-up** — confirm it's in place before relying on the managed-identity connection string in a real deploy
(there is no existing MI-to-SQL pattern verified against a real deploy in this repo today; the API itself uses
the `adventureworks-sql-connection-string` Key Vault secret below, not MI).

## Angular Configuration (Frontend)

These values are baked into the Angular build as `__PLACEHOLDER__` tokens and replaced at deploy time by `infra/scripts/replace-tokens.sh`.

| Config Key                     | Local Dev                    | Docker                           | ADO Pipeline (Dev)                                       | ADO Pipeline (Prod) | Azure Runtime        |
| ------------------------------ | ---------------------------- | -------------------------------- | -------------------------------------------------------- | ------------------- | -------------------- |
| App Insights Connection String | `environment.development.ts` | Dummy in `environment.docker.ts` | Token replacement (`__APP_INSIGHTS_CONNECTION_STRING__`) | Token replacement   | Baked into JS bundle |
| Entra Authority                | `environment.development.ts` | N/A (no auth in Docker)          | Token replacement (`__ENTRA_AUTHORITY__`)                | Token replacement   | Baked into JS bundle |
| Entra Client ID                | `environment.development.ts` | N/A                              | Token replacement (`__ENTRA_CLIENT_ID__`)                | Token replacement   | Baked into JS bundle |
| Entra Redirect URI             | `environment.development.ts` | N/A                              | Token replacement (`__ENTRA_REDIRECT_URI__`)             | Token replacement   | Baked into JS bundle |
| Entra Post-Logout URI          | `environment.development.ts` | N/A                              | Token replacement (`__ENTRA_POST_LOGOUT_REDIRECT_URI__`) | Token replacement   | Baked into JS bundle |
| Entra API Scope                | `environment.development.ts` | N/A                              | Token replacement (`__ENTRA_API_SCOPE__`)                | Token replacement   | Baked into JS bundle |

## Secret Sources by Environment

| Environment         | Secret Store                                      | Access Method                                       |
| ------------------- | ------------------------------------------------- | --------------------------------------------------- |
| Local Dev           | .NET User Secrets + `environment.development.ts`  | File system (gitignored)                            |
| Docker              | Environment variables + `appsettings.Docker.json` | docker-compose `.env.docker`                        |
| ADO Pipeline        | Key Vault linked variable groups                  | Pipeline variable expansion                         |
| Azure Runtime (API) | App Service settings (migrate to Key Vault later) | App settings / connection strings on App Service    |
| Azure Runtime (Web) | N/A                                               | Values baked into JS bundle at deploy time          |

## Key Vault Secrets (`MickKeyVaultWestUS`)

| Key Vault Secret Name                                  | Value Source                          | Consumed By                     |
| ------------------------------------------------------- | ------------------------------------- | ------------------------------- |
| `adventureworks-sql-connection-string`                  | Same as App Service `DefaultConnection` | DbUp migrations (pipeline)    |
| `adventureworks-aplication-insights-connection-string`  | App Service `APPLICATIONINSIGHTS_CONNECTION_STRING` | Angular token replacement |
| `adventureworks-entra-client-id`                        | Entra app registration client ID      | Angular token replacement       |
| `automapper-license-key`                                | AutoMapper license key                | .NET unit tests (build-time)    |
