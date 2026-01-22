# AdventureWorks Functions - Claude Code Configuration

.NET 10 isolated-worker Azure Functions app hosting the Sales Order Saga orchestrator (Feature 610, Epic #569). The orchestrator validates the order, checks and reserves inventory, authorizes payment, confirms, releases stock as compensation, and enqueues outbox events. `SalesOrderSagaStarter`/`SalesOrderSagaOrchestrator` document their own behavior in header comments. Read those first; this file is for what the code can't tell you.

## Architecture Decisions

1. **Service Bus Topic + Subscription, not a Queue.** Feature 610's tech notes say "Queue", but this scaffold uses Topic + Subscription to match Feature 608's shared-backbone design (topics per domain event) — verified against the Service Bus emulator's config schema.
2. **Flex Consumption plan with system-assigned identity, not classic Consumption.** No connection strings anywhere — identity-based storage auth only. **Unverified against a real subscription** (only `az deployment group what-if` has run, not a real deploy + runtime check). Reported rough edges with Flex Consumption + identity storage auth are mostly for *user-assigned* identity; this uses system-assigned, the safer path — but don't assume it works until actually deployed and confirmed running.
3. **Local NuGet feed** for `AdventureWorks.Domain`/`AdventureWorks.Application`/`AdventureWorks.Connections` (not `ProjectReference`) — keeps this app deployable independently of `apps/api-dotnet`. Run `./pack-local-nuget.sh` before first build and after any change to those projects; every run stamps a fresh version so restore never serves a stale cached package.
4. **SQL access, introduced by US 807.** A minimal, independent `SalesOrderSagaDbContext` (`Persistence/`) maps `Production.ProductInventory`, `Production.TransactionHistory`, `dbo.SalesOrderSagaInventoryAllocation`, and `dbo.SalesOrderSagaOutboxMessage` (the saga tables come from DbUp scripts `20260906_1200` and `20260906_1201`), plus `SalesOrderReadModel`, a read-only projection over `Sales.SalesOrderHeader`, `Sales.SalesOrderDetail`, `Person.Address`, and `Purchasing.ShipMethod`. Columns are verified against the live schema via the `querying-adventureworks-database` skill, not copied from `apps/api-dotnet`. Connection string is environment-specific config only, no code branching: SQL-auth via `dotnet user-secrets` locally, `Authentication=Active Directory Managed Identity` in Azure. **Entra admin configuration on the real Azure SQL Server is still an open infra follow-up.** The managed-identity path is wired but unverified against a real deploy. `Program.cs` resolves this via `AdventureWorks.Connections`' `IConnectionCatalog` under the canonical name `ConnectionNames.AdventureWorks`. Set it locally with `dotnet user-secrets set "ConnectionStrings:AdventureWorks" "..."`, not the old `ConnectionStrings:DefaultConnection`/flat `SqlConnectionString`.
5. **Local-dev Service Bus connection string** goes through the same `IConnectionCatalog` under `ConnectionNames.ServiceBus`. Aspire injects `ConnectionStrings__ServiceBus`; set it in `local.settings.json` only for a standalone run (see `tools/aspire/README.md`), never as the old flat `ServiceBusConnection`/`ConnectionStrings:servicebus`. The identity-based Azure path (`ServiceBusConnection:fullyQualifiedNamespace` + `DefaultAzureCredential`) and its Bicep binding are unchanged. Do not rename that without separate, explicit confirmation (it requires a redeploy).

## Local Development

Run it through Aspire: see [`tools/aspire/README.md`](../../../tools/aspire/README.md). Aspire starts Azurite, a session-scoped Service Bus emulator with its own isolated SQL, and this app; the AdventureWorks SQL stays in the external `tosk-mssql` container. Run `./pack-local-nuget.sh` first (Architecture Decision 3).

## Testing

```bash
dotnet test AdventureWorks.Functions.sln
```

`SalesOrderSagaStarterTests` mocks `DurableTaskClient` directly (no Functions host). That matches `apps/api-dotnet/tests/AdventureWorks.UnitTests` conventions. It's also the only reliable way to exercise the starter's dedupe-on-`Running` branch, which is too fast to race manually via the smoke test.

`tests/AdventureWorks.SalesOrderSaga.OrchestrationTests` is an optional, second layer that drives `CheckInventorySubOrchestratorCore`/`SalesOrderSagaOrchestratorCore` through the real in-memory engine via `Microsoft.DurableTask.InProcessTestHost` — that package is preview-only (no stable release, no SLA) as of this writing; the required Moq coverage in `AdventureWorks.SalesOrderSaga.UnitTests` stands on its own regardless of that package's status.

`tests/AdventureWorks.SalesOrderSaga.TestHarness` is the Aspire `saga-test-harness` resource (payment-authorization simulator and scenario runner, see `tools/aspire/SALES_ORDER_SAGA_TESTING.md`); `TestHarness.Tests` unit-tests it.

## Anti-Patterns

### DO
- Keep orchestrator functions deterministic; all I/O goes in `Activities/`.
- Reuse `SalesOrderSagaStarter.BuildInstanceId` for the deterministic instance ID format.
- Use identity-based connections (`__accountName`/`__credential`, `__fullyQualifiedNamespace`) in Bicep and Azure app settings — never a connection string.
- Map a concurrency token (e.g. `rowguid`) on any SQL row a saga activity mutates and bump it on every write — `BeginTransactionAsync` alone only makes one activity invocation's own batch atomic, not isolated from a second concurrent saga instance touching the same row. See `ProductInventoryConfiguration`/`ReserveStockActivityCore`.

### DON'T
- Add SQL access code to a new table without first verifying its columns against the live schema (via the `querying-adventureworks-database` skill) — this app maps its own minimal, independently-verified subset rather than reusing `apps/api-dotnet`'s configurations.
- Reference `AdventureWorks.Domain`/`AdventureWorks.Application`/`AdventureWorks.Connections` via `ProjectReference`. Use the local NuGet feed.
- Hardcode a static package version for the local feed — it must change on every pack.

## References

- [Root CLAUDE.md](../../../.claude/CLAUDE.md) | [Copilot instructions](../../../.github/instructions/functions-dotnet.instructions.md)
- [Durable Functions Azure Storage provider](https://learn.microsoft.com/en-us/azure/durable-task/durable-functions/durable-functions-azure-storage-provider)
- [Service Bus emulator](https://learn.microsoft.com/en-us/azure/service-bus-messaging/test-locally-with-service-bus-emulator)
- [Flex Consumption plan](https://learn.microsoft.com/en-us/azure/azure-functions/flex-consumption-plan)
