
# Enhance AdventureWorks Database Account Information 

The following outlines the process performed to enhance the Person.Password informaton into a more modern user table.

## Rename the existing Person.Password table

Decided upon a new table name that didn't include reserved keywords

```sql
/*************************************************
* Rename the current Person Password table
*************************************************/
EXECUTE sp_rename @objname = N'Person.PK_Password_BusinessEntityID', @newname = N'Person.PK_UserAccount_BusinessEntityID'

EXECUTE sp_rename @objname = N'Person.FK_Password_Person_BusinessEntityID', @newname = N'Person.FK_UserAccount_BusinessEntityID'

EXECUTE sp_rename @objname = N'Person.DF_Password_ModifiedDate', @newname = N'Person.DF_UserAccount_ModifiedDate'

EXECUTE sp_rename @objname = N'Person.DF_Password_rowguid', @newname = N'Person.DF_UserAccount_rowguid'

EXECUTE sp_rename @objname = N'Person.Password', @newname =N'UserAccount'


```

## Alter structure of Person.AccountInformation 

The existing table lacked a username field and the existing salt and password fields are not sufficient to store modern passwords.

```sql

/*************************************************
* Alter the current UserAccount table
*************************************************/
IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'Person.UserAccount') AND name = 'PasswordSalt')
BEGIN
	ALTER TABLE Person.UserAccount DROP COLUMN PasswordSalt;
END
GO
IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'Person.UserAccount') AND name = 'PasswordHash')
BEGIN
	ALTER TABLE Person.UserAccount DROP COLUMN PasswordHash;
END
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'Person.UserAccount') AND name = 'UserName')
BEGIN
	ALTER TABLE Person.UserAccount 
	  ADD UserName VARCHAR(128) NOT NULL
	  CONSTRAINT DF_Person_UserAccount_UserName DEFAULT('')
END
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'Person.PasswordHash') AND name = 'PasswordHash')
BEGIN
	ALTER TABLE Person.UserAccount 
	  ADD PasswordHash NVARCHAR(MAX) NOT NULL
	  CONSTRAINT DF_Person_UserAccount_PasswordHash DEFAULT('')
END
GO

```

## Set each person's new username field

```sql
UPDATE  u
SET	u.UserName = TRIM(LOWER(p.FirstName)) + '.' + TRIM(LOWER(p.LastName))
FROM    Person.UserAccount u
	INNER JOIN Person.Person p ON p.BusinessEntityID = u.BusinessEntityID

```

## Update username field to prevent duplicates

```sql
SELECT	BusinessEntityID
INTO	#TempPeopleToUpdate
FROM	Person.UserAccount
WHERE	UserName IN (SELECT a.UserName
					FROM Person.UserAccount a
					GROUP BY a.UserName
					HAVING COUNT(*) > 1)

UPDATE	a
SET	a.UserName = a.UserName + '.'+ CAST(a.BusinessEntityID AS VARCHAR(10))
FROM	Person.UserAccount a
		INNER JOIN #TempPeopleToUpdate temp ON a.BusinessEntityID = temp.BusinessEntityID


```

### View results after update 

```sql
SELECT a.UserName
FROM Person.AccountInformation a
GROUP BY a.UserName
HAVING COUNT(*) > 1
```

### Add new unique constraint to username field

```sql
ALTER TABLE Person.UserAccount 
		ADD CONSTRAINT UQ_Person_UserAccount_Username UNIQUE NONCLUSTERED (UserName);

```
