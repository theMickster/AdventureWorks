
# Enhance AdventureWorks Database Account Information 

The following outlines the process performed to enhance the Person.Password informaton into a more modern user table.

## Rename the existing Person.Password table

Decided upon a new table name that didn't include reserved keywords

```sql
/*************************************************
* Rename the current Person Password table
*************************************************/
EXECUTE sp_rename @objname = N'Person.PK_Password_BusinessEntityID', @newname = N'Person.PK_AccountInformation_BusinessEntityID'

EXECUTE sp_rename @objname = N'Person.FK_Password_Person_BusinessEntityID', @newname = N'Person.FK_AccountInformation_BusinessEntityID'

EXECUTE sp_rename @objname = N'Person.DF_Password_ModifiedDate', @newname = N'Person.DF_AccountInformation_ModifiedDate'

EXECUTE sp_rename @objname = N'Person.DF_Password_rowguid', @newname = N'Person.DF_AccountInformation_rowguid'

EXECUTE sp_rename @objname = N'Person.Password', @newname =N'AccountInformation'

```

## Alter structure of Person.AccountInformation 

The existing table lacked a username field and the existing salt and password fields are not sufficient to store modern passwords hashed with the SHA 512 hashing algorithm.

```sql

/*************************************************
* Alter the current AccountInformation table
*************************************************/
IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'Person.AccountInformation') AND name = 'PasswordSalt')
BEGIN
	ALTER TABLE Person.AccountInformation DROP COLUMN PasswordSalt;
END

IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'Person.AccountInformation') AND name = 'PasswordHash')
BEGIN
	ALTER TABLE Person.AccountInformation DROP COLUMN PasswordHash;
END

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'Person.AccountInformation') AND name = 'UserName')
BEGIN
	ALTER TABLE Person.AccountInformation 
	  ADD UserName VARCHAR(128) NOT NULL
	  CONSTRAINT DF_Person_AccountInformation_UserName DEFAULT( '')
END

```

## Set each person's new username field

```sql
UPDATE  ap
SET	ap.UserName = TRIM(LOWER(p.FirstName)) + '.' + TRIM(LOWER(p.LastName))
FROM    Person.AccountInformation ap
		INNER JOIN Person.Person p ON p.BusinessEntityID = ap.BusinessEntityID
```

## Update username field to prevent duplicates

```sql
SELECT	BusinessEntityID
INTO	#TempPeopleToUpdate
FROM	Person.AccountInformation
WHERE	UserName IN (SELECT a.UserName
					FROM Person.AccountInformation a
					GROUP BY a.UserName
					HAVING COUNT(*) > 1)

UPDATE	a
SET		a.UserName = a.UserName + '.'+ CAST(a.BusinessEntityID AS VARCHAR(10))
FROM	Person.AccountInformation a
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
ALTER TABLE Person.AccountInformation 
	ADD CONSTRAINT UQ_Person_AccountInformaton_Username UNIQUE NONCLUSTERED (UserName);

```
