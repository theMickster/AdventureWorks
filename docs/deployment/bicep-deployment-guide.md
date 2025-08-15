# Bicep Deployment Guide

Manual deployment of Azure infrastructure using Bicep templates. The pipeline does NOT run Bicep — these templates are deployed independently.

## Prerequisites

- Azure CLI installed (`az --version`)
- Bicep CLI installed (`az bicep version`)
- Logged in to Azure (`az login`)
- Correct subscription set (`az account set --subscription <name-or-id>`)

## What Bicep Manages

| Resource | Created by Bicep | Notes |
|----------|:---:|-------|
| App Service Plan | Yes | Linux B1, shared across all 4 App Services |
| API App Services (dev + prod) | Yes | .NET 10, managed identity, HTTPS, FTP disabled |
| Web App Services (dev + prod) | Yes | Node 24, HTTPS, FTP disabled |
| Key Vault RBAC | Yes | Grants API managed identities Key Vault Secrets User (cross-RG) |
| Application Insights | No | Referenced as `existing` (same RG) |
| Key Vault | No | Referenced as `existing` (different RG) |
| SQL Server / Database | No | Managed separately (different RG) |

## Resource Topology

```
AdventureWorks-West-US-3 (target RG)
├── mick-adventureworks-plan          (App Service Plan — created)
├── mick-adventureworks-api-dev       (App Service — created)
├── mick-adventureworks-api           (App Service — created)
├── mick-adventureworks-web-dev       (App Service — created)
├── mick-adventureworks-web           (App Service — created)
└── mick-adventureworks               (App Insights — existing, referenced)

Mick-West-US-DotNetDevTest (cross-RG, not deployed to)
├── MickKeyVaultWestUS                (Key Vault — existing, RBAC assigned)
└── mletofsky                         (SQL Server — not managed by Bicep)
    └── AdventureWorks                (SQL Database — not managed by Bicep)
```

## Deployment Strategy

Deploy twice to the SAME resource group — once for dev, once for prod. Shared resources (plan) are created on first deploy and reused on second. App Services are differentiated by environment suffix (`-dev` for dev, no suffix for prod).

```
Deploy dev  → creates plan + api-dev + web-dev + KV RBAC for api-dev
Deploy prod → reuses plan + creates api + web + KV RBAC for api
```

## Step 1: Validate Templates

```bash
az bicep build --file infra/main.bicep
rm -f infra/main.json
```

Zero errors and zero warnings expected.

## Step 2: What-If (Dry Run)

Always run what-if before deploying. This shows what Azure would create, modify, or delete — without changing anything.

### Dev

```bash
az deployment group create \
  --resource-group AdventureWorks-West-US-3 \
  --template-file infra/main.bicep \
  --parameters infra/parameters.dev.json \
  --what-if
```

### Prod

```bash
az deployment group create \
  --resource-group AdventureWorks-West-US-3 \
  --template-file infra/main.bicep \
  --parameters infra/parameters.prod.json \
  --what-if
```

**Expected output:** Create operations for App Service Plan, App Services, and Key Vault role assignments. No changes to App Insights. No SQL operations.

## Step 3: Deploy

### Dev

```bash
az deployment group create \
  --resource-group AdventureWorks-West-US-3 \
  --template-file infra/main.bicep \
  --parameters infra/parameters.dev.json
```

### Prod

```bash
az deployment group create \
  --resource-group AdventureWorks-West-US-3 \
  --template-file infra/main.bicep \
  --parameters infra/parameters.prod.json
```

Deployments use **incremental mode** (default) — they create/update resources but never delete existing ones.

## Step 4: Verify in Portal

After each deployment, confirm in the Azure Portal:

- [ ] App Service Plan exists (`mick-adventureworks-plan`, Linux, B1)
- [ ] API App Service exists (correct runtime, HTTPS only, FTP disabled, TLS 1.2)
- [ ] Web App Service exists (correct runtime, HTTPS only, FTP disabled, TLS 1.2)
- [ ] API has app settings: `APPLICATIONINSIGHTS_CONNECTION_STRING`, `ASPNETCORE_ENVIRONMENT`, `KeyVault__VaultUri`
- [ ] Web has app settings: `APPLICATIONINSIGHTS_CONNECTION_STRING`
- [ ] API managed identity has Key Vault Secrets User on `MickKeyVaultWestUS` (check IAM in the Key Vault)
- [ ] Web does NOT have Key Vault access (least privilege)

## Post-Deployment: App Service Configuration

Bicep creates the infrastructure. Runtime configuration is fully managed by the Azure Pipelines CI/CD pipeline at deploy time via the `AzureAppServiceSettings@1` task and the `AzureRmWebAppDeployment@4` `StartupCommand` input. No manual Portal configuration is needed.

The pipeline automatically configures:

1. **API connection strings** — `DefaultConnection` and `SqlAzureConnection` set by `AzureAppServiceSettings@1` (values from Key Vault linked variable groups)
2. **API app settings** — `AutoMapperLicenseKey` set by `AzureAppServiceSettings@1` (value from Key Vault linked variable groups)
3. **Web startup command** — `pm2 serve /home/site/wwwroot --no-daemon --spa` set by `AzureRmWebAppDeployment@4` `StartupCommand` input
4. **Angular config** — Baked into the JS bundle via token replacement at pipeline deploy time

## Rollback

If a deployment fails or produces unexpected results:

- **App Services:** Delete in Portal and redeploy
- **Plan:** Delete in Portal and redeploy (App Services must be deleted first)
- **Key Vault RBAC:** Remove role assignments in Portal → Key Vault → Access control (IAM)

Your existing App Insights, Key Vault, and SQL are never modified or deleted by these templates.

## Parameter Files

| File | Environment | Key Differences |
|------|-------------|-----------------|
| `infra/parameters.dev.json` | `dev` | Creates `*-dev` suffixed App Services |
| `infra/parameters.prod.json` | `prod` | Creates App Services without suffix |

Both reference the same App Insights, Key Vault, and Key Vault resource group.

## Related Documentation

- `infra/variable-groups.md` — ADO variable group definitions
- `infra/SECRETS_INVENTORY.md` — Secret flow across all environments
- `DOCKER.md` — Docker local development (separate from Azure deployment)
