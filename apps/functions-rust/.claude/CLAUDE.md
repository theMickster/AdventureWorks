# AdventureWorks Rust Functions

Feature 609 (BOM cost and feasibility engine), an Azure Functions custom handler on axum. Logic is in the `aw_bom_cost` lib; read the rustdoc on `domain::explode` first. This file lists only what the code cannot tell you.

- **Costing:** a made item's `StandardCost` already includes its subtree and raw parts are priced at 0, so each branch is priced at its first priced node and the rest is `shadowed`. Summing leaves or adding labor to a priced assembly double counts or drops parts. Computed cost intentionally differs from the catalogue `StandardCost`.
- **Labor:** `WorkOrderRouting.PlannedCost` is a fixed per-operation figure (no variation by order quantity in this data), so it is averaged per `(ProductID, OperationSequence)` and treated as per-unit.
- **Availability:** checked on tree leaves only, against `SUM(ProductInventory.Quantity)` across all locations. Stock held as intermediate assemblies is not counted.
- **Redis cache:** invalidation is TTL-only (`BomCache__TtlSeconds`); results can be one TTL stale and callers send `no-cache` for fresh data. Redis was built now although the Feature said deferred to #1000. What-if requests never touch the cache.
- **SQL:** `tiberius` 0.13 with a hand-written `bb8` manager, because `bb8-tiberius` 0.16 needs tiberius 0.12 and gives incompatible types.
- **Batch:** any failed pair must return non-2xx so the host retries and the queue dead-letters it; never return 2xx on partial failure.
- **Cosmos:** `azure_data_cosmos` is a pinned beta. There is no Cosmos emulator and no key auth: local runs use the real container `bom-cost-results-local` with Entra ID (`az login`), and containers are never created by the handler. Set `Cosmos__PreferredRegion` to the account's region in Azure. With `KeyVault__VaultUri` set, the endpoint comes from Key Vault; database and container are required settings with no defaults. Azure setup: `infra/AZURE_FUNCTIONS_SETUP.md`.
- **Ports** avoid other projects on this machine: Redis 6380, Azurite 11000-11002, RabbitMQ 5675 (5672 is taken).
- **RabbitMQ tests** (`tests/rabbitmq_flow.rs`, opt-in compose profile) verify the handler's retry and dead-letter contract only, not the Service Bus trigger binding.
- **Unverified:** managed-identity auth in Azure (developer-login auth to the real Cosmos, Key Vault and DEV queue works locally), Premium-plan behavior, and CI (never executed).
- **Don't:** add `bb8-tiberius`, cache what-if results, log or `Debug`-print `Settings` (holds secrets), or commit `local.settings.json` or real Postman environments.
- Every limit and fixed name goes in `limits.rs`; `domain/` stays free of I/O.
