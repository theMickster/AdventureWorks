---
applyTo: "apps/functions-dotnet/**/*"
---

# AdventureWorks Functions Copilot Instructions

**Applies to:** `apps/functions-dotnet/**`

Use this file together with `apps/functions-dotnet/.claude/CLAUDE.md`.

## Scope (US 806)

This app currently contains only the Sales Order Saga **scaffold**: a Service Bus-triggered
starter, a deterministic-instance-ID orchestrator stub, and shared domain/application packages
consumed via a local NuGet feed. Real saga activities (inventory validation, stock reservation,
payment authorization, compensation) are out of scope until stories 807-810 — do not add
SQL-touching code here without confirming with the user first.

## Local NuGet Feed

- `AdventureWorks.Domain` and `AdventureWorks.Application` are consumed as NuGet packages, not
  `ProjectReference`, so this app can be built/deployed independently of `apps/api-dotnet`.
- The version is never a static string — `pack-local-nuget.sh` generates a fresh timestamp
  version on every run and writes it to the gitignored `eng/local-nuget-version.props`.
- Run `./pack-local-nuget.sh` (from `apps/functions-dotnet/`) any time `AdventureWorks.Domain`,
  `AdventureWorks.Application`, `AdventureWorks.Common`, or `AdventureWorks.Models` change, before
  restoring or building this app. If restore still resolves an old version, run
  `dotnet nuget locals all --clear`.

## Managed Identity Only

- No connection strings or shared keys in `local.settings.json.example` or Bicep outputs.
- Storage (`AzureWebJobsStorage`, `DurableStorage`) and Service Bus (`ServiceBusConnection`) use
  identity-based connections (`__accountName`/`__credential` or `__fullyQualifiedNamespace`
  app-setting prefixes) in Azure. Local dev uses Azurite (`UseDevelopmentStorage=true`) and the
  Service Bus emulator's fixed dev-only connection string — neither is a real secret.
- SQL/MI is explicitly deferred — see the app's CLAUDE.md before adding any SQL access.

## Durable Functions

- Orchestrator functions must stay deterministic (no direct I/O, `DateTime.Now`, `Guid.NewGuid()`,
  or non-deterministic branching) — call activities for anything with a side effect.
- Use `context.CreateReplaySafeLogger(...)` inside orchestrators, never `ILogger` injected via DI.
- Instance IDs for the sales order saga are deterministic (`sales-order-saga-{SalesOrderId}`) via
  `SalesOrderSagaStarter.BuildInstanceId` — reuse it rather than reimplementing the format.
