
# Enhance AdventureWorks Database Account Information 

The following outlines the process performed to enhance the Person.Password informaton into a more modern user table.

## Create new schema for security tables

```sql
IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'Shield')
BEGIN
	EXECUTE sp_executesql N'CREATE SCHEMA Shield'
END 
```


## Rename the existing Person.Password table

Decided upon a new table name that didn't include reserved keywords

```sql
/*************************************************
* Rename the current Person Password table
*************************************************/
EXECUTE sp_rename @objname = N'Person.PK_Password_BusinessEntityID', @newname = N'PK_UserAccount_BusinessEntityID'

EXECUTE sp_rename @objname = N'Person.FK_Password_Person_BusinessEntityID', @newname = N'FK_UserAccount_BusinessEntityID'

EXECUTE sp_rename @objname = N'Person.DF_Password_ModifiedDate', @newname = N'DF_UserAccount_ModifiedDate'

EXECUTE sp_rename @objname = N'Person.DF_Password_rowguid', @newname = N'DF_UserAccount_rowguid'

EXECUTE sp_rename @objname = N'Person.Password', @newname =N'UserAccount'

/*************************************************
* Transfer to the new schema
*************************************************/
ALTER SCHEMA Shield TRANSFER Person.UserAccount

```

## Alter structure of Person.AccountInformation 

The existing table lacked a username field and the existing salt and password fields are not sufficient to store modern passwords.

```sql

/*************************************************
* Alter the current UserAccount table
*************************************************/
IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'Shield.UserAccount') AND name = 'PasswordSalt')
BEGIN
	ALTER TABLE Shield.UserAccount DROP COLUMN PasswordSalt;
END
GO
IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'Shield.UserAccount') AND name = 'PasswordHash')
BEGIN
	ALTER TABLE Shield.UserAccount DROP COLUMN PasswordHash;
END
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'Shield.UserAccount') AND name = 'UserName')
BEGIN
	ALTER TABLE Shield.UserAccount 
	  ADD UserName VARCHAR(128) NOT NULL
	  CONSTRAINT DF_Shield_UserAccount_UserName DEFAULT('')
END
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'Shield.PasswordHash') AND name = 'PasswordHash')
BEGIN
	ALTER TABLE Shield.UserAccount 
	  ADD PasswordHash NVARCHAR(MAX) NOT NULL
	  CONSTRAINT DF_Shield_UserAccount_PasswordHash DEFAULT('')
END
GO

```

## Set each person's new username field

```sql
UPDATE  u
SET	u.UserName = TRIM(LOWER(p.FirstName)) + '.' + TRIM(LOWER(p.LastName))
FROM    Shield.UserAccount u
	INNER JOIN Person.Person p ON p.BusinessEntityID = u.BusinessEntityID

```

## Update username field to prevent duplicates

```sql
SELECT	BusinessEntityID
INTO	#TempPeopleToUpdate
FROM	Shield.UserAccount
WHERE	UserName IN (	SELECT a.UserName
			FROM Shield.UserAccount a
			GROUP BY a.UserName
			HAVING COUNT(*) > 1)

UPDATE	a
SET	a.UserName = a.UserName + '.'+ CAST(a.BusinessEntityID AS VARCHAR(10))
FROM	Shield.UserAccount a
		INNER JOIN #TempPeopleToUpdate temp ON a.BusinessEntityID = temp.BusinessEntityID


```

### View results after update 

```sql
SELECT a.UserName
FROM Shield.UserAccount a
GROUP BY a.UserName
HAVING COUNT(*) > 1
```

### Add new unique constraint to username field

```sql
ALTER TABLE Shield.UserAccount 
		ADD CONSTRAINT UQ_Shield_UserAccount_Username UNIQUE NONCLUSTERED (UserName);

```
