# Manual Smoke Test — Sales Order Saga Scaffold (US 806)

Confirms the Service Bus-triggered starter deserializes an `OrderCreated` message and starts the orchestrator with a deterministic instance ID, running against the real Azurite + Service Bus emulator (not mocks). Takes about 5 minutes.

## 1. Pack the local NuGet feed

```bash
cd apps/functions-dotnet
./pack-local-nuget.sh
```

## 2. Start Azurite + the Service Bus emulator

Requires the `tosk-mssql` container (`localhost:1433`) already running — the emulator uses it as its SQL backend (`host.docker.internal:1433`) and will drop/recreate `SbGatewayDatabase` and `SbMessageContainerDatabase00001` on it every start.

```bash
cd apps/functions-dotnet/local-dev
cp .env.local.example .env.local   # first time only — set MSSQL_SA_PASSWORD to tosk-mssql's SA password
docker compose -f docker-compose.local.yml --env-file .env.local up -d
```

Wait for the emulator's health endpoint to report healthy (takes ~20-30s after the containers start):

```bash
curl -s http://localhost:5300/health
# {"status":"healthy","message":"Service is healthy."}
```

## 3. Start the Functions host

```bash
cd apps/functions-dotnet/src/AdventureWorks.Functions.SalesOrderSaga
cp local.settings.json.example local.settings.json   # first time only
func start
```

Confirm both functions are listed:

```
Functions:
    SalesOrderSagaOrchestrator: orchestrationTrigger
    SalesOrderSagaStarter: serviceBusTrigger
```

## 4. Publish a test `OrderCreated` message

In a second terminal:

```bash
cd apps/functions-dotnet/local-dev/smoke-test-tool
dotnet run -- 900000
```

`900000` is the sales order ID — change it to send a different order. Keep it outside AdventureWorks' real `SalesOrderID` range (43659-75123); an ID inside that range already has `TransactionHistory` rows and will trip `ReserveStockActivity`'s replay-idempotency short-circuit, silently skipping the actual reservation.

## 5. Confirm it fired

In the `func start` terminal, expect (order and exact timings will vary):

```
Executing 'Functions.SalesOrderSagaStarter' ...
Scheduling new SalesOrderSagaOrchestrator orchestration with instance ID 'sales-order-saga-900000' and NNN bytes of input data.
Started sales order saga sales-order-saga-900000 for SalesOrderId 900000.
Executed 'Functions.SalesOrderSagaStarter' (Succeeded, ...)
Executing 'Functions.SalesOrderSagaOrchestrator' ...
'SalesOrderSagaOrchestrator' orchestration with ID 'sales-order-saga-900000' started.
Sales order saga orchestration accepted for SalesOrderId 900000 with 1 line item(s).
'SalesOrderSagaOrchestrator' orchestration with ID 'sales-order-saga-900000' completed.
Executed 'Functions.SalesOrderSagaOrchestrator' (Succeeded, ...)
```

The instance ID is deterministic — re-running step 4 with the same sales order ID while the first orchestration is still `Running` logs "already Running; skipping duplicate OrderCreated delivery" instead of starting a second instance. Once an instance has completed, sending the same ID again starts a fresh orchestration (only in-flight duplicates are suppressed). The orchestrator stub completes in milliseconds, so racing the `Running` window manually isn't practical — `SalesOrderSagaStarterTests` covers that branch with a mocked `DurableTaskClient` instead.

## 6. Leave it running

Keep the `func start` terminal and the `azurite`/`emulator` containers up for the next session — no teardown needed. The only host ports in use are Azurite's `11000-11002` and the SB emulator's `5673`/`5300`; `tosk-mssql` is managed independently of this stack and isn't affected either way.

If you ever do want to tear it down:

```bash
# Ctrl+C the func start terminal
cd apps/functions-dotnet/local-dev
docker compose -f docker-compose.local.yml --env-file .env.local down
```
