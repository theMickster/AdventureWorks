# AdventureWorks.DbReset.Console — Testing Guide

## Prerequisites

### SQL Server container (OrbStack / Docker)

The tool connects to `tosk-mssql` (SQL Server 2025 container). SQL Server runs as the `mssql` user (uid 10001), which has no access to files owned by `root` unless read permission is granted explicitly.

**Grant read access to all `.bak` files in the backup directory** (run once, and again after `snapshot` writes a new `.bak`):

```bash
docker exec --user root tosk-mssql chmod -R o+r /var/opt/mssql/backup/
```

### Configuration

Real credentials live in user secrets, not in `appsettings.json`. The key values that matter:

| Key | Value |
| --- | ----- |
| `DbReset:BaselinePath` | `/var/opt/mssql/backup/AdventureWorks_baseline.bak` |
| `ConnectionStrings:AdventureWorks_E2E` | SQL Auth connection string for the target DB |
| `ConnectionStrings:AdventureWorksDev` | SQL Auth connection string for the source DB |

`BaselinePath` must be the **container-internal path** — SQL Server resolves it inside the container, not on the host. The host path for the same file is:
`/Users/mickletofsky/OrbStack/docker/volumes/sql_tosk_mssql_2025_data/backup/AdventureWorks_baseline.bak`

---

## Running the tests

All commands run from `tools/console-apps/`:

```bash
dotnet test AdventureWorks.DbReset.Console.Tests -- xUnit.MaxParallelThreads=1
```

---

## Manual test checkpoints

### 🛑 TEST #1 — Happy Path

```bash
cd tools/console-apps
dotnet run --project AdventureWorks.DbReset.Console -- restore
```

**Expected**: stdout shows `restore complete: AdventureWorks_E2E in X.Xs`. Exit code 0.

Verify in SSMS / Azure Data Studio: `AdventureWorks_E2E` is online, `MULTI_USER`, `RECOVERY SIMPLE`.

---

### 🛑 TEST #2 — Safety Refusal

```bash
dotnet run --project AdventureWorks.DbReset.Console -- restore --target AdventureWorksDev
```

**Expected**: Exit code 3. Stderr contains `Rule1_SameConnectionStringKey`. No SQL is sent.

---

### 🛑 TEST #3 — Baseline Missing

Temporarily rename or move the `.bak` file, then:

```bash
dotnet run --project AdventureWorks.DbReset.Console -- restore
```

**Expected**: Exit code 10. Stderr contains the baseline path and the `-- snapshot` hint.

Restore the `.bak` before continuing.

---

### 🛑 TEST #4 — Ctrl+C Self-Heal

Start a restore and press `Ctrl+C` mid-restore (within the first ~5 seconds):

```bash
dotnet run --project AdventureWorks.DbReset.Console -- restore
# Ctrl+C immediately — before any completion output
```

**Expected**: Exit non-zero. DB is not stuck in `SINGLE_USER` or `RESTORING` — confirm in SSMS/ADS.

Run restore again to confirm self-heal clears any stuck `RESTORING` state:

```bash
dotnet run --project AdventureWorks.DbReset.Console -- restore
```

Second run must complete successfully (exit 0, DB online).
