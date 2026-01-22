# DbUp Migration Project - AdventureWorks

`database/dbup/AdventureWorks.DbUp` evolves the AdventureWorks SQL Server database with version-controlled SQL scripts. This file covers what the code cannot say. Setup and baseline seeding are in `database/dbup/AdventureWorks.DbUp/README.md`; deployer design is in `database/dbup/AdventureWorks.DbUp/ARCHITECTURE.md`.

## Two Deployers, Two Folders

`Program.cs` runs `Deployers/MigrationDeployer.cs` first, then `Deployers/ProgrammableObjectsDeployer.cs`. Both embed `*.sql` from their folder, run each script in its own transaction (`WithTransactionPerScript()`), and run scripts in ordinal order of their embedded resource names (folder spaces become underscores, and lowercase `dbo` sorts after the PascalCase schemas). The two deployers are separate engines, so `RunGroupOrder` has no effect; only `Program.cs` sequences them. There is no per-script transaction opt-out marker; a script that cannot run inside a transaction (the full-text baseline, for example) is commented out and applied by hand.

**`Scripts/`** holds one-time, journaled migrations (`dbo.DatabaseMigrationHistory`). Flat folder, no subfolders. Name every file `{YYYYMMDD}_{HHmm}_{ACTION}_{Description}.sql`. The timestamp is only a sort key: pick one later than every script it must run after, and a future date is fine. Break a same-minute tie only with a trailing letter (`20250702_1008a_CREATE_PersonType_Table.sql`; `1008_` sorts before `1008a`, so the lettered script runs after). Do not use a seconds suffix for ties: ordering is ordinal, so `161130_` sorts before `1611_` and the new script would run first. The one six-digit script (`20260503_161100_...`) has no same-minute neighbour. Actions in use: `BASELINE`, `CREATE`, `ALTER`, `SEED`.

**`ProgrammableObjects/{Schema}/{Object Type}/`** holds always-run definitions (`RunAlways`, `NullJournal`): stored procedures, views, functions, and database triggers. One object per file, named after the object. Each file drops and recreates (`IF OBJECT_ID(...) IS NOT NULL DROP ...` + `CREATE`) or uses `CREATE OR ALTER`. Never put a procedure, view, function, or trigger in `Scripts/`: a journaled script runs once, so later edits would never deploy.

## Idempotency in `Scripts/`

Every migration must be safely re-runnable. Wrap DDL in an existence check:

```sql
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Employee' AND schema_id = SCHEMA_ID('HumanResources'))
BEGIN
    CREATE TABLE [HumanResources].[Employee] ([BusinessEntityID] INT PRIMARY KEY)
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('[HumanResources].[Employee]') AND name = 'EmailVerified')
BEGIN
    ALTER TABLE [HumanResources].[Employee] ADD [EmailVerified] BIT NOT NULL DEFAULT 0
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Employee_NationalID' AND object_id = OBJECT_ID('[HumanResources].[Employee]'))
BEGIN
    CREATE INDEX [IX_Employee_NationalID] ON [HumanResources].[Employee]([NationalIDNumber])
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'CK_Employee_BirthDate')
BEGIN
    ALTER TABLE [HumanResources].[Employee] ADD CONSTRAINT [CK_Employee_BirthDate] CHECK ([BirthDate] < GETDATE())
END
GO
```

Seed data with `MERGE ... WHEN NOT MATCHED BY TARGET` or `INSERT` guarded by `IF NOT EXISTS`. Data migrations need a `WHERE` that excludes already-migrated rows.

## Rules

- Forward only. No rollback scripts; write a compensating migration instead.
- One logical change per script. Do not mix schema, data, and programmable objects.
- Schema-qualify every object name: `[Schema].[Object]`.
- Separate batches with `GO`. DDL and the statements that depend on it belong in different batches.
- No `DROP TABLE` without an explicit archival step in the same story.
- No `SELECT *` in procedures or views; list columns.
- No hard-coded connection strings or server names in scripts.
- Breaking changes (rename, type change, drop column) are multi-phase: add new, backfill, switch the app, then drop old in a later script.

## Verify Before Handing Off

Run from the project folder, twice. `appsettings.json` is loaded from the current directory, so `dotnet run --project` from the repo root fails before connecting. The second run must report no pending migrations and still redeploy every programmable object without error:

```bash
cd database/dbup/AdventureWorks.DbUp
dotnet run
dotnet run
```

The Aspire dashboard's `dbup` resource (explicit start) runs the same thing from the project folder.

Verify migrations by running DbUp, not by applying SQL by hand. The `.claude/hooks/block-mutating-sql.sh` PreToolUse hook denies `sqlcmd` calls made through the read-only `querying-adventureworks-database` skill when they contain `CREATE`, `ALTER`, `INSERT`, or any other mutating keyword. A hand-applied change would also bypass the journal.

Deploying to an existing AdventureWorks database for the first time requires seeding the journal with the seven `20250701_*` baseline scripts first, or DbUp will try to recreate objects that already exist. The exact `INSERT` is in the README.
