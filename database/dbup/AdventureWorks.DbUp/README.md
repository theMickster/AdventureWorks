# AdventureWorks DbUp Console

Database migration tool for AdventureWorks using DbUp.

## Quick Start

### Prerequisites
- .NET 10 SDK
- SQL Server (local or remote)
- AdventureWorks database

### Local Development Setup

1. **Configure connection string** using User Secrets:
   ```bash
   dotnet user-secrets set "ConnectionStrings:AdventureWorks" "Server=localhost;Database=AdventureWorks;Integrated Security=true;TrustServerCertificate=true;"
   ```

2. **⚠️ IMPORTANT - Baseline Setup for Existing Database:**

   If you're deploying to an **existing AdventureWorks database**, you must seed the journal table first:

   ```sql
   -- Run these INSERT statements against your database:
   INSERT INTO dbo.DatabaseMigrationHistory (ScriptName, Applied)
   VALUES
   ('AdventureWorks.DbUp.Scripts.20250701_1000_BASELINE_FullTextIndexes.sql', GETDATE()),
   ('AdventureWorks.DbUp.Scripts.20250701_1001_BASELINE_DatabasePrincipals.sql', GETDATE()),
   ('AdventureWorks.DbUp.Scripts.20250701_1002_BASELINE_Schemas.sql', GETDATE()),
   ('AdventureWorks.DbUp.Scripts.20250701_1003_BASELINE_Objects.sql', GETDATE()),
   ('AdventureWorks.DbUp.Scripts.20250701_1004_BASELINE_ExtendedProperties.sql', GETDATE()),
   ('AdventureWorks.DbUp.Scripts.20250701_1005_BASELINE_GrantPermissions.sql', GETDATE()),
   ('AdventureWorks.DbUp.Scripts.20250701_1006_ALTER_Indexes.sql', GETDATE());
   ```

   **See [ARCHITECTURE.md - Baseline Setup](./ARCHITECTURE.md#baseline-setup-for-existing-databases) for detailed instructions.**

3. **Run deployment**:
   ```bash
   dotnet run
   ```

### Creating a New Migration

1. Generate timestamp: `YYYYMMDD_HHmm` (pick a time later than every script it must run after)
2. Create file in `Scripts/` folder: `{YYYYMMDD}_{HHmm}_{ACTION}_{Description}.sql`, e.g. `20260510_1200_CREATE_StoreSalesPersonHistory_Table.sql`. Break a same-minute tie with a trailing letter (`20250702_1008a_...`), never a seconds suffix (ordinal sort puts `161130_` before `1611_`)
3. Add migration header and SQL
4. Build project (validates syntax)
5. Test locally: `dotnet run`
6. Verify in database: `SELECT * FROM dbo.DatabaseMigrationHistory ORDER BY Applied DESC`

### Configuration

- **Local**: User Secrets (recommended)
- **CI/CD**: Environment variable `ConnectionStrings__AdventureWorks`
- **Default**: `appsettings.json` (Integrated Security)

### Migration Tracking

DbUp creates a `dbo.DatabaseMigrationHistory` table to track executed migrations.
- **Scripts folder**: One-time migrations, run once, tracked in journal
- **ProgrammableObjects folder**: Always-run objects (views, procedures, functions), deployed every time

## Project Structure

```
AdventureWorks.DbUp/
├── Configuration/
│   └── DbConnectionProvider.cs       # Connection string management
├── Deployers/
│   ├── MigrationDeployer.cs          # One-time migration executor
│   └── ProgrammableObjectsDeployer.cs # Always-run object executor
├── Scripts/                          # One-time migrations (embedded)
│   └── YYYYMMDD_HHmm_ACTION_Description.sql
├── ProgrammableObjects/              # Always-run definitions (embedded)
│   ├── Database Triggers/
│   ├── dbo/
│   ├── Person/
│   ├── HumanResources/
│   ├── Production/
│   ├── Purchasing/
│   └── Sales/
├── Program.cs                        # Thin orchestrator
├── appsettings.json
├── ARCHITECTURE.md                   # Detailed documentation
└── README.md
```

## Notes

- Target framework: .NET 10
- All SQL scripts are embedded resources
- Each script runs in its own transaction; there is no per-script opt-out
