# Sales Order Saga local test playbook

This playbook is the human verification gate for the Aspire-based Sales Order Saga harness. It creates only marked, disposable orders with IDs starting at 900000. Scenario rows are deliberately retained until you run cleanup.

## 1. One-time prerequisites

You need .NET 10, Node.js/npm, a running Docker-compatible engine, and the externally managed AdventureWorks SQL Server.

Set the AppHost connection string:

```bash
dotnet user-secrets set "ConnectionStrings:AdventureWorks" "YOUR_ADVENTUREWORKS_CONNECTION_STRING" \
  --project tools/aspire/AdventureWorks.AppHost
```

The database must contain these tables:

- `dbo.SalesOrderSagaInventoryAllocation`
- `dbo.SalesOrderSagaOutboxMessage`

If either is absent, launch Aspire and explicitly start the `dbup` resource before testing. DbUp uses its own `ConnectionStrings:AdventureWorks` user secret.

## 2. Start the complete stack

From the repository root:

```bash
dotnet run --project tools/aspire/AdventureWorks.AppHost
```

Open the dashboard URL printed in the terminal. Wait for these resources:

| Resource                      | Expected state    |
| ----------------------------- | ----------------- |
| `functions-storage`           | Running / Healthy |
| `servicebus`                  | Running / Healthy |
| `servicebus-mssql`            | Running / Healthy |
| `sales-order-events`          | Running / Healthy |
| `sales-order-saga`            | Running / Healthy |
| `sales-order-payment-results` | Running / Healthy |
| `sales-order-functions`       | Running           |
| `saga-test-harness`           | Running / Healthy |
| `tosk-mssql`                  | Running / Healthy |

The emulator containers use random loopback ports. Do not copy connection strings or ports into source files or `local.settings.json`.

## 3. Interactive decline flow

Use the command menu on `saga-test-harness`:

1. Run **Start test order** with quantity `1`.
2. Copy `salesOrderId` from the JSON result.
3. Run **List payments** until that ID appears with `state: "Pending"`.
4. Run **Decline payment** and enter the copied ID.
5. Run **Check saga status** for the same ID until:
   - `status.runtimeStatus` is `Completed`;
   - `status.sagaStatus` is `PaymentDeclined`;
   - the SQL evidence shows order status `1` and revision `0`;
   - inventory is back at its baseline;
   - every allocation is reversed; and
   - the `OrderFailed` outbox row has a non-null `outboxDispatchedAt`.

The default decline reason is `manual smoke decline`. Repeating **Decline payment** is safe. Trying to approve the same authorization afterward must return HTTP `409 Conflict`.

## 4. One-click scenarios

Run both commands from `saga-test-harness`:

1. **Run approved scenario**
2. **Run declined scenario**

Each can take slightly over one minute because outbox dispatch runs once per minute. The hard timeout is two minutes.

A passing response has:

```json
{
  "passed": true,
  "failureStage": null,
  "assertions": [{ "passed": true }]
}
```

Approved evidence must show `Completed/Approved`, order status `5`, one revision increment, inventory reduced by one, unreversed allocations, and a dispatched `OrderApproved` payload containing lines, shipping address, and ship method.

Declined evidence must show `Completed/PaymentDeclined`, unchanged pending order status/revision, restored inventory, allocations reversed once, and a dispatched `OrderFailed` payload containing `PaymentDeclined` and `manual smoke decline`.

If a scenario fails, preserve its order ID and inspect `failureStage`, the assertion list, Functions logs, and harness logs before cleanup.

## 5. Optional direct HTTP calls

Copy the harness HTTP endpoint from the dashboard and set it in your shell:

```bash
export HARNESS_URL="http://127.0.0.1:PORT"
```

Then use:

```bash
curl --fail-with-body -X POST "$HARNESS_URL/test-control/orders" \
  -H 'Content-Type: application/json' \
  -d '{"quantity":1,"unitPrice":2024.994}'

curl --fail-with-body "$HARNESS_URL/test-control/authorizations"

curl --fail-with-body -X POST \
  "$HARNESS_URL/test-control/authorizations/SALES_ORDER_ID/decline" \
  -H 'Content-Type: application/json' \
  -d '{"reason":"manual smoke decline"}'

curl --fail-with-body "$HARNESS_URL/test-control/sagas/SALES_ORDER_ID"

curl --fail-with-body -X POST "$HARNESS_URL/test-control/scenarios/approved" \
  -H 'Content-Type: application/json' -d '{}'

curl --fail-with-body -X POST "$HARNESS_URL/test-control/scenarios/declined" \
  -H 'Content-Type: application/json' -d '{}'
```

Replace `SALES_ORDER_ID` with the numeric value returned when the order was created.

## 6. Read-only SQL cross-check

Set `@SalesOrderID` to a generated ID:

```sql
DECLARE @SalesOrderID int = 900000;

SELECT SalesOrderID, Status, RevisionNumber, Comment
FROM Sales.SalesOrderHeader
WHERE SalesOrderID = @SalesOrderID;

SELECT SagaInstanceId, ProductId, LocationId, Quantity, ReservedAt, ReversedAt
FROM dbo.SalesOrderSagaInventoryAllocation
WHERE SalesOrderID = @SalesOrderID;

SELECT MessageId, EventName, DispatchedAt, DispatchAttemptCount, LastDispatchError, Payload
FROM dbo.SalesOrderSagaOutboxMessage
WHERE MessageId LIKE CONCAT('%sales-order-saga-', @SalesOrderID);

SELECT ProductId, ReferenceOrderId, ReferenceOrderLineId, TransactionType, Quantity
FROM Production.TransactionHistory
WHERE ReferenceOrderId = @SalesOrderID;
```

The header comment must begin `AW-SAGA-HARNESS:`. Stop if it does not; that order is not owned by the harness.

## 7. Guarded cleanup

After diagnosis, run **Delete harness fixtures** from the harness command menu and accept its confirmation prompt. The HTTP equivalent is:

```bash
curl --fail-with-body -X DELETE "$HARNESS_URL/test-control/fixtures?confirm=true"
```

Cleanup selects only headers marked `AW-SAGA-HARNESS:`. It restores any inventory still held by unreversed approved allocations, then removes the marked orders, details, allocation rows, transaction history, and outbox rows. The operation is transactional.

Finally, press `Ctrl+C` in the Aspire terminal. Aspire removes its session-scoped Azurite, Service Bus, and companion SQL resources.

## Troubleshooting

- **Commands are disabled:** wait for `sales-order-functions` and then `saga-test-harness` to become ready.
- **Fixture creation fails:** confirm the AppHost connection secret, DbUp tables, product `776`, and ordinary AdventureWorks customer/address/ship-method/special-offer reference data.
- **Authorization never becomes pending:** inspect `sales-order-functions` logs for the `OrderCreated` trigger and payment HTTP activity.
- **Saga completes but scenario waits:** the outbox timer may need up to one minute; inspect `LastDispatchError` if it exceeds that.
- **A decision returns 400:** the payment authorization has not reached the simulator yet.
- **A decision returns 409:** the authorization already has the opposite terminal decision.
