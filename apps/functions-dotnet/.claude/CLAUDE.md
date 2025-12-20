# AdventureWorks Functions - Claude Code Configuration

.NET 10 isolated-worker Azure Functions app hosting the Sales Order Saga orchestrator scaffold (US 806, Feature 610, Epic #569). `SalesOrderSagaStarter`/`SalesOrderSagaOrchestrator` document their own behavior in header comments — read those first; this file is for what the code can't tell you.

## Architecture Decisions

1. **Service Bus Topic + Subscription, not a Queue.** Feature 610's tech notes say "Queue", but this scaffold uses Topic + Subscription to match Feature 608's shared-backbone design (topics per domain event) — verified against the emulator's `Config.json` schema.
2. **Flex Consumption plan with system-assigned identity, not classic Consumption.** No connection strings anywhere — identity-based storage auth only. **Unverified against a real subscription** (only `az deployment group what-if` has run, not a real deploy + runtime check). Reported rough edges with Flex Consumption + identity storage auth are mostly for *user-assigned* identity; this uses system-assigned, the safer path — but don't assume it works until actually deployed and confirmed running.
3. **Local NuGet feed** for `AdventureWorks.Domain`/`AdventureWorks.Application` (not `ProjectReference`) — keeps this app deployable independently of `apps/api-dotnet`. Run `./pack-local-nuget.sh` before first build and after any change to those projects; every run stamps a fresh version so restore never serves a stale cached package.
4. **SQL / Managed Identity access is deferred** — no MI-to-SQL pattern exists yet in this repo (the API uses a Key Vault connection-string secret). Before adding any SQL-touching activity, confirm with the user whether an Entra admin is configured on the target SQL Server (required for `CREATE USER ... FROM EXTERNAL PROVIDER`) — don't add DbUp scripts speculatively.
5. **Local dev's Service Bus emulator uses the pre-existing `tosk-mssql` container (`host.docker.internal:1433`) as its SQL backend, not a disposable companion container.** This was an explicit user instruction, given with full knowledge that the emulator unconditionally drops/recreates `SbGatewayDatabase` and `SbMessageContainerDatabase00001` (fixed names, not configurable) on that server every startup. `tosk-mssql` must already be running before `docker compose up` — this compose file does not start it. **Do not reintroduce a disposable `mssql` companion service without checking with the user first.**

## Local Development

See `local-dev/SMOKE_TEST.md` for the full manual walkthrough (pack local NuGet → bring up Azurite + Service Bus emulator → start the Functions host → publish a test message → confirm it fired). Requires `tosk-mssql` already running (Architecture Decision 5).

## Testing

```bash
dotnet test AdventureWorks.Functions.sln
```

`SalesOrderSagaStarterTests` mocks `DurableTaskClient` directly (no Functions host) — matches `apps/api-dotnet/tests/AdventureWorks.UnitTests` conventions. It's also the only reliable way to exercise the starter's dedupe-on-`Running` branch — the orchestrator stub completes in milliseconds, too fast to race manually via the smoke test.

## Anti-Patterns

### DO
- Keep orchestrator functions deterministic; put I/O in activities (once activities exist).
- Reuse `SalesOrderSagaStarter.BuildInstanceId` for the deterministic instance ID format.
- Use identity-based connections (`__accountName`/`__credential`, `__fullyQualifiedNamespace`) in Bicep and Azure app settings — never a connection string.

### DON'T
- Add SQL access code in this app before confirming the Entra admin prerequisite with the user.
- Reference `AdventureWorks.Domain`/`AdventureWorks.Application` via `ProjectReference` — use the local NuGet feed.
- Hardcode a static package version for the local feed — it must change on every pack.
- Reintroduce a disposable SQL companion container for the local Service Bus emulator without checking with the user first — see Architecture Decision 5.

## References

- [Root CLAUDE.md](../../../.claude/CLAUDE.md) | [Copilot instructions](../../../.github/instructions/functions-dotnet.instructions.md)
- [Durable Functions Azure Storage provider](https://learn.microsoft.com/en-us/azure/durable-task/durable-functions/durable-functions-azure-storage-provider)
- [Service Bus emulator](https://learn.microsoft.com/en-us/azure/service-bus-messaging/test-locally-with-service-bus-emulator)
- [Flex Consumption plan](https://learn.microsoft.com/en-us/azure/azure-functions/flex-consumption-plan)
