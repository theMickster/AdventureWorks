# AdventureWorks.DbReset.Console

CLI that resets a _test-target_ SQL Server database to a known baseline and reapplies DbUp migrations. Built so k6 load tests (Feature #670) and Playwright E2E tests (Feature #671) can start each run from deterministic data without polluting the developer's source DB.

## Getting Started

Run all commands from the **repository root**.

**Step 1 — Set user secrets.** `BaselinePath` is the **container-internal** path — SQL Server resolves it inside `tosk-mssql`. See DOCKER.md for the host path and one-time permission setup.

```bash
dotnet user-secrets set "ConnectionStrings:AdventureWorksDev" "Server=localhost,1433;Database=AdventureWorksDev;User Id=sa;Password=…;Encrypt=false" --project tools/console-apps/AdventureWorks.DbReset.Console
dotnet user-secrets set "ConnectionStrings:AdventureWorks_E2E" "Server=localhost,1433;Database=AdventureWorks_E2E;User Id=sa;Password=…;Encrypt=false" --project tools/console-apps/AdventureWorks.DbReset.Console
dotnet user-secrets set "DbReset:BaselinePath" "/var/opt/mssql/backup/AdventureWorks_baseline.bak" --project tools/console-apps/AdventureWorks.DbReset.Console
```

**Step 2 — Capture a baseline.** Reads `AdventureWorksDev`, writes the `.bak` to `BaselinePath`, and stamps the source-marker extended property on the source DB. Re-run whenever you want to refresh the baseline from your local dev data.

```bash
dotnet run --project tools/console-apps/AdventureWorks.DbReset.Console -- snapshot
```

**Step 3 — Verify the baseline.** Confirms the `.bak` exists and is readable by SQL Server. Exits non-zero if missing or unreadable.

```bash
dotnet run --project tools/console-apps/AdventureWorks.DbReset.Console -- verify-baseline
```

**Step 4 — Reset the target database.** Orchestrates: verify-baseline → restore → migrate. `AdventureWorksDev` is never a valid target by design — see Dual-Role Safety Model below.

```bash
dotnet run --project tools/console-apps/AdventureWorks.DbReset.Console -- reset
dotnet run --project tools/console-apps/AdventureWorks.DbReset.Console -- reset --target AdventureWorks_Test
```

## Verbs

| Verb | What it does |
| --- | --- |
| `verify-baseline` | Confirms the `.bak` exists and is readable by SQL Server |
| `snapshot` | Backs up `SnapshotSource` to `BaselinePath`; stamps source-marker |
| `restore` | Restores `BaselinePath` to the target; drops source-marker |
| `migrate` | Runs `AdventureWorks.DbUp` migrations against the target |
| `reset` | Convenience: verify-baseline → restore → migrate |

`restore`, `migrate`, and `reset` accept `-t, --target <name>` to override `DbReset:DefaultTarget`. `snapshot` has no `--target` — it always reads from `SnapshotSource`.

## Dual-Role Safety Model

The tool refuses to run any destructive verb until five rules pass against the resolved target:

1. `SnapshotSource` and target use **different** `ConnectionStrings` keys.
2. Source and target do not resolve to the same `(Server, Database)` pair.
3. Both `ConnectionStrings` entries exist and are non-empty.
4. The target's database name matches `DbReset:TargetNamePattern` (default opts in names like `AdventureWorks_E2E`).
5. The target does NOT carry the configured source-marker extended property (`dbreset.role = source`).

- **Source-marker mechanism**: `snapshot` stamps `dbreset.role = source` as a SQL extended property on the source DB. `restore` drops it from the restored target — a restored DB cannot carry the marker even if the `.bak` came from a source DB. Rule #5 checks for this property before any destructive verb runs.
- **TargetNamePattern** is matched against `InitialCatalog` (the actual database name from the connection string), not the configuration key. Default: `^AdventureWorks_(E2E|Test|Load)([A-Za-z0-9_]*)?$`.
- **Dev DB safeguard**: `AdventureWorksDev` fails Rule #1 (same key as `SnapshotSource`) AND carries the source-marker after first snapshot — two independent checks prevent it from ever being a target.
- **Baseline drift**: Because `snapshot` reads from the local dev DB, developers with different seed data will have different baselines — this is expected, not a bug; test authors should treat the baseline as the data on the machine that last ran `snapshot`.

## Configuration

`appsettings.json` contains placeholders only. Real values go in user secrets (`dotnet user-secrets`) or environment variables.

```jsonc
{
  "ConnectionStrings": {
    "AdventureWorksDev": "...", // SnapshotSource — read-only
    "AdventureWorks_E2E": "...", // a destructive target
  },
  "DbReset": {
    "SnapshotSource": "AdventureWorksDev",
    "DefaultTarget": "AdventureWorks_E2E",
    "BaselinePath": "/var/opt/mssql/backup/AdventureWorks_baseline.bak", // container-internal path — set via user secrets (see DOCKER.md)
    "TargetNamePattern": "^AdventureWorks_(E2E|Test|Load)([A-Za-z0-9_]*)?$",
    "DbUpProjectPath": "database/dbup/AdventureWorks.DbUp",
    "SourceMarker": { "Property": "dbreset.role", "Value": "source" },
  },
}
```

## Pre-Test Hook Examples

Feature #670 (k6) and Feature #671 (Playwright) authors can invoke `reset` as a pre-test setup step using these patterns.

**npm script (Playwright — Feature #671):** Add this to the E2E project's `package.json` scripts. The path is relative to a project inside `apps/angular-web/`; adjust the depth or use an absolute path as needed.

```json
{
  "scripts": {
    "pretest": "dotnet run --project ../../tools/console-apps/AdventureWorks.DbReset.Console -- reset",
    "test": "npx playwright test"
  }
}
```

**Shell script (k6 or CI — Feature #670):**

```bash
#!/usr/bin/env bash
set -euo pipefail
dotnet run --project tools/console-apps/AdventureWorks.DbReset.Console -- reset
k6 run tests/load/my-scenario.js
```

## Layout

```
AdventureWorks.DbReset.Console/
├── Program.cs          # CommandLineParser MapResult dispatch
├── Verbs/              # 5 verbs + TargetableVerb base + Handlers/
├── Snapshot/           # LocalSqlServerSnapshotProvider, BACKUP/RESTORE SQL
├── Migration/          # DbUpProcessRunner
├── Configuration/      # DbResetOptions, ConfigurationValidator, DbResetDefaults
├── Safety/             # DualRoleSafetyValidator (Rules #1–#5), SqlSourceMarkerProbe
├── Resolution/         # RepoRootResolver, TargetResolver
└── Libs/               # Autofac bootstrapper
```
