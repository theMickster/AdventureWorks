# AdventureWorks.DbReset.Console — Testing Guide

## Verbs

| Verb              | Description                                              |
| ----------------- | -------------------------------------------------------- |
| `verify-baseline` | Checks the configured `.bak` file exists and is readable |
| `snapshot`        | Backs up the source DB to the configured `.bak` path     |
| `restore`         | Restores the `.bak` to the target DB                     |
| `migrate`         | Shells out to AdventureWorks.DbUp via `dotnet run`       |
| `reset`           | Orchestrates: verify-baseline → restore → migrate        |

## Exit codes

| Code | Constant                        | Meaning                                         |
| ---- | ------------------------------- | ----------------------------------------------- |
| 0    | `ExitOk`                        | Success                                         |
| 1    | `ExitParseError`                | CommandLineParser rejected argv                 |
| 2    | `ExitConfigInvalid`             | `appsettings.json` failed validation            |
| 3    | `ExitSafetyRefused`             | Dual-role safety rule violated (Rules #1–#5)    |
| 4    | `ExitRepoRootMissing`           | Repo root marker not found                      |
| 10   | `ExitVerifyBaselineMissing`     | Baseline `.bak` missing or unreadable           |
| 11   | `ExitSnapshotSourceUnreachable` | Snapshot source DB unreachable                  |
| 12   | `ExitSnapshotPermissionDenied`  | SQL Server cannot write the `.bak` (OS error 5) |
| 13   | `ExitRestoreTargetUnreachable`  | Restore target DB unreachable                   |
| 14   | `ExitRestorePermissionDenied`   | SQL Server cannot read the `.bak` (OS error 5)  |
| 15   | `ExitMigrateFailed`             | DbUp exited with non-zero code                  |
| 130  | `ExitCancelled`                 | Operation cancelled (Ctrl+C)                    |

## Prerequisites

### SQL Server container (OrbStack / Docker)

The tool connects to `tosk-mssql` (SQL Server 2025 container). SQL Server runs as the `mssql` user (uid 10001), which has no access to files owned by `root` unless read permission is granted explicitly.

**Grant read access to all `.bak` files in the backup directory** (run once, and again after `snapshot` writes a new `.bak`):

```bash
docker exec --user root tosk-mssql chmod -R o+r /var/opt/mssql/backup/
```

### Configuration

Real credentials live in user secrets, not in `appsettings.json`. The key values that matter:

| Key                                    | Value                                               |
| -------------------------------------- | --------------------------------------------------- |
| `DbReset:BaselinePath`                 | `/var/opt/mssql/backup/AdventureWorks_baseline.bak` |
| `ConnectionStrings:AdventureWorks_E2E` | SQL Auth connection string for the target DB        |
| `ConnectionStrings:AdventureWorksDev`  | SQL Auth connection string for the source DB        |

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

---

### 🛑 TEST #5 — `migrate` Happy Path

**Prerequisites**: `AdventureWorks_E2E` is online. Baseline `.bak` is NOT required for `migrate` alone.

```bash
cd tools/console-apps
dotnet run --project AdventureWorks.DbReset.Console -- migrate
```

**Expected**: DbUp banner + script list stream to terminal. Final line: `migrate complete: AdventureWorks_E2E`. Exit code 0.

Verify in SSMS / Azure Data Studio: `AdventureWorks_E2E.dbo.SchemaVersions` is populated with all migration script names.

---

### 🛑 TEST #6 — `migrate` Safety Refusal

```bash
dotnet run --project AdventureWorks.DbReset.Console -- migrate --target AdventureWorksDev
```

**Expected**: Exit code 3. Stderr contains `Rule1_SameConnectionStringKey`. No DbUp output (no child process spawned).

---

### 🛑 TEST #7 — `reset` Happy Path

**Prerequisites**: `AdventureWorks_E2E` is absent or out-of-date. Valid baseline `.bak` exists.

```bash
cd tools/console-apps
dotnet run --project AdventureWorks.DbReset.Console -- reset
```

**Expected output** (in sequence):

```
Baseline at /var/opt/mssql/backup/AdventureWorks_baseline.bak: X.X GiB on disk
restore complete: AdventureWorks_E2E in X.Xs
[DbUp banner + script list]
migrate complete: AdventureWorks_E2E
reset complete: AdventureWorks_E2E in XX.Xs
```

Exit code 0.

Verify: `AdventureWorks_E2E` is online, `MULTI_USER`, `RECOVERY SIMPLE`, and `dbo.SchemaVersions` is fully populated.

---

### 🛑 TEST #8 — `reset` Baseline Missing

Temporarily rename the `.bak` file, then:

```bash
dotnet run --project AdventureWorks.DbReset.Console -- reset
```

**Expected**: Exit code 10. Stderr contains the baseline path and the `-- snapshot` hint. No SQL restore is attempted.

Restore the `.bak` before continuing.

---

### 🛑 TEST #9 — Ctrl+C During `migrate` Inside `reset`

Start a `reset` and press `Ctrl+C` during the DbUp output phase (after `restore complete:` appears):

```bash
dotnet run --project AdventureWorks.DbReset.Console -- reset
# Press Ctrl+C during DbUp output
```

**Expected**:

- DbUp child process is killed (no zombie processes)
- Exit code 130 (`ExitCancelled`)
- `AdventureWorks_E2E` is not stuck in `SINGLE_USER` (was reset by restore cleanup)

Then run `reset` again to confirm clean recovery:

```bash
dotnet run --project AdventureWorks.DbReset.Console -- reset
```

Second run must complete successfully (exit 0).
