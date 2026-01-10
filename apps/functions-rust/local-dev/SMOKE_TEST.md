# Manual Smoke Test: BOM Cost Engine (Feature 609)

Run from `apps/functions-rust`. Needs Docker, `func` (Core Tools v4), the Rust toolchain, the `tosk-mssql` container (AdventureWorks on `localhost:1433`) and `az login`. Cosmos is the real account (container `bom-cost-results-local`), reached with Entra ID, so your user needs Key Vault secret `get` and the Cosmos data-plane role (see `infra/AZURE_FUNCTIONS_SETUP.md`). Step 9's `scripts/test-live.sh` passed (6 of 6) against the real `bom-cost-results-local` container on 2026-10-04, and the host started with the vault and answered `/api/health` and `/api/bom-cost/771` with 200. Steps 1 to 8, 10 and 11 otherwise were last run against the former Cosmos emulator, which has been removed.

## 1. Start the dependencies

```bash
docker compose -f local-dev/docker-compose.yml up -d
cp local.settings.json.example local.settings.json   # first time only
```

Edit `local.settings.json` first time: set the SQL password and the Azurite account key (the public Azurite key from Microsoft Learn). `KeyVault__VaultUri` supplies the Cosmos endpoint; `Cosmos__Database` and `Cosmos__Container` come from the file.

```bash
docker ps --format '{{.Names}}' | grep '^aw-bom-'
```

Expected: `aw-bom-redis`, `aw-bom-azurite`.

## 2. Start the host

```bash
scripts/start-local.sh --functions bom-cost health
```

Expected: `cargo build` succeeds, the host listens on `http://localhost:7071`, and the first request logs `custom handler listening`. `--functions bom-cost health` skips the Service Bus trigger, so no Service Bus access is needed for steps 3 to 8.

## 3. Health returns 200

```bash
curl -s -i http://localhost:7071/api/health
```

Expected: `200`, body `"status":"healthy"`, `"sql":true`, `"cache":true`. With Redis stopped: still `200`, `"status":"degraded"`. With SQL unreachable: `503`.

## 4. Cost a real product and echo the correlation id

```bash
curl -s -i -H 'x-correlation-id: smoke-1' 'http://localhost:7071/api/bom-cost/771?quantity=2'
```

Expected: `200`, header `x-correlation-id: smoke-1`, `correlationId: "smoke-1"` in the body, and `x-cache: miss` (first call after a TTL expiry or restart).

For `quantity=1`, expect `unitCost.material` `1748.2544`, `unitCost.labor` `49`, `unitCost.total` `1797.2544` (product 771, Mountain-100 Silver, 38; catalogue `standardCost` is `1912.1544`, see CLAUDE.md decision 3).

## 5. Second call is served from Redis

```bash
curl -s -i 'http://localhost:7071/api/bom-cost/771?quantity=2' | grep -i x-cache
```

Expected: `x-cache: hit`.

## 6. What-if returns a delta and is not cached

```bash
curl -s -i 'http://localhost:7071/api/bom-cost/771?quantity=2&override=748:800'
```

Expected: `x-cache: skipped`, `whatIf.applied` lists product `748` at `800`, `whatIf.deltaUnit.total` `52.7998`, `deltaBatch.total` `105.5996`, `deltaPercent` `2.9378`, `ignored` empty.

## 7. `Cache-Control: no-cache` refreshes

```bash
curl -s -i -H 'Cache-Control: no-cache' 'http://localhost:7071/api/bom-cost/771?quantity=2' | grep -i x-cache
curl -s -i 'http://localhost:7071/api/bom-cost/771?quantity=2' | grep -i x-cache
```

Expected: `x-cache: refresh`, then `x-cache: hit`.

## 8. Shortage case

```bash
curl -s -H 'Cache-Control: no-cache' 'http://localhost:7071/api/bom-cost/771?quantity=9'
```

Expected: `feasibility.feasible` is `false`, `componentsChecked` `48`, one shortage: `Paint - Silver` (productId `494`), `required` `72`, `available` `65`, `deficit` `7`.

## 9. Batch to Cosmos

Without Service Bus (the handler's batch logic against the real `bom-cost-results-local` container, same as the trigger would call it; needs `az login`):

```bash
scripts/test-live.sh
```

Expected: `6 passed; 0 failed`, including `batch_message_upserts_each_pair_into_cosmos_with_deterministic_ids` and `batch_with_one_bad_pair_fails_the_message_after_storing_the_good_one`.

The live Service Bus trigger runs against the real DEV queue `bom-batch-requests-dev` (no emulator), with your `az login`. It needs the roles Azure Service Bus Data Receiver and Data Sender on that queue. `local.settings.json` sets `ServiceBusConnection__fullyQualifiedNamespace` and `BomBatchQueueName=bom-batch-requests-dev`.

```bash
scripts/start-local.sh      # no --functions filter, so the trigger is registered
```

Expected: the host lists `bom-batch: serviceBusTrigger` and logs a receive link for `bom-batch-requests-dev`. Send a message:

```bash
az rest --method post --resource https://servicebus.azure.net --url "https://mick-adventureworks-events.servicebus.windows.net/bom-batch-requests-dev/messages?api-version=2017-04" --headers "Content-Type=application/json" --body '{"items":[{"productId":771,"quantity":3}]}'
```

Expected: `201`, then `Executed 'Functions.bom-batch' (Succeeded ...)` and a `batch processed` log line, and the message is completed. A malformed body such as `{"items":"not-a-list"}` fails 3 times (queue max delivery 3) and lands in the dead-letter queue. Run on 2026-10-04: the good message succeeded in 5.5 s and the malformed one dead-lettered after 3 failed executions. Document `bom:771:5` was then read back from the real container over the Cosmos data plane (HTTP 200). The DEV queue is shared with any future DEV Function App, and a dead-lettered test message must be removed by hand.

## 10. RabbitMQ messaging tests (optional)

```bash
docker compose -f local-dev/docker-compose.yml --profile rabbitmq up -d rabbitmq
scripts/test-live.sh   # also runs tests/rabbitmq_flow.rs when 127.0.0.1:5675 is listening
```

Expected: `tests/rabbitmq_flow.rs` reports 2 passed. A good message is acknowledged on the first delivery and document `bom:771:11` lands in Cosmos; a malformed message is delivered 3 times and then sits in the dead-letter queue. A test-only consumer plays the Functions host. This verifies the handler's retry and dead-letter contract, not the Service Bus trigger binding, which stays unverified until it runs against the Service Bus emulator or the real namespace. Management UI: http://localhost:15672 (user `aw`, local-only password from the compose file).

## 11. Run the Postman collection

```bash
npx --yes newman run postman/collections/aw-bom-cost.postman_collection.json -e postman/environments/local.example.postman_environment.json
```

Expected: `requests` `10`, `assertions` `37`, `failed` `0`. Leave `functionKey` empty: the local host does not enforce function keys.

## 12. Stop

```bash
# Ctrl+C the start-local.sh terminal
docker compose -f local-dev/docker-compose.yml down
```

Add `--profile rabbitmq` to the `down` command if you started that profile. Do not stop `tosk-mssql`; it is managed independently.
