# AdventureWorks.DbReset.Console

CLI that resets a _test-target_ SQL Server database to a known baseline and reapplies DbUp migrations. Built so k6 load tests (Feature #670) and Playwright E2E tests (Feature #671) can start each run from deterministic data without polluting the developer's source DB.

> **Status:** Scaffolding only — Story #924 of Feature #923. Verbs are stubs that print and exit 0. Real SQL behavior lands in Stories #925–#928. Full Getting Started docs land in Story #929.

## Verbs

| Verb              | Status | Lands in |
| ----------------- | ------ | -------- |
| `verify-baseline` | stub   | #926     |
| `snapshot`        | stub   | #926     |
| `restore`         | stub   | #927     |
| `migrate`         | stub   | #928     |
| `reset`           | stub   | #928     |

`restore`, `migrate`, and `reset` accept `-t, --target <name>` to override the configured `DbReset:DefaultTarget`.

## Build & test

From the project directory (`tools/console-apps/AdventureWorks.DbReset.Console/`):

```bash
dotnet build
dotnet run -- --help
```

Tests live in the sibling project (`tools/console-apps/AdventureWorks.DbReset.Console.Tests/`):

```bash
dotnet test ../AdventureWorks.DbReset.Console.Tests -- xUnit.MaxParallelThreads=1
```

The pre-existing `AdventureWorks.Testing.Console` and `AdventureWorks.UserSetup.Console` projects in this folder have unrelated NU1605 EF/BCrypt downgrade errors at the solution level; build the DbReset csprojs directly to bypass.

## Dual-role safety model

The tool refuses to run any destructive verb until five rules pass against the resolved target:

1. `SnapshotSource` and target use **different** `ConnectionStrings` keys.
2. Source and target do not resolve to the same `(Server, Database)` pair.
3. Both `ConnectionStrings` entries exist and are non-empty.
4. The target's database name matches `DbReset:TargetNamePattern` (default opts in names like `AdventureWorks_E2E`).
5. The target does NOT carry the configured source-marker extended property (`dbreset.role = source`).

Source-marker stamping happens during `snapshot` (Story #926). Until then, Rule #5 is enforced via a stub probe that always returns `false`.

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
    "BaselinePath": "/var/opt/mssql/backup/AdventureWorks_Baseline.bak",
    "TargetNamePattern": "^AdventureWorks_(E2E|Test|Load)([A-Za-z0-9_]*)?$",
    "DbUpProjectPath": "database/dbup/AdventureWorks.DbUp",
    "SourceMarker": { "Property": "dbreset.role", "Value": "source" },
  },
}
```

## Layout

```
AdventureWorks.DbReset.Console/
├── Program.cs                     # CommandLineParser MapResult dispatch
├── Verbs/                         # 5 stub verbs + TargetableVerb base
├── Configuration/                 # DbResetOptions, ConfigurationValidator
├── Safety/                        # DualRoleSafetyValidator + 5 rules
├── Resolution/                    # RepoRootResolver, TargetResolver
└── Libs/                          # Autofac bootstrapper
```
