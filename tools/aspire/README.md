# AdventureWorks local development with Aspire

Aspire is the canonical local experience. One AppHost starts the API, Angular app, Sales Order Functions, payment/test harness, session-scoped Azurite, and a session-scoped Service Bus emulator with its isolated companion SQL dependency. The existing AdventureWorks SQL Server remains external and is shown as `tosk-mssql` in the dashboard.

## Prerequisites and setup

- .NET SDK 10, Node.js/npm, and a Docker-compatible container runtime
- An AdventureWorks SQL Server with the latest DbUp scripts, including the saga allocation and outbox tables
- The AppHost database secret:

```bash
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "YOUR_CONNECTION_STRING" --project tools/aspire/AdventureWorks.AppHost
```

DbUp keeps its separate `ConnectionStrings:AdventureWorks` user secret. If the external SQL container is not named `tosk-mssql`, set `SqlServer:ContainerName` in the AppHost configuration.

## Launch

```bash
dotnet run --project tools/aspire/AdventureWorks.AppHost
```

Open the dashboard URL printed in the terminal. No emulator ports or connection strings need to be configured: Aspire injects them.

## Resources

| Resource                | Purpose                                                                     |
| ----------------------- | --------------------------------------------------------------------------- |
| `functions-storage`     | Session-scoped Azurite host and Durable storage                             |
| `servicebus`            | Session-scoped emulator; its internal SQL is isolated from AdventureWorks   |
| `sales-order-functions` | Durable saga, topic triggers, status endpoint, and outbox timer             |
| `saga-test-harness`     | Payment simulator, fixture controller, evidence reader, and smoke scenarios |
| `api` / `angular-web`   | Existing application workflow                                               |
| `dbup`                  | Explicit-start migration runner                                             |
| `tosk-mssql`            | Health-only representation of the externally managed database               |

The `sales-order-events` topic contains `sales-order-saga` and `sales-order-payment-results` subscriptions. Emulator state lasts only for the Aspire session.

## Test harness commands

Open the `saga-test-harness` resource commands in the dashboard. Commands are enabled when the harness and its dependencies are healthy. You can start an order, list payments, approve or decline by ID, inspect combined evidence, run either complete scenario, or clean up fixtures.

Follow the complete human verification procedure in [`SALES_ORDER_SAGA_TESTING.md`](SALES_ORDER_SAGA_TESTING.md).

One-click scenarios keep generated rows for diagnosis and return structured assertions, elapsed time, and the failure stage. They allow two minutes so the one-minute outbox timer can dispatch. IDs begin at 900000 and remain monotonic for the AppHost session.

Direct HTTP equivalents are under the harness `/test-control` route:

```bash
curl -X POST "$HARNESS_URL/test-control/scenarios/declined" -H 'Content-Type: application/json' -d '{}'
curl "$HARNESS_URL/test-control/authorizations"
curl "$HARNESS_URL/test-control/sagas/900000"
curl -X DELETE "$HARNESS_URL/test-control/fixtures?confirm=true"
```

Cleanup requires both the dashboard confirmation and `confirm=true`. It selects only headers whose comment begins `AW-SAGA-HARNESS:` and removes their related details, saga allocations, transaction history, and outbox records. It never targets ordinary AdventureWorks orders.

## Standalone Functions fallback

`local.settings.json` intentionally contains only `FUNCTIONS_WORKER_RUNTIME=dotnet-isolated`. Without Aspire, manually supply `AzureWebJobsStorage`, `ServiceBusConnection`, the three `ServiceBusSalesOrder...` topic/subscription settings, `ConnectionStrings__DefaultConnection` (or `SqlConnectionString`), and `PaymentAuthorization__BaseUrl`. Durable Functions shares `AzureWebJobsStorage`. Standalone mode does not start emulators or the payment simulator.

The separate containerized API/web workflow is documented in [`../../docker/README.md`](../../docker/README.md).
