-- Replaces the legacy PersonType nchar(2) column on Person.Person with a
-- normalized FK to Person.PersonType. Steps: add column, backfill from legacy
-- code, enforce FK + NOT NULL, then drop the old column and its check constraint.
-- See: database/docs/EnhancePersonType.md

-- Step 1: Add PersonTypeId as nullable (required before backfill)
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('[Person].[Person]') AND name = 'PersonTypeId')
BEGIN
    ALTER TABLE [Person].[Person] ADD [PersonTypeId] INTEGER NULL;
END
GO

-- Step 2: Backfill PersonTypeId from the legacy PersonType code (only if legacy column still exists)
IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('[Person].[Person]') AND name = 'PersonType')
BEGIN
    UPDATE p
    SET    p.[PersonTypeId] = pt.[PersonTypeId]
    FROM   [Person].[Person] p
           INNER JOIN [Person].[PersonType] pt ON p.[PersonType] = pt.[PersonTypeCode]
    WHERE  p.[PersonTypeId] IS NULL
END
GO

-- Step 3: Add FK constraint
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Person_PersonTypeId' AND parent_object_id = OBJECT_ID('[Person].[Person]'))
BEGIN
    ALTER TABLE [Person].[Person]
        WITH CHECK ADD CONSTRAINT [FK_Person_PersonTypeId]
        FOREIGN KEY ([PersonTypeId]) REFERENCES [Person].[PersonType] ([PersonTypeId])
END
GO

-- Step 4: Make PersonTypeId NOT NULL (backfill must be complete first)
IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('[Person].[Person]') AND name = 'PersonTypeId' AND is_nullable = 1)
BEGIN
    ALTER TABLE [Person].[Person] ALTER COLUMN [PersonTypeId] INTEGER NOT NULL;
END
GO

-- Step 5: Drop the legacy check constraint
IF EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'CK_Person_PersonType' AND parent_object_id = OBJECT_ID('[Person].[Person]'))
BEGIN
    ALTER TABLE [Person].[Person] DROP CONSTRAINT [CK_Person_PersonType]
END
GO

-- Step 6: Drop the legacy PersonType column
IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('[Person].[Person]') AND name = 'PersonType')
BEGIN
    ALTER TABLE [Person].[Person] DROP COLUMN [PersonType];
END
GO
