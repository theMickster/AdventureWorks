# DbUp SQL Migration Project Instructions

**Location**: `src/backend/database/dbup/`  
**Purpose**: Manage SQL schema migrations for AdventureWorks database using DbUp.

---

## 🧠 Chain-of-Thought Requirement

**CRITICAL**: When performing ANY DbUp task, use explicit numbered chain-of-thought reasoning.

**Example**:
```
Step 1: Analyze current schema and required changes
Step 2: Determine transaction requirement (default: yes, unless -- NoTransaction needed)
Step 3: Generate timestamp filename: YYYYMMDD_HHmmss_Description.sql
Step 4: Create script with header template
Step 5: Write idempotent SQL with IF NOT EXISTS checks
Step 6: Set Build Action to EmbeddedResource
Step 7: Test locally against dev database
```

Apply this to: Creating migrations, setup, troubleshooting, configuration, testing.

---

## 📁 Project Structure

```
src/backend/database/dbup/
├── AdventureWorks.DbUp.Console/
│   ├── Program.cs
│   ├── appsettings.json              # No secrets
│   ├── Scripts/
│   │   ├── 20251127_100000_EnhancePersonType.sql
│   │   └── ...
│   └── AdventureWorks.DbUp.Console.csproj
```

**Solution**: Add to `AdventureWorks.ConsoleApps.sln`  
**Target**: `net9.0`

---

## 🎯 Core Principles

1. **One-time execution**: Scripts run once, tracked in `SchemaVersions` table
2. **Lexical ordering**: Execute by filename alphabetical order (timestamp ensures order)
3. **Forward-only**: No rollbacks; create compensating migrations
4. **Transactional by default**: Use `-- NoTransaction` only when required
5. **Immutable**: Never edit executed scripts; create new ones

---

## 📝 Script Standards

### Filename Convention
```
YYYYMMDD_HHmmss_BriefDescription.sql

Examples:
20251127_103045_EnhancePersonTypeTable.sql
20251127_143000_AddEmailAddressIndex.sql
```

### Required Header Template

```sql
/*
 * Migration: [One-line description]
 * Date: YYYY-MM-DD
 * Author: [Name]
 * Issue: [Link or ticket]
 * 
 * Purpose:
 * [WHY this change is needed - business/technical context]
 * 
 * Changes:
 * - [Specific change 1]
 * - [Specific change 2]
 * 
 * Rollback Strategy:
 * [How to undo: new migration filename or manual steps]
 */
```

### Idempotency Pattern

```sql
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'MyTable')
BEGIN
    CREATE TABLE dbo.MyTable (...);
END

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Person_Email')
BEGIN
    CREATE INDEX IX_Person_Email ON Person.Person(EmailAddress);
END

IF COL_LENGTH('Person.PersonType', 'IsActive') IS NULL
BEGIN
    ALTER TABLE Person.PersonType ADD IsActive bit NOT NULL DEFAULT 1;
END
```

### Code Standards
- Schema-qualified names: `Person.PersonType`, `dbo.MyTable`
- Keywords UPPERCASE, objects PascalCase
- 4-space indent, 120 char line length
- `-- NoTransaction` header only when transaction not supported

---

## 🏗️ Console Application

### Program.cs Pattern

```csharp
using DbUp;
using Microsoft.Extensions.Configuration;
using System.Reflection;

var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false)
    .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true)
    .AddUserSecrets<Program>(optional: true)
    .AddEnvironmentVariables()
    .Build();

var connectionString = configuration.GetConnectionString("AdventureWorks");

if (string.IsNullOrEmpty(connectionString))
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine("Connection string 'AdventureWorks' not found.");
    Console.ResetColor();
    return -1;
}

var upgrader = DeployChanges.To
    .SqlDatabase(connectionString)
    .WithScriptsEmbeddedInAssembly(Assembly.GetExecutingAssembly())
    .LogToConsole()
    .WithTransactionPerScript()
    .Build();

var result = upgrader.PerformUpgrade();

if (!result.Successful)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine($"Migration failed: {result.Error}");
    Console.ResetColor();
    return -1;
}

Console.ForegroundColor = ConsoleColor.Green;
Console.WriteLine("Migration completed successfully!");
Console.ResetColor();
return 0;
```

### Required NuGet Packages

```xml
<PackageReference Include="DbUp" Version="5.0.*" />
<PackageReference Include="Microsoft.Extensions.Configuration" Version="9.0.*" />
<PackageReference Include="Microsoft.Extensions.Configuration.Json" Version="9.0.*" />
<PackageReference Include="Microsoft.Extensions.Configuration.UserSecrets" Version="9.0.*" />
<PackageReference Include="Microsoft.Extensions.Configuration.EnvironmentVariables" Version="9.0.*" />
```

### Embed SQL Scripts

```xml
<ItemGroup>
  <EmbeddedResource Include="Scripts\**\*.sql" />
</ItemGroup>
```

---

## 🔒 Security

**Rules**:
- Never commit connection strings with credentials
- Use User Secrets for local dev: `dotnet user-secrets set "ConnectionStrings:AdventureWorks" "..."`
- Use environment variables or Key Vault for CI/CD
- Prefer integrated security or managed identity

**appsettings.json** (safe to commit):
```json
{
  "ConnectionStrings": {
    "AdventureWorks": "Server=localhost;Database=AdventureWorks;Integrated Security=true;TrustServerCertificate=true;"
  }
}
```

---

## 🧪 Testing Workflow

**Chain-of-Thought**:
```
Step 1: Restore local AdventureWorks database
Step 2: Run: dotnet run (in DbUp console project)
Step 3: Check SchemaVersions table for new entry
Step 4: Validate schema change (query sys.tables, sys.columns, sys.indexes)
Step 5: Test application compatibility (run API if available)
Step 6: Optional: Restore DB and re-run to verify idempotency
Step 7: Commit script
```

**Validation Query**:
```sql
SELECT * FROM dbo.SchemaVersions ORDER BY Applied DESC;
```

---

## 🎯 Common Patterns

### Add Column
```sql
IF COL_LENGTH('Person.PersonType', 'PreferredName') IS NULL
BEGIN
    ALTER TABLE Person.PersonType 
    ADD PreferredName nvarchar(50) NULL;
END
```

### Create Index
```sql
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_PersonType_Name')
BEGIN
    CREATE INDEX IX_PersonType_Name ON Person.PersonType(Name);
END
```

### Data Migration
```sql
IF EXISTS (SELECT 1 FROM Person.Person WHERE PersonType IS NULL)
BEGIN
    UPDATE Person.Person
    SET PersonType = 'GC'
    WHERE PersonType IS NULL;
    
    PRINT 'Updated ' + CAST(@@ROWCOUNT AS nvarchar) + ' rows';
END
```

### Stored Procedure
```sql
IF OBJECT_ID('dbo.GetPersonsByType', 'P') IS NULL
    EXEC('CREATE PROCEDURE dbo.GetPersonsByType AS SELECT 1');
GO

ALTER PROCEDURE dbo.GetPersonsByType @PersonType nvarchar(2)
AS
BEGIN
    SELECT BusinessEntityID, FirstName, LastName
    FROM Person.Person
    WHERE PersonType = @PersonType;
END
GO
```

---

## 📋 Copilot Behavioral Rules

When working on DbUp tasks, Copilot MUST:

1. **Use chain-of-thought**: Always break tasks into numbered steps
2. **Generate timestamps**: Use UTC for script filenames (YYYYMMDD_HHmmss)
3. **Include headers**: Never skip migration header template
4. **Write idempotent SQL**: Use existence checks unless impossible
5. **Set EmbeddedResource**: Ensure .csproj includes SQL as embedded
6. **No hardcoded secrets**: Use configuration providers
7. **Schema-qualify objects**: Always use schema prefix
8. **Target .NET 9**: Match main API framework
9. **Document rollback**: Include undo strategy in header
10. **Test reminder**: Prompt user to validate locally before commit

---

## ✅ Quick Checklist

**Creating Migration**:
- [ ] Chain-of-thought planning
- [ ] Timestamp filename generated
- [ ] Complete header with rollback strategy
- [ ] Idempotency checks included
- [ ] EmbeddedResource in .csproj
- [ ] Build succeeds
- [ ] Tested locally
- [ ] SchemaVersions table updated

**Console App Setup**:
- [ ] .NET 9 target
- [ ] DbUp + configuration packages
- [ ] User Secrets configured
- [ ] Scripts embedded in assembly
- [ ] Exit codes for success/failure
