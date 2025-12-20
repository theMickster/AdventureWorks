# DbUp Migration Project - AdventureWorks

**Purpose**: Database evolution using DbUp for the AdventureWorks SQL Server database. This project manages schema changes, data migrations, and stored procedures through version-controlled SQL scripts.

## Core Principles

1. **IDEMPOTENCY IS MANDATORY** - Every script must be safely re-runnable
2. **Forward Only** - No rollback scripts, create compensating migrations instead
3. **One Logical Change Per Script** - Don't mix concerns (schema + data + sprocs)
4. **Test Locally First** - Run against local DB before committing
5. **Script Order Matters** - Dependencies must be resolved sequentially

## Project Structure

```
database/dbup/
├── AdventureWorks.DbUp/
│   ├── Scripts/                     # Migration scripts (EmbeddedResources), flat — no subfolders
│   │   ├── 20250701_1000_BASELINE_FullTextIndexes.sql
│   │   ├── 20250702_1007_ALTER_add_BusinessEntity_IsEntraUser.sql
│   │   └── 20260605_1202_CREATE_dbo_ActivityLog.sql
│   ├── ProgrammableObjects/          # Always-run objects (views, procs, functions), by schema
│   ├── Program.cs                    # DbUp engine configuration
│   └── AdventureWorks.DbUp.csproj
└── README.md
```

**Rules:**

- Scripts live flat in `Scripts/` — no version subfolders.
- Scripts named: `{YYYYMMDD}_{HHmm}_{ACTION}_{Description}.sql`
- All scripts marked as `EmbeddedResource` in .csproj
- DbUp runs scripts in filename (timestamp) order

## Migration Script Standards

### Naming Convention

```
Scripts/
  20250701_1002_BASELINE_Schemas.sql
  20250702_1008a_CREATE_PersonType_Table.sql
  20250702_1008b_SEED_PersonType_Data.sql
  20260510_1200_CREATE_StoreSalesPersonHistory_Table.sql
```

**Pattern:** `{YYYYMMDD}_{HHmm}_{ACTION}_{Description}.sql` — the timestamp is the sequencing mechanism; pick a time later than every script it must run after. A trailing letter (`1008a`, `1008b`) breaks ties between scripts created in the same session.

### IDEMPOTENCY - Non-Negotiable Patterns

#### Check Before Create

```sql
-- Tables
IF NOT EXISTS (SELECT * FROM sys.tables
               WHERE name = 'Employee' AND schema_id = SCHEMA_ID('HumanResources'))
BEGIN
    CREATE TABLE [HumanResources].[Employee] (
        [BusinessEntityID] INT PRIMARY KEY,
        [NationalIDNumber] NVARCHAR(15) NOT NULL
    )
END
GO

-- Columns
IF NOT EXISTS (SELECT * FROM sys.columns
               WHERE object_id = OBJECT_ID('[HumanResources].[Employee]')
               AND name = 'EmailVerified')
BEGIN
    ALTER TABLE [HumanResources].[Employee]
    ADD [EmailVerified] BIT NOT NULL DEFAULT 0
END
GO

-- Indexes
IF NOT EXISTS (SELECT * FROM sys.indexes
               WHERE name = 'IX_Employee_NationalID'
               AND object_id = OBJECT_ID('[HumanResources].[Employee]'))
BEGIN
    CREATE INDEX [IX_Employee_NationalID]
    ON [HumanResources].[Employee]([NationalIDNumber])
END
GO

-- Constraints
IF NOT EXISTS (SELECT * FROM sys.check_constraints
               WHERE name = 'CK_Employee_BirthDate')
BEGIN
    ALTER TABLE [HumanResources].[Employee]
    ADD CONSTRAINT [CK_Employee_BirthDate]
    CHECK ([BirthDate] < GETDATE())
END
GO

-- Stored Procedures (DROP/CREATE is idempotent)
IF EXISTS (SELECT * FROM sys.objects
           WHERE name = 'uspGetEmployee' AND type = 'P')
BEGIN
    DROP PROCEDURE [HumanResources].[uspGetEmployee]
END
GO

CREATE PROCEDURE [HumanResources].[uspGetEmployee]
    @EmployeeID INT
AS
BEGIN
    SELECT * FROM [HumanResources].[Employee]
    WHERE [BusinessEntityID] = @EmployeeID
END
GO
```

### Script Categories - Separate Concerns

#### 1. Schema Changes (DDL)

- Tables, columns, constraints, indexes
- One object type per script when possible
- Always schema-qualified: `[Schema].[Object]`

```sql
-- Good: 001_CreateEmployeeTable.sql
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Employee' AND schema_id = SCHEMA_ID('HumanResources'))
BEGIN
    CREATE TABLE [HumanResources].[Employee] (
        [BusinessEntityID] INT IDENTITY(1,1) PRIMARY KEY,
        [NationalIDNumber] NVARCHAR(15) NOT NULL,
        [LoginID] NVARCHAR(256) NOT NULL,
        [JobTitle] NVARCHAR(50) NOT NULL,
        [BirthDate] DATE NOT NULL,
        [HireDate] DATE NOT NULL,
        [ModifiedDate] DATETIME NOT NULL DEFAULT GETUTCDATE()
    )
END
GO
```

#### 2. Data Migrations (DML)

- Moving, transforming, or updating existing data
- Use WHERE clauses to prevent duplicate updates
- Consider batching for large tables

```sql
-- Good: 002_MigrateEmployeeEmails.sql
-- Migrate email format from old to new system
UPDATE [Person].[EmailAddress]
SET [EmailAddress] = LOWER([EmailAddress])
WHERE [EmailAddress] != LOWER([EmailAddress])
    AND [EmailAddressID] IN (
        SELECT TOP 1000 [EmailAddressID]
        FROM [Person].[EmailAddress]
        WHERE [EmailAddress] != LOWER([EmailAddress])
    )
GO
```

#### 3. Reference Data (Static Inserts)

- Lookup tables, configuration data
- Use MERGE or INSERT with NOT EXISTS

```sql
-- Good: 003_SeedDepartments.sql
IF NOT EXISTS (SELECT * FROM [HumanResources].[Department] WHERE [DepartmentID] = 1)
BEGIN
    INSERT INTO [HumanResources].[Department] ([DepartmentID], [Name], [GroupName])
    VALUES (1, 'Engineering', 'Research and Development')
END
GO

-- Better: Use MERGE for multiple rows
MERGE [HumanResources].[Department] AS target
USING (VALUES
    (1, 'Engineering', 'Research and Development'),
    (2, 'Tool Design', 'Research and Development')
) AS source ([DepartmentID], [Name], [GroupName])
ON target.[DepartmentID] = source.[DepartmentID]
WHEN NOT MATCHED BY TARGET THEN
    INSERT ([DepartmentID], [Name], [GroupName])
    VALUES (source.[DepartmentID], source.[Name], source.[GroupName]);
GO
```

#### 4. Stored Procedures / Functions / Views

- DROP/CREATE pattern (inherently idempotent)
- Separate script per object
- Include parameter documentation in comments

```sql
-- Good: 004_CreateGetEmployeeProc.sql
IF EXISTS (SELECT * FROM sys.objects WHERE name = 'uspGetEmployeesByDepartment' AND type = 'P')
BEGIN
    DROP PROCEDURE [HumanResources].[uspGetEmployeesByDepartment]
END
GO

CREATE PROCEDURE [HumanResources].[uspGetEmployeesByDepartment]
    @DepartmentID INT,
    @ActiveOnly BIT = 1
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        e.[BusinessEntityID],
        e.[NationalIDNumber],
        e.[JobTitle],
        e.[HireDate]
    FROM [HumanResources].[Employee] e
    INNER JOIN [HumanResources].[EmployeeDepartmentHistory] edh
        ON e.[BusinessEntityID] = edh.[BusinessEntityID]
    WHERE edh.[DepartmentID] = @DepartmentID
        AND (@ActiveOnly = 0 OR edh.[EndDate] IS NULL)
END
GO
```

## Common Patterns & Solutions

### Adding Columns with Defaults

```sql
-- Non-nullable with default
IF NOT EXISTS (SELECT * FROM sys.columns
               WHERE object_id = OBJECT_ID('[Sales].[Store]') AND name = 'IsActive')
BEGIN
    ALTER TABLE [Sales].[Store]
    ADD [IsActive] BIT NOT NULL DEFAULT 1
END
GO
```

### Renaming Columns (Two-Step Migration)

```sql
-- Step 1: Add new column (001_AddNewColumn.sql)
IF NOT EXISTS (SELECT * FROM sys.columns
               WHERE object_id = OBJECT_ID('[Person].[Person]') AND name = 'FullName')
BEGIN
    ALTER TABLE [Person].[Person]
    ADD [FullName] NVARCHAR(150) NULL
END
GO

-- Step 2: Migrate data (002_MigrateToNewColumn.sql)
UPDATE [Person].[Person]
SET [FullName] = [FirstName] + ' ' + [LastName]
WHERE [FullName] IS NULL
GO

-- Step 3: Drop old column (003_DropOldColumn.sql) - Only after app updated!
IF EXISTS (SELECT * FROM sys.columns
           WHERE object_id = OBJECT_ID('[Person].[Person]') AND name = 'FirstName')
BEGIN
    ALTER TABLE [Person].[Person]
    DROP COLUMN [FirstName], [LastName]
END
GO
```

### Modifying Column Types

```sql
-- Safe approach: Create new, migrate, drop old
-- 001_AddNewColumnWithNewType.sql
IF NOT EXISTS (SELECT * FROM sys.columns
               WHERE object_id = OBJECT_ID('[Sales].[SalesOrderHeader]')
               AND name = 'OrderDateNew')
BEGIN
    ALTER TABLE [Sales].[SalesOrderHeader]
    ADD [OrderDateNew] DATETIME2 NULL
END
GO

-- 002_MigrateData.sql
UPDATE [Sales].[SalesOrderHeader]
SET [OrderDateNew] = CAST([OrderDate] AS DATETIME2)
WHERE [OrderDateNew] IS NULL
GO

-- 003_DropOldRenameNew.sql (After app deployment)
IF EXISTS (SELECT * FROM sys.columns
           WHERE object_id = OBJECT_ID('[Sales].[SalesOrderHeader]')
           AND name = 'OrderDate')
BEGIN
    -- Drop constraints/indexes on old column first
    ALTER TABLE [Sales].[SalesOrderHeader] DROP COLUMN [OrderDate]
    EXEC sp_rename '[Sales].[SalesOrderHeader].[OrderDateNew]', 'OrderDate', 'COLUMN'
END
GO
```

### Creating Indexes on Large Tables

```sql
-- Use ONLINE = ON for minimal blocking (Enterprise Edition)
IF NOT EXISTS (SELECT * FROM sys.indexes
               WHERE name = 'IX_SalesOrderDetail_ProductID')
BEGIN
    CREATE NONCLUSTERED INDEX [IX_SalesOrderDetail_ProductID]
    ON [Sales].[SalesOrderDetail]([ProductID])
    INCLUDE ([OrderQty], [UnitPrice])
    WITH (ONLINE = ON) -- Remove if Standard Edition
END
GO
```

## DON'Ts - Critical Anti-Patterns

- ❌ **NO bare DDL** - Always wrap in IF NOT EXISTS checks
- ❌ **NO hard-coded connection strings** - Use config/environment variables
- ❌ **NO DROP TABLE** without explicit backup/archival strategy
- ❌ **NO mixing schema and data** in same script (separate concerns)
- ❌ **NO SELECT \* in stored procedures** - Explicit column lists only
- ❌ **NO missing GO statements** - Each batch must be separated
- ❌ **NO unqualified object names** - Always use [Schema].[Object]
- ❌ **NO assumptions about data** - Check for existence before UPDATE/DELETE
- ❌ **NO breaking changes** without coordination - Multi-phase migrations

## Transaction Boundaries

**DbUp runs each script in a transaction by default.**

```sql
-- Each script is implicitly wrapped in:
BEGIN TRANSACTION
    -- Your script content
COMMIT TRANSACTION
```

**Use GO to separate batches** (commits transaction between batches):

```sql
CREATE TABLE [dbo].[Example] ([Id] INT)
GO  -- Transaction commits here

CREATE PROCEDURE [dbo].[uspExample] AS SELECT * FROM [dbo].[Example]
GO  -- New transaction
```

**Opt-out of transactions** for specific scripts (add comment at top):

```sql
-- DbUp:NoTransaction
-- Use for operations that can't run in transactions (e.g., full-text indexes)
CREATE FULLTEXT INDEX ON [Production].[Product]([Name])
GO
```

## Testing & Verification

### Before Committing a Migration

1. **Run against local development database**

   ```
   dotnet run --project src/AdventureWorks.DbUp
   ```

2. **Run twice to verify idempotency**

   ```
   dotnet run --project src/AdventureWorks.DbUp
   dotnet run --project src/AdventureWorks.DbUp  # Should succeed, no changes
   ```

3. **Verify schema changes**

   ```sql
   -- Check object exists
   SELECT * FROM sys.tables WHERE name = 'YourTable'
   SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('[Schema].[Table]')
   ```

4. **Test with rollback scenario** - Create a compensating migration if needed

### Script Checklist

- [ ] IF NOT EXISTS checks present
- [ ] Schema-qualified names `[Schema].[Object]`
- [ ] GO statements between batches
- [ ] Marked as EmbeddedResource in .csproj
- [ ] Tested locally and runs twice successfully
- [ ] No hard-coded values (connection strings, server names)
- [ ] Proper sequence number in filename
- [ ] Comments explain WHY, not WHAT

## DbUp Program.cs Configuration

```csharp
var connectionString = configuration.GetConnectionString("AdventureWorks");

var upgrader = DeployChanges.To
    .SqlDatabase(connectionString)
    .WithScriptsEmbeddedInAssembly(Assembly.GetExecutingAssembly())
    .WithTransactionPerScript()  // Default: each script in own transaction
    .LogToConsole()
    .Build();

var result = upgrader.PerformUpgrade();

if (!result.Successful)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine(result.Error);
    return -1;
}

Console.ForegroundColor = ConsoleColor.Green;
Console.WriteLine("Success!");
return 0;
```

## When Creating New Migrations

### Decision Tree

**Adding new table?**
→ Create: `{YYYYMMDD}_{HHmm}_CREATE_{Entity}Table.sql`
→ Include: Primary key, required columns, defaults
→ Next script: Add indexes, foreign keys (later timestamp, same day is fine)

**Modifying existing column?**
→ Create: New column → Migrate data → Drop old (3 scripts)
→ Coordinate with application deployment

**Adding reference data?**
→ Use MERGE pattern for idempotency
→ Separate script from schema changes

**Creating stored procedure?**
→ DROP IF EXISTS, then CREATE
→ One procedure per script

**Performance concern?**
→ Use ONLINE = ON for index creation
→ Batch large data updates (TOP 1000 in loop)

## Examples for Common Scenarios

### Scenario 1: New Feature - Employee Skills

```
Scripts/
  20260710_0900_CREATE_Skill_Table.sql          # Reference data table
  20260710_0901_CREATE_EmployeeSkill_Table.sql  # Junction table
  20260710_0902_CREATE_EmployeeSkill_Indexes.sql
  20260710_0903_SEED_Skill_Defaults.sql
```

### Scenario 2: Enhance Existing - Add Audit Fields

```
Scripts/
  20260710_1000_ALTER_Employee_Add_CreatedBy.sql
  20260710_1001_ALTER_Employee_Add_ModifiedBy.sql
  20260710_1002_BACKFILL_Employee_AuditFields.sql   # Populate existing rows
  20260710_1003_ALTER_Employee_AuditFields_NotNull.sql  # After backfill complete
```

### Scenario 3: Performance - Add Indexes

```
Scripts/
  20260710_1100_CREATE_SalesOrderHeader_OrderDate_Index.sql
  20260710_1101_CREATE_Customer_Name_Index.sql
  20260710_1102_UPDATE_Statistics.sql
```

## Key Takeaways

1. **Idempotency is not optional** - Every script runs in any environment safely
2. **Schema-qualify everything** - `[Schema].[Object]` always
3. **One concern per script** - Tables → Data → Procedures
4. **Test twice locally** - Second run should be no-op
5. **GO separates batches** - Critical for DDL commands
6. **Forward only** - No rollbacks, create compensating migrations
7. **Flat, timestamp-named `Scripts/`** - No version subfolders; filename timestamp is the run order

## Questions to Ask Before Writing a Migration

1. Can this script run multiple times safely? (Idempotent?)
2. Does this break existing application code? (Coordinate deployment)
3. Will this lock tables in production? (Consider ONLINE operations)
4. Is data at risk? (Backup strategy needed?)
5. Are dependencies resolved? (Foreign keys, procedures referencing tables)
6. Does the timestamp sort after every script it depends on?

---

**Remember:** Migrations are permanent. Code carefully. Test thoroughly. Deploy confidently.
