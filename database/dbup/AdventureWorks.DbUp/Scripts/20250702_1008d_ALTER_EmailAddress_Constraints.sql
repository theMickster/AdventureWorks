-- Replaces the composite PK on Person.EmailAddress with a single-column PK on
-- EmailAddressID, and adds a UNIQUE CLUSTERED constraint on (BusinessEntityID, EmailAddressID).
-- See: database/docs/EnhanceEmailAddressInformation.md

-- Step 1: Drop extended property on old composite PK (if it exists)
IF EXISTS (
    SELECT 1 FROM sys.extended_properties ep
    WHERE ep.major_id = (SELECT object_id FROM sys.key_constraints WHERE name = 'PK_EmailAddress_BusinessEntityID_EmailAddressID' AND parent_object_id = OBJECT_ID('[Person].[EmailAddress]'))
    AND ep.name = N'MS_Description'
)
BEGIN
    EXEC sys.sp_dropextendedproperty
        @name       = N'MS_Description',
        @level0type = N'SCHEMA',
        @level0name = N'Person',
        @level1type = N'TABLE',
        @level1name = N'EmailAddress',
        @level2type = N'CONSTRAINT',
        @level2name = N'PK_EmailAddress_BusinessEntityID_EmailAddressID'
END
GO

-- Step 2: Drop old composite PK
IF EXISTS (SELECT 1 FROM sys.key_constraints WHERE name = 'PK_EmailAddress_BusinessEntityID_EmailAddressID' AND parent_object_id = OBJECT_ID('[Person].[EmailAddress]'))
BEGIN
    ALTER TABLE [Person].[EmailAddress] DROP CONSTRAINT [PK_EmailAddress_BusinessEntityID_EmailAddressID]
END
GO

-- Step 3: Add single-column PK on EmailAddressID
IF NOT EXISTS (SELECT 1 FROM sys.key_constraints WHERE name = 'PK_EmailAddress' AND parent_object_id = OBJECT_ID('[Person].[EmailAddress]'))
BEGIN
    ALTER TABLE [Person].[EmailAddress] ADD CONSTRAINT [PK_EmailAddress] PRIMARY KEY NONCLUSTERED ([EmailAddressID] ASC)
END
GO

-- Step 4: Add extended property on new PK
IF NOT EXISTS (
    SELECT 1 FROM sys.extended_properties ep
    WHERE ep.major_id = (SELECT object_id FROM sys.key_constraints WHERE name = 'PK_EmailAddress' AND parent_object_id = OBJECT_ID('[Person].[EmailAddress]'))
    AND ep.name = N'MS_Description'
)
BEGIN
    EXEC sys.sp_addextendedproperty
        @name       = N'MS_Description',
        @value      = N'Primary key constraint',
        @level0type = N'SCHEMA',
        @level0name = N'Person',
        @level1type = N'TABLE',
        @level1name = N'EmailAddress',
        @level2type = N'CONSTRAINT',
        @level2name = N'PK_EmailAddress'
END
GO

-- Step 5: Add UNIQUE CLUSTERED constraint on (BusinessEntityID, EmailAddressID)
IF NOT EXISTS (SELECT 1 FROM sys.key_constraints WHERE name = 'unq_email_address' AND parent_object_id = OBJECT_ID('[Person].[EmailAddress]'))
BEGIN
    ALTER TABLE [Person].[EmailAddress]
        ADD CONSTRAINT [unq_email_address] UNIQUE CLUSTERED ([BusinessEntityID], [EmailAddressID]) WITH (FILLFACTOR = 80)
END
GO

-- Step 6: Add extended property on unique constraint
IF NOT EXISTS (
    SELECT 1 FROM sys.extended_properties ep
    WHERE ep.major_id = (SELECT object_id FROM sys.key_constraints WHERE name = 'unq_email_address' AND parent_object_id = OBJECT_ID('[Person].[EmailAddress]'))
    AND ep.name = N'MS_Description'
)
BEGIN
    EXEC sys.sp_addextendedproperty
        @name       = N'MS_Description',
        @value      = N'Unique clustered constraint replacing the original composite primary key',
        @level0type = N'SCHEMA',
        @level0name = N'Person',
        @level1type = N'TABLE',
        @level1name = N'EmailAddress',
        @level2type = N'CONSTRAINT',
        @level2name = N'unq_email_address'
END
GO
