# AdventureWorks DbUp Architecture

## Overview

Clean, modular DbUp application supporting **two deployment patterns**:

1. **One-time migrations** (Scripts folder) - schema changes tracked in journal
2. **Always-run programmable objects** (ProgrammableObjects folder) - deployed every time in alphabetical order

## Project Structure

```
AdventureWorks.DbUp/
├── Configuration/
│   └── DbConnectionProvider.cs       # Connection string management
│
├── Deployers/
│   ├── MigrationDeployer.cs          # One-time migration executor
│   └── ProgrammableObjectsDeployer.cs # Always-run object executor
│
├── Scripts/                          # One-time migrations (embedded)
│   ├── 20250701_1000_BASELINE_FullTextIndexes.sql
│   ├── 20250701_1001_BASELINE_DatabasePrincipals.sql
│   └── ...
│
├── ProgrammableObjects/              # Always-run definitions (embedded)
│   ├── dbo/
│   ├── Person/
│   ├── HumanResources/
│   ├── Production/
│   ├── Purchasing/
│   └── Sales/
│
├── Program.cs                        # Thin orchestrator
├── appsettings.json
└── AdventureWorks.DbUp.csproj
```

## Execution Flow

```
Program.cs
  ├─> DbConnectionProvider.BuildConfiguration()
  ├─> DbConnectionProvider.GetConnectionString()
  │
  ├─> MigrationDeployer.Deploy()
  │     ├─> Filters: Scripts embedded in assembly (.Scripts.)
  │     ├─> Journal: dbo.DatabaseMigrationHistory
  │     ├─> Behavior: Run once, skip if already executed
  │     └─> Order: Alphabetical (timestamp-based filenames)
  │
  └─> ProgrammableObjectsDeployer.Deploy()
        ├─> Filters: ProgrammableObjects embedded in assembly (.ProgrammableObjects.)
        ├─> Journal: NullJournal (no tracking)
        ├─> Behavior: Run always (ScriptType.RunAlways)
        └─> Order: Alphabetical (folder/file structure)
```

## Key Design Decisions

### 1. Separation of Concerns

- **Configuration**: `DbConnectionProvider` handles all configuration loading
- **Deployment Logic**: Separate deployer classes for each pattern
- **Orchestration**: `Program.cs` is a thin coordinator (~50 lines)

### 2. Embedded Resources

Both `Scripts` and `ProgrammableObjects` folders are embedded as resources:

```xml
<EmbeddedResource Include="Scripts\**\*.sql" />
<EmbeddedResource Include="ProgrammableObjects\**\*.sql" />
```

This ensures:

- No external file dependencies at runtime
- Single executable deployment
- Consistent resource naming for filtering

### 3. Filtering Strategy

- **Migrations**: `script => script.Contains(".Scripts.")`
- **Programmable Objects**: `script => script.Contains(".ProgrammableObjects.")`

Embedded resource names follow pattern: `{AssemblyName}.{FolderPath}.{FileName}`

Example: `AdventureWorks.DbUp.Scripts.20250701_1000_BASELINE_FullTextIndexes.sql`

### 4. Programmable Objects Execution

```csharp
.WithScriptsEmbeddedInAssembly(
    Assembly.GetExecutingAssembly(),
    script => script.Contains(".ProgrammableObjects."),
    new SqlScriptOptions {
        ScriptType = ScriptType.RunAlways,  // Execute every deployment
        RunGroupOrder = 1                    // Run after migrations (order 0)
    })
.JournalTo(new NullJournal())                // Don't track in database
```

**Key Points**:

- `ScriptType.RunAlways` ensures execution on every run
- `NullJournal` prevents journal tracking (would re-execute infinitely otherwise)
- Alphabetical ordering maintained by embedded resource naming
- Each script runs in its own transaction

### 5. Idempotency

Programmable objects must be idempotent (DROP/CREATE or CREATE OR ALTER pattern):

```sql
DROP PROCEDURE IF EXISTS [Person].[sp_DeletePerson_Temporal];
GO
CREATE PROCEDURE [Person].[sp_DeletePerson_Temporal]
    @BusinessEntityID INT
AS
BEGIN
    -- Implementation
END
GO
```

## Usage

### Running Locally

```bash
cd AdventureWorks.DbUp
dotnet run
```

**⚠️ First-time setup with existing database?** See [Baseline Setup for Existing Databases](#baseline-setup-for-existing-databases) section below to seed the journal table before your first run.

### Expected Output

```
═══════════════════════════════════════════════════════════════
  AdventureWorks Database Deployment (DbUp)
═══════════════════════════════════════════════════════════════
Target Database: AdventureWorks

Running one-time migrations (Scripts folder)...
─────────────────────────────────────────────────
Beginning database upgrade
Checking whether journal table exists..
Fetching list of already executed scripts.
No new scripts need to be executed - completing.
✓ Migrations completed successfully!

Deploying programmable objects (ProgrammableObjects folder)...
───────────────────────────────────────────────────────────────
Beginning database upgrade
Executing Database Server script 'AdventureWorks.DbUp.ProgrammableObjects.Person.sp_DeletePerson_Temporal.sql'
Executing Database Server script 'AdventureWorks.DbUp.ProgrammableObjects.Person.sp_UpdatePerson_Temporal.sql'
[... all programmable objects ...]
✓ Programmable objects deployed successfully!

═══════════════════════════════════════════════════════════════
  ✓ Database deployment completed successfully!
═══════════════════════════════════════════════════════════════
```

## Configuration

### Connection Strings

Priority order (highest to lowest):

1. **Environment variables** (CI/CD) - **Highest priority, overrides all other sources**
2. **User Secrets** (local dev)
3. **appsettings.{Environment}.json**
4. **appsettings.json** - **Lowest priority, provides defaults**

**Note:** In .NET's ConfigurationBuilder, sources added later override values from earlier sources. Since environment variables are added last in `DbConnectionProvider.BuildConfiguration()`, they have the highest priority.

**Local Development**:

```bash
dotnet user-secrets set "ConnectionStrings:AdventureWorks" "Server=localhost;Database=AdventureWorks;Integrated Security=true;TrustServerCertificate=true;"
```

**CI/CD**:

```bash
export ConnectionStrings__AdventureWorks="Server=...;Database=...;"
```

## Baseline Setup for Existing Databases

**⚠️ IMPORTANT:** If deploying to an **existing AdventureWorks database** that already has the baseline objects (schemas, tables, stored procedures), you **MUST** seed the journal table **BEFORE** your first deployment run.

### Why Baseline Seeding is Required

The AdventureWorks database already contains:

- All schemas (Person, Sales, HumanResources, Production, etc.)
- All tables, indexes, and constraints
- Many stored procedures, views, and functions

The baseline migration scripts (`20250701_1000` through `20250701_1006`) were created to represent these existing objects. If DbUp tries to run these scripts against an existing database, they will **fail** because the objects already exist.

### How to Baseline an Existing Database

**Step 1: Ensure the journal table exists**

Run the DbUp application once - it will create `dbo.DatabaseMigrationHistory` table:

```bash
dotnet run
```

The first run will fail on baseline scripts (expected), but it creates the journal table.

**Step 2: Seed the journal with baseline migrations**

Execute these INSERT statements against your AdventureWorks database:

```sql
-- Mark baseline migrations as already applied
INSERT INTO dbo.DatabaseMigrationHistory (ScriptName, Applied)
VALUES ('AdventureWorks.DbUp.Scripts.20250701_1000_BASELINE_FullTextIndexes.sql', GETDATE());

INSERT INTO dbo.DatabaseMigrationHistory (ScriptName, Applied)
VALUES ('AdventureWorks.DbUp.Scripts.20250701_1001_BASELINE_DatabasePrincipals.sql', GETDATE());

INSERT INTO dbo.DatabaseMigrationHistory (ScriptName, Applied)
VALUES ('AdventureWorks.DbUp.Scripts.20250701_1002_BASELINE_Schemas.sql', GETDATE());

INSERT INTO dbo.DatabaseMigrationHistory (ScriptName, Applied)
VALUES ('AdventureWorks.DbUp.Scripts.20250701_1003_BASELINE_Objects.sql', GETDATE());

INSERT INTO dbo.DatabaseMigrationHistory (ScriptName, Applied)
VALUES ('AdventureWorks.DbUp.Scripts.20250701_1004_BASELINE_ExtendedProperties.sql', GETDATE());

INSERT INTO dbo.DatabaseMigrationHistory (ScriptName, Applied)
VALUES ('AdventureWorks.DbUp.Scripts.20250701_1005_BASELINE_GrantPermissions.sql', GETDATE());

INSERT INTO dbo.DatabaseMigrationHistory (ScriptName, Applied)
VALUES ('AdventureWorks.DbUp.Scripts.20250701_1006_ALTER_Indexes.sql', GETDATE());
```

**Step 3: Verify baseline seeding**

```sql
-- Should show 7 baseline migrations
SELECT ScriptName, Applied
FROM dbo.DatabaseMigrationHistory
ORDER BY Applied;
```

**Step 4: Run deployment again**

```bash
dotnet run
```

**Expected output:**

```
Running one-time migrations (Scripts folder)...
─────────────────────────────────────────────────
Beginning database upgrade
Checking whether journal table exists..
Fetching list of already executed scripts.
No new scripts need to be executed - completing.
✓ Migrations completed successfully!

Deploying programmable objects (ProgrammableObjects folder)...
───────────────────────────────────────────────────────────────
[Programmable objects will deploy successfully]
```

### When NOT to Baseline

**Do NOT seed the journal if:**

- Deploying to a **new/empty database** - let DbUp run all migrations
- Database was created by this DbUp project from scratch
- You want to recreate the database from baseline scripts

### Troubleshooting

**Problem:** "Object already exists" errors during first run

- **Solution:** Follow baseline seeding steps above

**Problem:** Journal table doesn't exist

- **Solution:** Run `dotnet run` once to create it (even if migrations fail)

**Problem:** Unsure if database needs baselining

- **Check:** `SELECT COUNT(*) FROM sys.tables WHERE schema_id = SCHEMA_ID('Person')`
- **If > 0:** Database has objects, needs baselining
- **If = 0:** Database is empty, no baselining needed

## Adding New Scripts

### One-Time Migration

1. Create file in `Scripts/` folder: `YYYYMMDD_HHMM_Description.sql`
2. Add DbUp header with purpose, changes, rollback strategy
3. Build project (validates embedded resource)
4. Run: `dotnet run`
5. Verify in journal: `SELECT * FROM dbo.DatabaseMigrationHistory`

### Programmable Object

1. Create file in `ProgrammableObjects/{Schema}/{ObjectType}/` folder
2. Use idempotent DROP/CREATE pattern
3. Build project
4. Run: `dotnet run` - will deploy on every execution

## Testing

### Test Against Copy Database

```sql
-- Create test database
RESTORE DATABASE [AdventureWorks_Test]
FROM DISK = 'C:\Backup\AdventureWorks.bak';

-- Update connection string
dotnet user-secrets set "ConnectionStrings:AdventureWorks" "Server=localhost;Database=AdventureWorks_Test;..."

-- Run deployment
dotnet run

-- Verify
-- Run again to verify idempotency
dotnet run
```

## Error Handling

- Connection failures: Exit code -1, red error message
- Migration failures: Exit code -1, rollback transaction
- Programmable object failures: Exit code -1, rollback transaction
- Configuration errors: Exit code -1, exception message

## Future Enhancements

1. **Pre/Post Deployment Scripts**: Add separate folders with different `RunGroupOrder`
2. **Conditional Execution**: Filter programmable objects by environment
3. **Parallel Execution**: Deploy non-dependent objects concurrently
4. **Validation Mode**: Dry-run that outputs SQL without executing
5. **Rollback Generation**: Auto-generate compensating migrations

## References

- DbUp Documentation: https://dbup.readthedocs.io/
- Script Types: https://dbup.readthedocs.io/en/latest/usage/#script-types
- Journal: https://dbup.readthedocs.io/en/latest/more-info/journaling/
