-- Normalizes the legacy PersonType nchar(2) column on Person.Person into a
-- proper lookup table, enabling FK relationships and richer type metadata.
-- See: database/docs/EnhancePersonType.md
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'PersonType' AND schema_id = SCHEMA_ID('Person'))
BEGIN
    CREATE TABLE [Person].[PersonType]
    (
        [PersonTypeId]          INTEGER          IDENTITY(1, 1) NOT NULL,
        [PersonTypeGuid]        UNIQUEIDENTIFIER               NOT NULL,
        [PersonTypeCode]        VARCHAR(10)                    NOT NULL,
        [PersonTypeName]        VARCHAR(128)                   NOT NULL,
        [PersonTypeDescription] VARCHAR(500)                   NOT NULL,
        [CreatedBy]             INTEGER                        NOT NULL,
        [CreatedOn]             DATETIME                       NOT NULL CONSTRAINT [df_person_type_created_on]  DEFAULT SYSUTCDATETIME(),
        [ModifiedBy]            INTEGER                        NOT NULL,
        [ModifiedOn]            DATETIME                       NOT NULL CONSTRAINT [df_person_type_modified_on] DEFAULT SYSUTCDATETIME(),
        CONSTRAINT [pk_person_type]              PRIMARY KEY CLUSTERED     ([PersonTypeId]),
        CONSTRAINT [unq_person_type_person_type_guid] UNIQUE NONCLUSTERED ([PersonTypeGuid])
    )
END
GO
