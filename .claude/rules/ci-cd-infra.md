---
paths:
  - ".github/workflows/**"
  - "azure-pipelines.yml"
  - "pipelines/**"
  - "infra/**"
---

# CI/CD and Infrastructure Rules

## Gitignored Files Do Not Exist in CI

`environment.development.ts` and similar dev files are gitignored. CI commands must use explicit `--configuration` flags or `--exclude` to avoid referencing them.

## Nx Tests: App ≠ Workspace

`nx test <app>` runs ONE project's tests. Use `nx run-many -t test` to run all libraries. Always verify the test count, not just pass/fail.

## CI Cache Keys Must Hit

Never put commit SHA or build ID in primary cache keys. For npm, cache `~/.npm`, not `node_modules` (`npm ci` deletes it).

## GitHub Actions

Every `uses:` in `.github/workflows/` MUST be pinned to the full commit SHA of the latest stable release (verified signed, with a `# vX.Y.Z` comment), never a tag or branch.

## Azure Pipelines

- Use `PublishPipelineArtifact@1` / `DownloadPipelineArtifact@2` (not deprecated v1)
- Use `continueOnError: true` on tasks, never `|| true` in scripts
- Deployment jobs: use `$(Build.SourcesDirectory)` for absolute paths, not relative
- Cross-RG deployment names: `{name}-${environment}` to avoid collisions

## Verify Azure Resource Names

Run `az resource list` before writing Bicep or docs. Never assume names follow a convention.

## Bicep Layout

- `infra/main.bicep` is the orchestrator; modules live under `infra/modules/`. Four modules (`sqlServer`, `sqlDatabase`, `keyVault`, `appInsights`) are not referenced by `main.bicep`.
- `parameters.dev.json` and `parameters.prod.json` hold environment-specific values. No secrets in param files.
- Validate with `az bicep build --file infra/main.bicep`
- Deploy strategy: one resource group; shared App Service plan; per-environment API and Web App Services plus an optional saga stack (storage, Service Bus, Flex function app) gated by `deploySalesOrderSaga`. App Insights and Key Vault are pre-existing (Key Vault in `keyVaultResourceGroup`). SQL is not deployed by Bicep.
