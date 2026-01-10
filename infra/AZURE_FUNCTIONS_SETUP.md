# Azure Functions Setup: Rust BOM Cost Engine

This describes the Cosmos DB and Service Bus names the Rust BOM Function (`apps/functions-rust/`) uses per environment, what to adjust on the existing items, and what to create. Nothing here is applied automatically: `az` commands run only when you approve them. So far the three Cosmos containers `bom-cost-results`, `bom-cost-results-dev` and `bom-cost-results-local` exist (created with the commands below); DEV topic `sales-order-events-dev` with its two subscriptions (with filters), the queues `bom-batch-requests` and `bom-batch-requests-dev`, and the PROD subscription `sales-order-payment-results` exist; the filters (rules) in the Filters and rules section are in place; nothing else has been run, including the optional PROD topic fixes below. State was read earlier with read-only `az` calls on subscription MickKnowledgeServices, so re-check before acting.

DEV and PROD share the existing Cosmos account and Service Bus namespace and are separated by name suffix (PROD unsuffixed), like the `Beers` / `Beers-Dev` / `Beers-QA` containers and `envSuffix` in `main.bicep`.

## Names by environment

| Item                  | Local                                             | DEV                             | PROD                         |
| --------------------- | ------------------------------------------------- | ------------------------------- | ---------------------------- |
| Cosmos account        | `mickcosmos`                                      | `mickcosmos`                    | `mickcosmos`                 |
| Cosmos database       | `PlatformDatabases`                               | `PlatformDatabases`             | `PlatformDatabases`          |
| Cosmos container      | `bom-cost-results-local`                          | `bom-cost-results-dev`          | `bom-cost-results`           |
| Partition key         | `/productId`                                      | `/productId`                    | `/productId`                 |
| Service Bus namespace | `mick-adventureworks-events`                      | `mick-adventureworks-events`    | `mick-adventureworks-events` |
| Queue                 | `bom-batch-requests-dev`                          | `bom-batch-requests-dev`        | `bom-batch-requests`         |
| Topic                 | `sales-order-events`                              | `sales-order-events-dev`        | `sales-order-events`         |
| Subscriptions         | `sales-order-saga`, `sales-order-payment-results` | same names, under the DEV topic | same names                   |

DEV and PROD are separated on the topic: DEV has its own topic `sales-order-events-dev`, created with duplicate detection and the same filter expressions as the Bicep (under the rule names in Filters and rules), so DEV messages never reach PROD consumers. Subscription names are per topic, so DEV reuses the PROD names. On the PROD topic, the saga subscription's `1=1` rule was replaced by `OrderCreatedOnly` and the `sales-order-payment-results` subscription was added; the topic itself is unchanged apart from the optional items below.

App settings (Azure values; the local values are in `local.settings.json.example`):

| Setting                                         | DEV                                                 | PROD                 |
| ----------------------------------------------- | --------------------------------------------------- | -------------------- |
| `Cosmos__Endpoint`                              | `https://mickcosmos.documents.azure.com:443/`       | same                 |
| `Cosmos__Database`                              | `PlatformDatabases`                                 | `PlatformDatabases`  |
| `Cosmos__Container`                             | `bom-cost-results-dev`                              | `bom-cost-results`   |
| `Cosmos__PreferredRegion`                       | `West US 3`                                         | `West US 3`          |
| `ServiceBusSalesOrderEventsTopicName`           | `sales-order-events-dev`                            | `sales-order-events` |
| `BomBatchQueueName`                             | `bom-batch-requests-dev`                            | `bom-batch-requests` |
| `ServiceBusConnection__fullyQualifiedNamespace` | `mick-adventureworks-events.servicebus.windows.net` | same                 |

Set `KeyVault__VaultUri` (`https://mickkeyvaultwestus.vault.azure.net/`) and the Function reads `Cosmos__Endpoint` from the vault secret `AzureCosmosDbAccountUri`, replacing the app setting of the same name. It authenticates to the vault and to Cosmos with Entra ID (managed identity in Azure, `az login` locally), never with a key: `AzureCosmosDbSecurityKey` and `AzureCosmosDbDatabaseName` are not read and the handler has no key setting. The vault `mickkeyvaultwestus` uses access policies (not RBAC), so the identity needs a policy with secret permission `get`: `az keyvault set-policy -n mickkeyvaultwestus --object-id <principal-id> --secret-permissions get`. It also needs a Cosmos data-plane role: `az cosmosdb sql role assignment create -a mickcosmos -g Mick-West-US-3-CosmosDb --role-definition-id 00000000-0000-0000-0000-000000000002 --principal-id <principal-id> --scope "/"` (Built-in Data Contributor; scope to `/dbs/PlatformDatabases/colls/<container>` to limit it to one container). The developer login already holds a Cosmos data-plane role and a vault policy; the Function App's managed identity does not exist yet. `Cosmos__Database` and `Cosmos__Container` are required app settings, not vault secrets. Without `KeyVault__VaultUri`, `Cosmos__Endpoint` must be set directly. `bom-batch/function.json` reads the queue from `%BomBatchQueueName%`; the Aspire AppHost keeps the literal local name.

## Adjustments to existing items

Each changes something that exists today. Run or decline each one. Resource groups: Cosmos `Mick-West-US-3-CosmosDb`, Service Bus `AdventureWorks-West-US-3`.

- [x] PROD subscription `sales-order-saga` had a `1=1` rule that matched every message, while `serviceBus.bicep` expects `sys.Label = 'OrderCreated'`. It now has the filter `OrderCreatedOnly` (see Filters and rules below), so the saga receives only `OrderCreated` messages. The old `1=1` rule is gone. The saga code already ignores any other subject. I did not find the API code that publishes `OrderCreated`, so confirm it sets the message label (`Subject`) before relying on this filter.

```bash
az servicebus topic subscription rule create -g AdventureWorks-West-US-3 --namespace-name mick-adventureworks-events --topic-name sales-order-events --subscription-name sales-order-saga --name OrderCreatedOnly --filter-type SqlFilter --filter-sql-expression "sys.Label = 'OrderCreated'"
```

- [ ] PROD topic `sales-order-events` (the DEV topic is already correct) lacks the duplicate detection (10 minutes) and `P1D` TTL that the Bicep sets. Duplicate detection is immutable, so the fix is delete and recreate, which destroys its subscriptions and in-flight messages. Optional.

```bash
az servicebus topic update -g AdventureWorks-West-US-3 --namespace-name mick-adventureworks-events -n sales-order-events --default-message-time-to-live P1D
```

The TTL alone can be updated in place as above; duplicate detection needs `az servicebus topic delete` then `az servicebus topic create ... --enable-duplicate-detection true --duplicate-detection-history-time-window PT10M`.

- [ ] Cosmos database `PlatformDatabases` has shared autoscale capped at 1000 RU/s (the free-tier allowance). No change: both new containers draw from it, so they share that budget with everything else. For Entra auth instead of a key, the identity needs the data-plane role `Cosmos DB Built-in Data Contributor` (`00000000-0000-0000-0000-000000000002`).

```bash
az cosmosdb sql role assignment create -a mickcosmos -g Mick-West-US-3-CosmosDb --role-definition-id 00000000-0000-0000-0000-000000000002 --principal-id <principal-id> --scope "/"
```

## New items to create

Cosmos containers (`bom-cost-results-local` too, for local development against the real account), no dedicated throughput (they use the shared pool). The partition key is a single-level `/productId`, version 2, because every access is a point read or upsert by `(productId, id)`. Hierarchical keys were rejected: the id already encodes the quantity, so a second key level gives one document per key and no benefit. Indexing excludes every path because only point reads and upserts are used; any future query must first add the paths it filters on.

```bash
echo '{"indexingMode":"consistent","automatic":true,"includedPaths":[],"excludedPaths":[{"path":"/*"}]}' > /tmp/bom-idx.json
az cosmosdb sql container create -a mickcosmos -g Mick-West-US-3-CosmosDb -d PlatformDatabases -n bom-cost-results --partition-key-path /productId --partition-key-version 2 --idx @/tmp/bom-idx.json
az cosmosdb sql container create -a mickcosmos -g Mick-West-US-3-CosmosDb -d PlatformDatabases -n bom-cost-results-dev --partition-key-path /productId --partition-key-version 2 --idx @/tmp/bom-idx.json
az cosmosdb sql container create -a mickcosmos -g Mick-West-US-3-CosmosDb -d PlatformDatabases -n bom-cost-results-local --partition-key-path /productId --partition-key-version 2 --idx @/tmp/bom-idx.json
```

Service Bus queues (both created). Max delivery 3 matches `apps/functions-rust/local-dev/Config.json`.

```bash
az servicebus queue create -g AdventureWorks-West-US-3 --namespace-name mick-adventureworks-events -n bom-batch-requests --lock-duration PT1M --max-delivery-count 3 --default-message-time-to-live P1D --enable-dead-lettering-on-message-expiration true
az servicebus queue create -g AdventureWorks-West-US-3 --namespace-name mick-adventureworks-events -n bom-batch-requests-dev --lock-duration PT1M --max-delivery-count 3 --default-message-time-to-live P1D --enable-dead-lettering-on-message-expiration true
```

Subscription `sales-order-payment-results` on the PROD topic (created; the DEV topic has it too). Its filter is the rule `PaymentResultsOnly`, created on both topics:

```bash
az servicebus topic subscription create -g AdventureWorks-West-US-3 --namespace-name mick-adventureworks-events --topic-name sales-order-events -n sales-order-payment-results --default-message-time-to-live P1D --max-delivery-count 10 --enable-dead-lettering-on-message-expiration true
az servicebus topic subscription rule create -g AdventureWorks-West-US-3 --namespace-name mick-adventureworks-events --topic-name sales-order-events --subscription-name sales-order-payment-results --name PaymentResultsOnly --filter-type SqlFilter --filter-sql-expression "sys.Label IN ('PaymentApproved', 'PaymentDeclined')"
az servicebus topic subscription rule create -g AdventureWorks-West-US-3 --namespace-name mick-adventureworks-events --topic-name sales-order-events-dev --subscription-name sales-order-payment-results --name PaymentResultsOnly --filter-type SqlFilter --filter-sql-expression "sys.Label IN ('PaymentApproved', 'PaymentDeclined')"
```

## Filters and rules

A subscription's filter is a rule: the Azure portal calls them **Filters** (bottom of the subscription's Overview page), while the CLI, the REST API and `serviceBus.bicep` call them **rules**. A subscription with no rule receives no messages. Each rule here is a SQL filter on `sys.Label`, which is the message `Subject` the publisher sets.

| Topic                    | Subscription                  | Rule                 | Filter                                                |
| ------------------------ | ----------------------------- | -------------------- | ----------------------------------------------------- |
| `sales-order-events`     | `sales-order-saga`            | `OrderCreatedOnly`   | `sys.Label = 'OrderCreated'`                          |
| `sales-order-events`     | `sales-order-payment-results` | `PaymentResultsOnly` | `sys.Label IN ('PaymentApproved', 'PaymentDeclined')` |
| `sales-order-events-dev` | `sales-order-saga`            | `OrderCreatedOnly`   | `sys.Label = 'OrderCreated'`                          |
| `sales-order-events-dev` | `sales-order-payment-results` | `PaymentResultsOnly` | `sys.Label IN ('PaymentApproved', 'PaymentDeclined')` |

Naming: do not name these rules `$Default`. Rules created under that name did not persist, while rules with other names did. `serviceBus.bicep` still uses `$Default`, so a Bicep deployment would need the rules renamed to match.

Verify rules with the data plane, which is what routes messages (replace the topic and subscription):

```bash
az rest --method get --resource https://servicebus.azure.net --url "https://mick-adventureworks-events.servicebus.windows.net/sales-order-events/subscriptions/sales-order-saga/rules?api-version=2017-04"
```

## Unverified

- That the rules listed under Filters and rules are still present after time has passed (checked once, right after creation).
- The API publisher of `OrderCreated`: not found in `apps/api-dotnet/src`, so it is unconfirmed that it sets the label the saga filter matches.
- The Service Bus trigger against the PROD queue and a managed identity. Locally, the host resolved `%BomBatchQueueName%` to `bom-batch-requests-dev` and ran a message end to end with your developer login (2026-10-04, see `apps/functions-rust/local-dev/SMOKE_TEST.md`).
- Managed-identity auth against real Cosmos and Key Vault, including the role assignments above, which have not been made. Your developer login has been confirmed locally: the host read the vault and the live tests wrote to and read back from `bom-cost-results-local`.
- The `West US 3` value for `Cosmos__PreferredRegion`.
- Container TTL with the index excluded (no TTL is set above).
