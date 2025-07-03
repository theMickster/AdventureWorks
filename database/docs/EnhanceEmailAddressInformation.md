# Enhance Email Address Information within AdventureWorks Database 

The following outlines the process performed to enhance the email address information within the AdventureWorks database. 

## Drop Existing constraints on Person.EmailAddress

Personally, I think these primary key and unique constraints are incorrect. In my humble opinion, primary keys should not be composite keys.

```sql
EXEC sys.sp_dropextendedproperty
   @name = N'MS_Description'
   ,@level0type = N'SCHEMA'
   ,@level0name = N'Person'
   ,@level1type = N'TABLE'
   ,@level1name = N'EmailAddress'
   ,@level2type = N'CONSTRAINT'
   ,@level2name = N'PK_EmailAddress_BusinessEntityID_EmailAddressID'
GO

ALTER TABLE Person.EmailAddress DROP CONSTRAINT PK_EmailAddress_BusinessEntityID_EmailAddressID
GO

ALTER TABLE Person.EmailAddress ADD CONSTRAINT PK_EmailAddress PRIMARY KEY NONCLUSTERED ( EmailAddressID ASC );
GO
```

## Create new constraints on Person.EmailAddress

```sql

EXEC sys.sp_addextendedproperty
   @name = N'MS_Description'
   ,@value = N'Primary key constraint'
   ,@level0type = N'SCHEMA'
   ,@level0name = N'Person'
   ,@level1type = N'TABLE'
   ,@level1name = N'EmailAddress'
   ,@level2type = N'CONSTRAINT'
   ,@level2name = N'PK_EmailAddress'
GO

ALTER TABLE Person.EmailAddress ADD CONSTRAINT unq_email_address UNIQUE CLUSTERED (BusinessEntityID, EmailAddressID) WITH (FILLFACTOR = 80);

EXEC sys.sp_addextendedproperty
   @name = N'MS_Description'
   ,@value = N'Primary key constraint'
   ,@level0type = N'SCHEMA'
   ,@level0name = N'Person'
   ,@level1type = N'TABLE'
   ,@level1name = N'EmailAddress'
   ,@level2type = N'CONSTRAINT'
   ,@level2name = N'unq_email_address'
GO
```

## Enhance UserAccount to include primary email address 

We will leverage this during authorization & user account workflows

```sql
ALTER TABLE Shield.UserAccount ADD PrimaryEmailAddressId INTEGER NULL; 
GO
ALTER TABLE Shield.UserAccount
WITH CHECK ADD  CONSTRAINT FK_UserAccount_EmailAddress FOREIGN KEY(PrimaryEmailAddressId)
REFERENCES person.EmailAddress (EmailAddressID);
GO
UPDATE	sa
SET	sa.PrimaryEmailAddressId = ea.EmailAddressID
FROM	Shield.UserAccount sa
		INNER JOIN Person.Person p ON sa.BusinessEntityID = p.BusinessEntityID
		INNER JOIN person.EmailAddress ea ON p.BusinessEntityID = ea.BusinessEntityID
GO
ALTER TABLE Shield.UserAccount ALTER COLUMN PrimaryEmailAddressId INTEGER NOT NULL; 
```
