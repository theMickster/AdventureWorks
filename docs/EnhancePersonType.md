# Enhance Person Type Information within AdventureWorks Database

In order to support a richer and normalized database schema for person type, the following adjustments have been made to the AdventureWorks database.

## Create new table for Person Type information

```sql
CREATE TABLE Person.PersonType
(
	PersonTypeId integer IDENTITY(1,1) NOT NULL
	,PersonTypeGuid UNIQUEIDENTIFIER NOT NULL
	,PersonTypeCode VARCHAR(10) NOT NULL
	,PersonTypeName VARCHAR(128) NOT NULL
	,PersonTypeDescription VARCHAR(500) NOT NULL
	,CreatedBy INTEGER NOT NULL
	,CreatedOn DATETIME NOT NULL CONSTRAINT df_person_type_created_on DEFAULT SYSUTCDATETIME()
	,ModifiedBy INTEGER NOT NULL
	,ModifiedOn DATETIME NOT NULL CONSTRAINT df_person_type_modified_on DEFAULT SYSUTCDATETIME()
	,CONSTRAINT pk_person_type PRIMARY KEY CLUSTERED (PersonTypeId)
	,CONSTRAINT unq_person_type_person_type_guid UNIQUE NONCLUSTERED (PersonTypeGuid)
)
```

## Load Person Type Data

```sql
SET IDENTITY_INSERT Person.PersonType ON; 

INSERT INTO Person.PersonType (PersonTypeId, PersonTypeGuid, PersonTypeCode, PersonTypeName, PersonTypeDescription, CreatedBy, CreatedOn, ModifiedBy, ModifiedOn)
SELECT 1, 'E86DDC95-5C77-4BF4-B1AC-95F1317DA877', 'SC', 'Store Contact', 'A Store Contact for AdventureWorks Cycling',1000001,SYSUTCDATETIME(),1000001,SYSUTCDATETIME()
WHERE NOT EXISTS (SELECT 1 FROM Person.PersonType WHERE PersonTypeId = 1)

INSERT INTO Person.PersonType (PersonTypeId, PersonTypeGuid, PersonTypeCode, PersonTypeName, PersonTypeDescription, CreatedBy, CreatedOn, ModifiedBy, ModifiedOn)
SELECT 2, '0B61708C-C1EA-44B9-9BAE-12A22F0D84C9', 'IN', 'Individual Customer', 'An Individual, retail customer of AdventureWorks Cycling',1000001,SYSUTCDATETIME(),1000001,SYSUTCDATETIME()
WHERE NOT EXISTS (SELECT 1 FROM Person.PersonType WHERE PersonTypeId = 2)

INSERT INTO Person.PersonType (PersonTypeId, PersonTypeGuid, PersonTypeCode, PersonTypeName, PersonTypeDescription, CreatedBy, CreatedOn, ModifiedBy, ModifiedOn)
SELECT 3, 'BB5F86D0-140A-4154-B078-07883B376DCC', 'SP', 'Sales Person', 'A sales person who works on behalf of AdventureWorks Cycling',1000001,SYSUTCDATETIME(),1000001,SYSUTCDATETIME()
WHERE NOT EXISTS (SELECT 1 FROM Person.PersonType WHERE PersonTypeId = 3)

INSERT INTO Person.PersonType (PersonTypeId, PersonTypeGuid, PersonTypeCode, PersonTypeName, PersonTypeDescription, CreatedBy, CreatedOn, ModifiedBy, ModifiedOn)
SELECT 4, 'C3FBB79A-2AC3-4AA8-9E6D-32CE2DD0EC78', 'EM', 'Employee (Non-Sales Person)', 'An employee of AdventureWorks Cycling who is not a sales person',1000001,SYSUTCDATETIME(),1000001,SYSUTCDATETIME()
WHERE NOT EXISTS (SELECT 1 FROM Person.PersonType WHERE PersonTypeId = 4)

INSERT INTO Person.PersonType (PersonTypeId, PersonTypeGuid, PersonTypeCode, PersonTypeName, PersonTypeDescription, CreatedBy, CreatedOn, ModifiedBy, ModifiedOn)
SELECT 5, '6E9899A1-797B-4914-BC5E-B5F2790D5FC7', 'VC', 'Vendor Contact', 'A vendor contact who works with AdventureWorks Cycling',1000001,SYSUTCDATETIME(),1000001,SYSUTCDATETIME()
WHERE NOT EXISTS (SELECT 1 FROM Person.PersonType WHERE PersonTypeId = 5)

INSERT INTO Person.PersonType (PersonTypeId, PersonTypeGuid, PersonTypeCode, PersonTypeName, PersonTypeDescription, CreatedBy, CreatedOn, ModifiedBy, ModifiedOn)
SELECT 6, 'CBE70EF2-3F7A-49F8-BC88-6FE97B109C66', 'GC', 'General Contact', 'A general contact for AdventureWorks Cycling',1000001,SYSUTCDATETIME(),1000001,SYSUTCDATETIME()
WHERE NOT EXISTS (SELECT 1 FROM Person.PersonType WHERE PersonTypeId = 6)


SET IDENTITY_INSERT Person.PersonType OFF;
```

## Alter Person table creating reference to new PersonType table and information

```sql
ALTER TABLE Person.Person ADD PersonTypeId INTEGER NULL; 

GO

UPDATE	p
SET     P.PersonTypeId = pt.PersonTypeId
FROM    Person.Person p
		    INNER JOIN Person.PersonType pt ON p.PersonType = pt.PersonTypeCode
GO

ALTER TABLE Person.Person 
	WITH CHECK ADD CONSTRAINT FK_Person_PersonTypeId FOREIGN KEY (PersonTypeId)
	REFERENCES Person.PersonType (PersonTypeId)

GO

ALTER TABLE Person.Person ALTER COLUMN PersonTypeId INTEGER NOT NULL; 

GO

ALTER TABLE Person.Person DROP CONSTRAINT CK_Person_PersonType

GO

ALTER TABLE Person.Person DROP COLUMN PersonType;

```




