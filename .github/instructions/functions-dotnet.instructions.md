---
applyTo: "apps/functions-dotnet/**/*"
---

# AdventureWorks Functions Copilot Instructions

**Applies to:** `apps/functions-dotnet/**`

Use this file together with `apps/functions-dotnet/.claude/CLAUDE.md`.

## Scope (US 806-807)

US 806 scaffolded the Sales Order Saga app; US 807 added the first real saga steps (order
validation, fan-out inventory check, transactional stock reservation) and this app's first SQL
access. See the CLAUDE.md's Architecture Decisions for the SQL/connection-string mechanics.
Payment authorization and compensation remain out of scope until stories 808-810.

## Local NuGet Feed

- `AdventureWorks.Domain` and `AdventureWorks.Application` are consumed as NuGet packages, not
  `ProjectReference`, so this app can be built/deployed independently of `apps/api-dotnet`.
- The version is never a static string — `pack-local-nuget.sh` generates a fresh timestamp
  version on every run and writes it to the gitignored `eng/local-nuget-version.props`.
- Run `./pack-local-nuget.sh` (from `apps/functions-dotnet/`) any time `AdventureWorks.Domain`,
  `AdventureWorks.Application`, `AdventureWorks.Common`, or `AdventureWorks.Models` change, before
  restoring or building this app. If restore still resolves an old version, run
  `dotnet nuget locals all --clear`.

## Managed Identity for Storage/Service Bus; SQL Auth Only for Local Dev

- Storage and Service Bus use identity-based connections in Azure, never a connection string.
  Local dev uses Azurite and the Service Bus emulator's fixed dev-only connection string.
- SQL is the one exception, per Architecture Decision 4 in the app's CLAUDE.md: SQL-auth locally
  (gitignored `local.settings.json`, never committed), managed identity in Azure.

## Durable Functions

- Orchestrator functions must stay deterministic (no direct I/O, `DateTime.Now`, `Guid.NewGuid()`,
  or non-deterministic branching) — call activities for anything with a side effect.
- Use `context.CreateReplaySafeLogger(...)` inside orchestrators, never `ILogger` injected via DI.
- Instance IDs for the sales order saga are deterministic (`sales-order-saga-{SalesOrderId}`) via
  `SalesOrderSagaStarter.BuildInstanceId` — reuse it rather than reimplementing the format.
