# Secrets Inventory — AdventureWorks

Single reference for how every configuration value flows across environments.

## API Configuration (.NET)

| Config Key                     | Local Dev                     | Docker                             | ADO Pipeline (Dev)                | ADO Pipeline (Prod)              | Azure Runtime                                             |
| ------------------------------ | ----------------------------- | ---------------------------------- | --------------------------------- | -------------------------------- | --------------------------------------------------------- |
| SQL Connection String          | User Secrets                  | `CONNECTION_STRING` env var        | Key Vault linked var              | Key Vault linked var             | Key Vault via managed identity                            |
| Key Vault URI                  | User Secrets                  | Dummy in `appsettings.Docker.json` | `keyVaultUri` pipeline var        | `keyVaultUri` pipeline var       | `KeyVault__VaultUri` Bicep app setting                    |
| App Insights Connection String | User Secrets                  | Dummy in `appsettings.Docker.json` | Key Vault linked var              | Key Vault linked var             | `APPLICATIONINSIGHTS_CONNECTION_STRING` Bicep app setting |
| ASPNETCORE_ENVIRONMENT         | VS Code `launch.json` env var | `Docker` (docker-compose)          | Bicep app setting (`Development`) | Bicep app setting (`Production`) | Bicep app setting                                         |
| AutoMapper License Key         | User Secrets                  | N/A                                | Key Vault linked var / GH Secret  | Key Vault linked var             | N/A (build-time only)                                     |

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
| Azure Runtime (API) | Azure Key Vault                                   | Managed identity + `KeyVault__VaultUri` app setting |
| Azure Runtime (Web) | N/A                                               | Values baked into JS bundle at deploy time          |
