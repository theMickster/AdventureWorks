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

2. **Run migrations**:
   ```bash
   dotnet run
   ```

### Creating a New Migration

1. Generate timestamp: `YYYYMMDD_HHmmss`
2. Create file in `Scripts/` folder: `20251127_120000_Description.sql`
3. Add migration header and SQL
4. Build project (validates syntax)
5. Test locally: `dotnet run`
6. Verify in database: `SELECT * FROM dbo.SchemaVersions ORDER BY Applied DESC`

### Configuration

- **Local**: User Secrets (recommended)
- **CI/CD**: Environment variable `ConnectionStrings__AdventureWorks`
- **Default**: `appsettings.json` (Integrated Security)

### Migration Tracking

DbUp creates a `SchemaVersions` table to track executed scripts. Scripts run once in alphabetical order.

## Project Structure

```
AdventureWorks.DbUp.Console/
├── Program.cs                  # DbUp runner
├── appsettings.json            # Default config (safe to commit)
├── appsettings.Development.json
├── Scripts/                    # SQL migration scripts (embedded resources)
│   └── (empty - ready for migrations)
└── README.md
```

## Notes

- Target framework: .NET 10
- All SQL scripts are embedded resources
- Transactions enabled by default per script
- Use `-- NoTransaction` header when needed
