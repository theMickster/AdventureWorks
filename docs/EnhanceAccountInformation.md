
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

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'Person.AccountInformation') AND name = 'SaltGuid')
BEGIN
	ALTER TABLE Person.AccountInformation 
	  ADD SaltGuid UNIQUEIDENTIFIER NOT NULL
	  CONSTRAINT DF_Person_AccountInformation_SaltGuid DEFAULT( '00000000-0000-0000-0000-000000000000')
END

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'Person.AccountInformation') AND name = 'AccountPasswordHash')
BEGIN
	ALTER TABLE Person.AccountInformation 
	  ADD AccountPasswordHash VARBINARY(256) NOT NULL
	  CONSTRAINT DF_Person_AccountInformation_AccountPasswordHash DEFAULT( CAST(0x0 AS VARBINARY) )
END

```

## Set each person's new username field

```sql
UPDATE  ap
SET	ap.UserName = TRIM(LOWER(p.FirstName)) + '.' + TRIM(LOWER(p.LastName))
FROM    Person.AccountInformation ap
		INNER JOIN Person.Person p ON p.BusinessEntityID = ap.BusinessEntityID
```

## Iterate through each record crafting a unique password salt guid

:clipboard: This process will take around 2-4 minutes to complete

```sql
DECLARE @TempId INTEGER = 1
	,@BusinessEntityID INTEGER = 0
	,@NewGuid UNIQUEIDENTIFIER 

DECLARE @tempPeople TABLE (TempId INTEGER IDENTITY(1,1) NOT NULL PRIMARY KEY, BusinessEntityID INTEGER UNIQUE);

INSERT INTO @tempPeople
SELECT	DISTINCT BusinessEntityID
FROM	person.AccountInformation
ORDER BY BusinessEntityID ASC

SELECT @BusinessEntityID = t.BusinessEntityID
FROM @tempPeople t
WHERE t.TempId = @TempId

WHILE @@ROWCOUNT <> 0
BEGIN 
	SELECT @NewGuid = NEWID();

	UPDATE Person.AccountInformation
	SET SaltGuid = @NewGuid
	WHERE BusinessEntityID = @BusinessEntityID

	SET @TempId += 1;

	SELECT @BusinessEntityID = t.BusinessEntityID
	FROM @tempPeople t
	WHERE t.TempId = @TempId	
END
```

In a separate query window, you may track progress using the following:
```sql
SELECT COUNT(*) FROM Person.AccountInformation WITH(NOLOCK) WHERE saltguid = '00000000-0000-0000-0000-000000000000'
```

## View new table after updates

```sql 
SELECT * FROM Person.AccountInformation 
```

## Generate fake passwords for each account

As an example, you could set each person's account to "HelloWorld!" + their BusinessEntityID. Please do not do this in a production application :smiley:.  

### Use the following SELECT statement to view the results.

```sql
SELECT	TRIM(CAST(ap.SaltGuid AS VARCHAR(40))) + 'HelloWorld!' + TRIM(CAST(ap.BusinessEntityID AS VARCHAR(40)))
	,(SELECT HASHBYTES('SHA2_512',TRIM(CAST(ap.SaltGuid AS VARCHAR(40))) + 'HelloWorld!' + TRIM(CAST(ap.BusinessEntityID AS VARCHAR(40)))))
FROM	Person.AccountInformation ap
```

### Use the following UPDATE statement to update the password for each account

```sql 
UPDATE	ap
SET	AccountPasswordHash = (SELECT HASHBYTES('SHA2_512',TRIM(CAST(ap.SaltGuid AS VARCHAR(40))) + 'HelloWorld!' + TRIM(CAST(ap.BusinessEntityID AS VARCHAR(40)))))
FROM	Person.AccountInformation ap
```

### View new table after updates

```sql 
SELECT  pp.BusinessEntityID
        ,pp.FirstName
        ,pp.LastName
		,pai.UserName
		,pai.SaltGuid
		,pai.AccountPasswordHash
FROM    Person.Person AS pp
        INNER JOIN Person.AccountInformation AS pai ON pp.BusinessEntityID = pai.BusinessEntityID
ORDER BY pp.BusinessEntityID ASC

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
