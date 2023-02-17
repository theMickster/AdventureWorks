# Create Custom Authorization within AdventureWorks Database 

The following outlines the process performed to create custom authorization within the AdventureWorks Database. The goal is to create a flexible, reliable, and easy to maintain authorization structure. 

Security roles will be levergaed to create authorization policies and guards within the applications. Security roles will be assocaited to Security Groups in the cross-reference table SecurityGroupSecurityRole.

Security groups will be leveraged to group user accounts so that we simply applying authorization rules en mass instead of to individual user accounts. 

Security functions are very specialized abilities (yet to be defined within AdventureWorks) that are associated to a limited group of users. Typically these are very special permissions that have a narrow use case. For example, the ability to hire and release employees would be a good exmaple of a security function only associated to Human Resources Managers that may entail several steps or procedures to complete. 

## Create Authorization Tables

```sql
CREATE TABLE Shield.SecurityRole
(
	RoleId INTEGER IDENTITY(1,1) NOT NULL
	,RoleGuid UNIQUEIDENTIFIER NOT NULL
	,RoleName VARCHAR(128) NOT NULL
	,RoleDescription VARCHAR(500) NOT NULL
	,CreatedBy INTEGER NOT NULL
	,CreatedOn DATETIME NOT NULL CONSTRAINT df_security_role_created_on DEFAULT SYSUTCDATETIME()
	,ModifiedBy INTEGER NOT NULL
	,ModifiedOn DATETIME NOT NULL CONSTRAINT df_security_role_modified_on DEFAULT SYSUTCDATETIME()
	,CONSTRAINT pk_security_role PRIMARY KEY CLUSTERED (RoleId)
	,CONSTRAINT unq_security_role_role_guid UNIQUE NONCLUSTERED (RoleGuid)	
)

CREATE TABLE Shield.SecurityGroup
(
	GroupId INTEGER IDENTITY(1,1) NOT NULL
	,GroupGuid UNIQUEIDENTIFIER NOT NULL
	,GroupName VARCHAR(128) NOT NULL
	,GroupDescription VARCHAR(500) NOT NULL
	,CreatedBy INTEGER NOT NULL
	,CreatedOn DATETIME NOT NULL CONSTRAINT df_security_group_created_on DEFAULT SYSUTCDATETIME()
	,ModifiedBy INTEGER NOT NULL
	,ModifiedOn DATETIME NOT NULL CONSTRAINT df_security_group_modified_on DEFAULT SYSUTCDATETIME()
	,CONSTRAINT pk_security_group PRIMARY KEY CLUSTERED (GroupId)
	,CONSTRAINT unq_security_group_group_guid UNIQUE NONCLUSTERED (GroupGuid)	
)

CREATE TABLE Shield.SecurityFunction
(
	FunctionId INTEGER IDENTITY(1,1) NOT NULL
	,FunctionGuid UNIQUEIDENTIFIER NOT NULL
	,FunctionName VARCHAR(128) NOT NULL
	,FunctionDescription VARCHAR(500) NOT NULL
	,CreatedBy INTEGER NOT NULL
	,CreatedOn DATETIME NOT NULL CONSTRAINT df_security_function_created_on DEFAULT SYSUTCDATETIME()
	,ModifiedBy INTEGER NOT NULL
	,ModifiedOn DATETIME NOT NULL CONSTRAINT df_security_function_modified_on DEFAULT SYSUTCDATETIME()
	,CONSTRAINT pk_security_function PRIMARY KEY CLUSTERED (FunctionId)
	,CONSTRAINT unq_security_function_function_guid UNIQUE NONCLUSTERED (FunctionGuid)	
)

CREATE TABLE Shield.SecurityGroupUserAccount
(
	SecurityGroupUserAccountId INTEGER IDENTITY(1,1) NOT NULL
	,SecurityGroupUserAccountGuid  UNIQUEIDENTIFIER NOT NULL
	,GroupId INTEGER NOT NULL
	,BusinessEntityId INTEGER NOT NULL
	,CreatedBy INTEGER NOT NULL
	,CreatedOn DATETIME NOT NULL CONSTRAINT df_security_group_user_account_created_on DEFAULT SYSUTCDATETIME()
	,ModifiedBy INTEGER NOT NULL
	,ModifiedOn DATETIME NOT NULL CONSTRAINT df_security_group_user_account_modified_on DEFAULT SYSUTCDATETIME()
	,CONSTRAINT pk_security_group_user_account PRIMARY KEY NONCLUSTERED (SecurityGroupUserAccountId)
	,CONSTRAINT unq_security_group_user_account_guid UNIQUE NONCLUSTERED (SecurityGroupUserAccountGuid)	
	,CONSTRAINT unq_security_group_user_account UNIQUE CLUSTERED (GroupId, BusinessEntityId) WITH (FILLFACTOR = 80)
)

CREATE TABLE Shield.SecurityGroupSecurityRole
(
	SecurityGroupSecurityRoleId INTEGER IDENTITY(1,1) NOT NULL
	,SecurityGroupSecurityRoleGuid  UNIQUEIDENTIFIER NOT NULL
	,GroupId INTEGER NOT NULL
	,RoleId INTEGER NOT NULL
	,CreatedBy INTEGER NOT NULL
	,CreatedOn DATETIME NOT NULL CONSTRAINT df_security_group_security_role_created_on DEFAULT SYSUTCDATETIME()
	,ModifiedBy INTEGER NOT NULL
	,ModifiedOn DATETIME NOT NULL CONSTRAINT df_security_group_security_role_modified_on DEFAULT SYSUTCDATETIME()
	,CONSTRAINT pk_security_group_security_role PRIMARY KEY NONCLUSTERED (SecurityGroupSecurityRoleId)
	,CONSTRAINT unq_security_group_security_role_guid UNIQUE NONCLUSTERED (SecurityGroupSecurityRoleGuid)	
	,CONSTRAINT unq_security_group_security_role UNIQUE CLUSTERED (GroupId, RoleId) WITH (FILLFACTOR = 80)
)

CREATE TABLE Shield.SecurityGroupSecurityFunction
(
	SecurityGroupSecurityFunctionId INTEGER IDENTITY(1,1) NOT NULL
	,SecurityGroupSecurityFunctionGuid  UNIQUEIDENTIFIER NOT NULL
	,GroupId INTEGER NOT NULL
	,FunctionId INTEGER NOT NULL
	,CreatedBy INTEGER NOT NULL
	,CreatedOn DATETIME NOT NULL CONSTRAINT df_security_group_security_function_created_on DEFAULT SYSUTCDATETIME()
	,ModifiedBy INTEGER NOT NULL
	,ModifiedOn DATETIME NOT NULL CONSTRAINT df_security_group_security_function_modified_on DEFAULT SYSUTCDATETIME()
	,CONSTRAINT pk_security_group_security_function PRIMARY KEY NONCLUSTERED (SecurityGroupSecurityFunctionId)
	,CONSTRAINT unq_security_group_security_function_guid UNIQUE NONCLUSTERED (SecurityGroupSecurityFunctionGuid)	
	,CONSTRAINT unq_security_group_security_function UNIQUE CLUSTERED (GroupId, FunctionId) WITH (FILLFACTOR = 80)
)


```

## Create new Business Entities, Persons, UserAccounts, and Email Addresses

The following DML is intended to be idempotent to ensure any accidental re-runs do not break existing functionality

### Person.BusinessEntity

```sql
SET IDENTITY_INSERT person.BusinessEntity ON;

INSERT INTO person.BusinessEntity (BusinessEntityID, rowguid, ModifiedDate)
SELECT 1000001, 'd88908ff-3775-4024-859e-d5ae0e85ae83', SYSUTCDATETIME()
WHERE NOT EXISTS (SELECT 1 FROM person.BusinessEntity WHERE BusinessEntityID = 1000001) 

INSERT INTO person.BusinessEntity (BusinessEntityID, rowguid, ModifiedDate)
SELECT 1000002, '37646de0-9e5a-4b49-86c2-dfabb071107e', SYSUTCDATETIME()
WHERE NOT EXISTS (SELECT 1 FROM person.BusinessEntity WHERE BusinessEntityID = 1000002) 
	
INSERT INTO person.BusinessEntity (BusinessEntityID, rowguid, ModifiedDate)
SELECT 1000003, 'd55857a8-2f60-43ca-b629-4ab923a0feea', SYSUTCDATETIME()
WHERE NOT EXISTS (SELECT 1 FROM person.BusinessEntity WHERE BusinessEntityID = 1000003) 

INSERT INTO person.BusinessEntity (BusinessEntityID, rowguid, ModifiedDate)	
SELECT 1000004, '1b610b56-0a9f-48c1-bd7f-67f573efa3e3', SYSUTCDATETIME()
WHERE NOT EXISTS (SELECT 1 FROM person.BusinessEntity WHERE BusinessEntityID = 1000004) 

INSERT INTO person.BusinessEntity (BusinessEntityID, rowguid, ModifiedDate)	
SELECT 1000005, '615d5c1d-ff5a-4f50-b259-c6c77f8cc950', SYSUTCDATETIME()
WHERE NOT EXISTS (SELECT 1 FROM person.BusinessEntity WHERE BusinessEntityID = 1000005) 

INSERT INTO person.BusinessEntity (BusinessEntityID, rowguid, ModifiedDate)	
SELECT 1000006, 'fc134202-a5a0-410e-a057-b0f9d7f8608b', SYSUTCDATETIME()
WHERE NOT EXISTS (SELECT 1 FROM person.BusinessEntity WHERE BusinessEntityID = 1000006) 
	
INSERT INTO person.BusinessEntity (BusinessEntityID, rowguid, ModifiedDate)
SELECT 1000007, '12e60cc2-b2c3-4c7f-a902-fb30406df054', SYSUTCDATETIME()
WHERE NOT EXISTS (SELECT 1 FROM person.BusinessEntity WHERE BusinessEntityID = 1000007) 

INSERT INTO person.BusinessEntity (BusinessEntityID, rowguid, ModifiedDate)
SELECT 1000008, '66580b1d-1d0c-4145-9798-7c76fe141dce', SYSUTCDATETIME()
WHERE NOT EXISTS (SELECT 1 FROM person.BusinessEntity WHERE BusinessEntityID = 1000008) 

INSERT INTO person.BusinessEntity (BusinessEntityID, rowguid, ModifiedDate)
SELECT 1000009, '47dc4448-df2c-4ff2-a6d1-6ad0ccaa40aa', SYSUTCDATETIME()
WHERE NOT EXISTS (SELECT 1 FROM person.BusinessEntity WHERE BusinessEntityID = 1000009) 

INSERT INTO person.BusinessEntity (BusinessEntityID, rowguid, ModifiedDate)
SELECT 1000010, '56bc4cdb-42d0-4e5d-aa8c-f91760e6a87d', SYSUTCDATETIME()
WHERE NOT EXISTS (SELECT 1 FROM person.BusinessEntity WHERE BusinessEntityID = 1000010) 

INSERT INTO person.BusinessEntity (BusinessEntityID, rowguid, ModifiedDate)
SELECT 1000011, '2dd19c46-c8f2-469c-9cdc-ed5651cfe3c8', SYSUTCDATETIME()
WHERE NOT EXISTS (SELECT 1 FROM person.BusinessEntity WHERE BusinessEntityID = 1000011) 

INSERT INTO person.BusinessEntity (BusinessEntityID, rowguid, ModifiedDate)
SELECT 1000012, '8d363d5b-b450-40b3-82f9-d68bacb0c706', SYSUTCDATETIME()
WHERE NOT EXISTS (SELECT 1 FROM person.BusinessEntity WHERE BusinessEntityID = 1000012) 

INSERT INTO person.BusinessEntity (BusinessEntityID, rowguid, ModifiedDate)
SELECT 1000013, '37502158-c533-409c-bd6b-1de1b621375c', SYSUTCDATETIME()
WHERE NOT EXISTS (SELECT 1 FROM person.BusinessEntity WHERE BusinessEntityID = 1000013) 

INSERT INTO person.BusinessEntity (BusinessEntityID, rowguid, ModifiedDate)
SELECT 1000014, 'b3c3bd8d-138a-48d1-a093-8cf8ed0471cd', SYSUTCDATETIME()
WHERE NOT EXISTS (SELECT 1 FROM person.BusinessEntity WHERE BusinessEntityID = 1000014) 

INSERT INTO person.BusinessEntity (BusinessEntityID, rowguid, ModifiedDate)
SELECT 1000015, '70b929cd-9544-4437-b9d3-dcb04a95b9b8', SYSUTCDATETIME()
WHERE NOT EXISTS (SELECT 1 FROM person.BusinessEntity WHERE BusinessEntityID = 1000015) 

INSERT INTO person.BusinessEntity (BusinessEntityID, rowguid, ModifiedDate)
SELECT 1000016, '9a327eec-e6b7-41a4-8aaf-b29773a37628', SYSUTCDATETIME()
WHERE NOT EXISTS (SELECT 1 FROM person.BusinessEntity WHERE BusinessEntityID = 1000016) 

INSERT INTO person.BusinessEntity (BusinessEntityID, rowguid, ModifiedDate)
SELECT 1000017, 'd3030720-47eb-427c-b7ab-65019993b0f9', SYSUTCDATETIME()
WHERE NOT EXISTS (SELECT 1 FROM person.BusinessEntity WHERE BusinessEntityID = 1000017) 

INSERT INTO person.BusinessEntity (BusinessEntityID, rowguid, ModifiedDate)
SELECT 1000018, 'a356ca4f-6290-48b1-ac65-35c6dec437bc', SYSUTCDATETIME()
WHERE NOT EXISTS (SELECT 1 FROM person.BusinessEntity WHERE BusinessEntityID = 1000018) 

INSERT INTO person.BusinessEntity (BusinessEntityID, rowguid, ModifiedDate)
SELECT 1000019, 'b0525436-c2aa-474c-bbee-e00382a46265', SYSUTCDATETIME()
WHERE NOT EXISTS (SELECT 1 FROM person.BusinessEntity WHERE BusinessEntityID = 1000019)

INSERT INTO person.BusinessEntity (BusinessEntityID, rowguid, ModifiedDate)
SELECT 1000020, 'f3eba696-edbe-4011-ad67-7a6f8c6ecb36', SYSUTCDATETIME()
WHERE NOT EXISTS (SELECT 1 FROM person.BusinessEntity WHERE BusinessEntityID = 1000020)

INSERT INTO person.BusinessEntity (BusinessEntityID, rowguid, ModifiedDate)
SELECT 1000021, 'eee40c1f-70c7-4068-b825-1c3592ff414b', SYSUTCDATETIME()
WHERE NOT EXISTS (SELECT 1 FROM person.BusinessEntity WHERE BusinessEntityID = 1000021)

INSERT INTO person.BusinessEntity (BusinessEntityID, rowguid, ModifiedDate)
SELECT 1000022, '2a4ed27a-69af-4e46-9ed7-1f6bd9952ee8', SYSUTCDATETIME()
WHERE NOT EXISTS (SELECT 1 FROM person.BusinessEntity WHERE BusinessEntityID = 1000022) 

INSERT INTO person.BusinessEntity (BusinessEntityID, rowguid, ModifiedDate)
SELECT 1000023, '2ea0e33f-cac5-4d30-871b-bbba09c9140c', SYSUTCDATETIME()
WHERE NOT EXISTS (SELECT 1 FROM person.BusinessEntity WHERE BusinessEntityID = 1000023)

INSERT INTO person.BusinessEntity (BusinessEntityID, rowguid, ModifiedDate)
SELECT 1000024, 'b073fcb6-ed11-43fa-860d-d2e40dd4f523', SYSUTCDATETIME()
WHERE NOT EXISTS (SELECT 1 FROM person.BusinessEntity WHERE BusinessEntityID = 1000024) 

INSERT INTO person.BusinessEntity (BusinessEntityID, rowguid, ModifiedDate)
SELECT 1000025, 'fd84ae74-d26a-4733-a280-54744d4c8fa3', SYSUTCDATETIME()
WHERE NOT EXISTS (SELECT 1 FROM person.BusinessEntity WHERE BusinessEntityID = 1000025)

INSERT INTO person.BusinessEntity (BusinessEntityID, rowguid, ModifiedDate)
SELECT 1000026, '36d3ac6d-f144-4fad-b70d-90a580db71e0', SYSUTCDATETIME()
WHERE NOT EXISTS (SELECT 1 FROM person.BusinessEntity WHERE BusinessEntityID = 1000026)

INSERT INTO person.BusinessEntity (BusinessEntityID, rowguid, ModifiedDate)
SELECT 1000027, 'f629c536-564b-4bbd-82e3-826646ab3aa2', SYSUTCDATETIME()
WHERE NOT EXISTS (SELECT 1 FROM person.BusinessEntity WHERE BusinessEntityID = 1000027)

INSERT INTO person.BusinessEntity (BusinessEntityID, rowguid, ModifiedDate)
SELECT 1000028, 'f22c8351-e6e1-429b-805d-d4f8d47eead2', SYSUTCDATETIME()
WHERE NOT EXISTS (SELECT 1 FROM person.BusinessEntity WHERE BusinessEntityID = 1000028) 

SET IDENTITY_INSERT person.BusinessEntity OFF;

```

### Person.Person

```sql
INSERT INTO person.Person (BusinessEntityID,PersonType,Title,FirstName,LastName,EmailPromotion,ModifiedDate)
SELECT 1000001,'EM','Mr.','Your Name Here','Your Name Here',0,SYSUTCDATETIME()
WHERE NOT EXISTS (SELECT 1 FROM person.Person p WHERE P.BusinessEntityID = 1000001)

INSERT INTO person.Person (BusinessEntityID,PersonType,Title,FirstName,LastName,EmailPromotion,ModifiedDate)
SELECT 1000002,'EM','Mr.','Jim','Halpert',0,SYSUTCDATETIME()
WHERE NOT EXISTS (SELECT 1 FROM person.Person p WHERE P.BusinessEntityID = 1000002)

INSERT INTO person.Person (BusinessEntityID,PersonType,Title,FirstName,LastName,EmailPromotion,ModifiedDate)
SELECT 1000003,'EM','Mr.','Michael','Scott',0,SYSUTCDATETIME()
WHERE NOT EXISTS (SELECT 1 FROM person.Person p WHERE P.BusinessEntityID = 1000003)

INSERT INTO person.Person (BusinessEntityID,PersonType,Title,FirstName,LastName,EmailPromotion,ModifiedDate)
SELECT 1000004,'EM','Mrs.','Pam','Beesly (Halpert)',0,SYSUTCDATETIME()
WHERE NOT EXISTS (SELECT 1 FROM person.Person p WHERE P.BusinessEntityID = 1000004)

INSERT INTO person.Person (BusinessEntityID,PersonType,Title,FirstName,LastName,EmailPromotion,ModifiedDate)
SELECT 1000005,'EM','Mr.','Dwight','Schrute',0,SYSUTCDATETIME()
WHERE NOT EXISTS (SELECT 1 FROM person.Person p WHERE P.BusinessEntityID = 1000005)

INSERT INTO person.Person (BusinessEntityID,PersonType,Title,FirstName,LastName,EmailPromotion,ModifiedDate)
SELECT 1000006,'EM','Mr.','Stanley','Hudson',0,SYSUTCDATETIME()
WHERE NOT EXISTS (SELECT 1 FROM person.Person p WHERE P.BusinessEntityID = 1000006)

INSERT INTO person.Person (BusinessEntityID,PersonType,Title,FirstName,LastName,EmailPromotion,ModifiedDate)
SELECT 1000007,'EM','Mr.','Kevin','Malone',0,SYSUTCDATETIME()
WHERE NOT EXISTS (SELECT 1 FROM person.Person p WHERE P.BusinessEntityID = 1000007)

INSERT INTO person.Person (BusinessEntityID,PersonType,Title,FirstName,LastName,EmailPromotion,ModifiedDate)
SELECT 1000008,'EM','Mr.','Ryan','Howard',0,SYSUTCDATETIME()
WHERE NOT EXISTS (SELECT 1 FROM person.Person p WHERE P.BusinessEntityID = 1000008)

INSERT INTO person.Person (BusinessEntityID,PersonType,Title,FirstName,LastName,EmailPromotion,ModifiedDate)
SELECT 1000009,'EM','Mrs.','Angela','Schrute (Martin)',0,SYSUTCDATETIME()
WHERE NOT EXISTS (SELECT 1 FROM person.Person p WHERE P.BusinessEntityID = 1000009)

INSERT INTO person.Person (BusinessEntityID,PersonType,Title,FirstName,LastName,EmailPromotion,ModifiedDate)
SELECT 1000010,'EM','Mr.','Oscar','Martinez',0,SYSUTCDATETIME()
WHERE NOT EXISTS (SELECT 1 FROM person.Person p WHERE P.BusinessEntityID = 1000010)

INSERT INTO person.Person (BusinessEntityID,PersonType,Title,FirstName,LastName,EmailPromotion,ModifiedDate)
SELECT 1000011,'EM','Ms.','Kelly','Kapoor',0,SYSUTCDATETIME()
WHERE NOT EXISTS (SELECT 1 FROM person.Person p WHERE P.BusinessEntityID = 1000011)

INSERT INTO person.Person (BusinessEntityID,PersonType,Title,FirstName,LastName,EmailPromotion,ModifiedDate)
SELECT 1000012,'EM','Ms.','Meredith','Palmer',0,SYSUTCDATETIME()
WHERE NOT EXISTS (SELECT 1 FROM person.Person p WHERE P.BusinessEntityID = 1000012)

INSERT INTO person.Person (BusinessEntityID,PersonType,Title,FirstName,LastName,EmailPromotion,ModifiedDate)
SELECT 1000013,'EM','Mr.','Creed','Bratton',0,SYSUTCDATETIME()
WHERE NOT EXISTS (SELECT 1 FROM person.Person p WHERE P.BusinessEntityID = 1000013)

INSERT INTO person.Person (BusinessEntityID,PersonType,Title,FirstName,LastName,EmailPromotion,ModifiedDate)
SELECT 1000014,'EM','Ms.','Erin','Hannon',0,SYSUTCDATETIME()
WHERE NOT EXISTS (SELECT 1 FROM person.Person p WHERE P.BusinessEntityID = 1000014)

INSERT INTO person.Person (BusinessEntityID,PersonType,Title,FirstName,LastName,EmailPromotion,ModifiedDate)
SELECT 1000015,'EM','Mr.','Darryl','Philbin',0,SYSUTCDATETIME()
WHERE NOT EXISTS (SELECT 1 FROM person.Person p WHERE P.BusinessEntityID = 1000015)

INSERT INTO person.Person (BusinessEntityID,PersonType,Title,FirstName,LastName,EmailPromotion,ModifiedDate)
SELECT 1000016,'EM','Mr.','Gabe','Lewis',0,SYSUTCDATETIME()
WHERE NOT EXISTS (SELECT 1 FROM person.Person p WHERE P.BusinessEntityID = 1000016)

INSERT INTO person.Person (BusinessEntityID,PersonType,Title,FirstName,LastName,EmailPromotion,ModifiedDate)
SELECT 1000017,'EM','Mrs.','Holly','Flax',0,SYSUTCDATETIME()
WHERE NOT EXISTS (SELECT 1 FROM person.Person p WHERE P.BusinessEntityID = 1000017)

INSERT INTO person.Person (BusinessEntityID,PersonType,Title,FirstName,LastName,EmailPromotion,ModifiedDate)
SELECT 1000018,'EM','Mr.','Toby','Flenderson',0,SYSUTCDATETIME()
WHERE NOT EXISTS (SELECT 1 FROM person.Person p WHERE P.BusinessEntityID = 1000018)

INSERT INTO person.Person (BusinessEntityID,PersonType,Title,FirstName,LastName,EmailPromotion,ModifiedDate)
SELECT 1000019,'EM','Mr.','David','Wallace',0,SYSUTCDATETIME()
WHERE NOT EXISTS (SELECT 1 FROM person.Person p WHERE P.BusinessEntityID = 1000019)

INSERT INTO person.Person (BusinessEntityID,PersonType,Title,FirstName,LastName,EmailPromotion,ModifiedDate)
SELECT 1000020,'EM','Ms.','Jan','Levinson',0,SYSUTCDATETIME()
WHERE NOT EXISTS (SELECT 1 FROM person.Person p WHERE P.BusinessEntityID = 1000020)

INSERT INTO person.Person (BusinessEntityID,PersonType,Title,FirstName,LastName,EmailPromotion,ModifiedDate)
SELECT 1000021,'EM','Mr.','Deangelo','Vickers',0,SYSUTCDATETIME()
WHERE NOT EXISTS (SELECT 1 FROM person.Person p WHERE P.BusinessEntityID = 1000021)

INSERT INTO person.Person (BusinessEntityID,PersonType,Title,FirstName,LastName,EmailPromotion,ModifiedDate)
SELECT 1000022,'EM','Ms.','Karen','Filippelli',0,SYSUTCDATETIME()
WHERE NOT EXISTS (SELECT 1 FROM person.Person p WHERE P.BusinessEntityID = 1000022)

INSERT INTO person.Person (BusinessEntityID,PersonType,Title,FirstName,LastName,EmailPromotion,ModifiedDate)
SELECT 1000023,'EM','Mr.','Todd','Packer',0,SYSUTCDATETIME()
WHERE NOT EXISTS (SELECT 1 FROM person.Person p WHERE P.BusinessEntityID = 1000023)

INSERT INTO person.Person (BusinessEntityID,PersonType,Title,FirstName,LastName,EmailPromotion,ModifiedDate)
SELECT 1000024,'EM','Mr.','Clark','Green',0,SYSUTCDATETIME()
WHERE NOT EXISTS (SELECT 1 FROM person.Person p WHERE P.BusinessEntityID = 1000024)

INSERT INTO person.Person (BusinessEntityID,PersonType,Title,FirstName,LastName,EmailPromotion,ModifiedDate)
SELECT 1000025,'EM','Mr.','Pete','Miller',0,SYSUTCDATETIME()
WHERE NOT EXISTS (SELECT 1 FROM person.Person p WHERE P.BusinessEntityID = 1000025)

INSERT INTO person.Person (BusinessEntityID,PersonType,Title,FirstName,LastName,EmailPromotion,ModifiedDate)
SELECT 1000026,'EM','Ms.','Nellie','Bertram',0,SYSUTCDATETIME()
WHERE NOT EXISTS (SELECT 1 FROM person.Person p WHERE P.BusinessEntityID = 1000026)

INSERT INTO person.Person (BusinessEntityID,PersonType,Title,FirstName,LastName,EmailPromotion,ModifiedDate)
SELECT 1000027,'EM','Mrs.','Phyllis','Vance',0,SYSUTCDATETIME()
WHERE NOT EXISTS (SELECT 1 FROM person.Person p WHERE P.BusinessEntityID = 1000027)

INSERT INTO person.Person (BusinessEntityID,PersonType,Title,FirstName,LastName,EmailPromotion,ModifiedDate)
SELECT 1000028,'EM','Mr.','Andy','Bernard',0,SYSUTCDATETIME()
WHERE NOT EXISTS (SELECT 1 FROM person.Person p WHERE P.BusinessEntityID = 1000028)


```

### Shield.UserAccount

```sql
INSERT INTO Shield.UserAccount (BusinessEntityID,rowguid,ModifiedDate,UserName,PasswordHash)
SELECT 1000001, 'A0C8FD52-2921-4279-AC4A-35255DD36733', SYSUTCDATETIME(), 'mick.letofsky', '' 
WHERE NOT EXISTS (SELECT 1 FROM Shield.UserAccount WHERE BusinessEntityID = 1000001)

INSERT INTO Shield.UserAccount (BusinessEntityID,rowguid,ModifiedDate,UserName,PasswordHash)
SELECT 1000002, '4D8222D1-FA91-4BA2-BF27-1933A7457E67', SYSUTCDATETIME(), 'jim.halpert', '' 
WHERE NOT EXISTS (SELECT 1 FROM Shield.UserAccount WHERE BusinessEntityID = 1000002)

INSERT INTO Shield.UserAccount (BusinessEntityID,rowguid,ModifiedDate,UserName,PasswordHash)
SELECT 1000003, 'D1A4B6DE-1D25-468B-846A-F667E3D79280', SYSUTCDATETIME(), 'michael.scott', '' 
WHERE NOT EXISTS (SELECT 1 FROM Shield.UserAccount WHERE BusinessEntityID = 1000003)

INSERT INTO Shield.UserAccount (BusinessEntityID,rowguid,ModifiedDate,UserName,PasswordHash)
SELECT 1000004, '7C0DAD50-5D76-41A6-95CC-A61B0E894383', SYSUTCDATETIME(), 'pam.beesly (halpert)', '' 
WHERE NOT EXISTS (SELECT 1 FROM Shield.UserAccount WHERE BusinessEntityID = 1000004)

INSERT INTO Shield.UserAccount (BusinessEntityID,rowguid,ModifiedDate,UserName,PasswordHash)
SELECT 1000005, '69F9CBD4-5A12-4AC4-9586-1F3270466D59', SYSUTCDATETIME(), 'dwight.schrute', '' 
WHERE NOT EXISTS (SELECT 1 FROM Shield.UserAccount WHERE BusinessEntityID = 1000005)

INSERT INTO Shield.UserAccount (BusinessEntityID,rowguid,ModifiedDate,UserName,PasswordHash)
SELECT 1000006, '59E46E5F-E7B9-48DD-AB68-C47648A5A39E', SYSUTCDATETIME(), 'stanley.hudson', '' 
WHERE NOT EXISTS (SELECT 1 FROM Shield.UserAccount WHERE BusinessEntityID = 1000006)

INSERT INTO Shield.UserAccount (BusinessEntityID,rowguid,ModifiedDate,UserName,PasswordHash)
SELECT 1000007, 'A70E01D1-CCD6-4C1C-B67F-41EB516AE154', SYSUTCDATETIME(), 'kevin.malone', '' 
WHERE NOT EXISTS (SELECT 1 FROM Shield.UserAccount WHERE BusinessEntityID = 1000007)

INSERT INTO Shield.UserAccount (BusinessEntityID,rowguid,ModifiedDate,UserName,PasswordHash)
SELECT 1000008, 'A991BD30-429A-4342-9289-BFEC1C6B41D2', SYSUTCDATETIME(), 'ryan.howard', '' 
WHERE NOT EXISTS (SELECT 1 FROM Shield.UserAccount WHERE BusinessEntityID = 1000008)

INSERT INTO Shield.UserAccount (BusinessEntityID,rowguid,ModifiedDate,UserName,PasswordHash)
SELECT 1000009, 'A6FC67CF-2EE7-43A4-A960-4BEB89943CE7', SYSUTCDATETIME(), 'angela.schrute (martin)', '' 
WHERE NOT EXISTS (SELECT 1 FROM Shield.UserAccount WHERE BusinessEntityID = 1000009)

INSERT INTO Shield.UserAccount (BusinessEntityID,rowguid,ModifiedDate,UserName,PasswordHash)
SELECT 1000010, '662051A9-2601-4C31-BDCC-3F3B41E7095B', SYSUTCDATETIME(), 'oscar.martinez', '' 
WHERE NOT EXISTS (SELECT 1 FROM Shield.UserAccount WHERE BusinessEntityID = 1000010)

INSERT INTO Shield.UserAccount (BusinessEntityID,rowguid,ModifiedDate,UserName,PasswordHash)
SELECT 1000011, '3B4C4B23-8CD6-4C1A-9E71-74AE1D228E3C', SYSUTCDATETIME(), 'kelly.kapoor', '' 
WHERE NOT EXISTS (SELECT 1 FROM Shield.UserAccount WHERE BusinessEntityID = 1000011)

INSERT INTO Shield.UserAccount (BusinessEntityID,rowguid,ModifiedDate,UserName,PasswordHash)
SELECT 1000012, 'AE71658B-C1B8-4836-A5C1-4515B5851039', SYSUTCDATETIME(), 'meredith.palmer', '' 
WHERE NOT EXISTS (SELECT 1 FROM Shield.UserAccount WHERE BusinessEntityID = 1000012)

INSERT INTO Shield.UserAccount (BusinessEntityID,rowguid,ModifiedDate,UserName,PasswordHash)
SELECT 1000013, '5496C265-3A7F-4ACC-B199-688C1E1D01D7', SYSUTCDATETIME(), 'creed.bratton', '' 
WHERE NOT EXISTS (SELECT 1 FROM Shield.UserAccount WHERE BusinessEntityID = 1000013)

INSERT INTO Shield.UserAccount (BusinessEntityID,rowguid,ModifiedDate,UserName,PasswordHash)
SELECT 1000014, 'BCD35B24-DED7-4737-A4C0-8DEED107D681', SYSUTCDATETIME(), 'erin.hannon', '' 
WHERE NOT EXISTS (SELECT 1 FROM Shield.UserAccount WHERE BusinessEntityID = 1000014)

INSERT INTO Shield.UserAccount (BusinessEntityID,rowguid,ModifiedDate,UserName,PasswordHash)
SELECT 1000015, '8D29FE9F-855D-4E1C-93AD-92F22799C4CD', SYSUTCDATETIME(), 'darryl.philbin', '' 
WHERE NOT EXISTS (SELECT 1 FROM Shield.UserAccount WHERE BusinessEntityID = 1000015)

INSERT INTO Shield.UserAccount (BusinessEntityID,rowguid,ModifiedDate,UserName,PasswordHash)
SELECT 1000016, '41414922-C0F4-4B6D-8F2E-D56E2ECA1567', SYSUTCDATETIME(), 'gabe.lewis', '' 
WHERE NOT EXISTS (SELECT 1 FROM Shield.UserAccount WHERE BusinessEntityID = 1000016)

INSERT INTO Shield.UserAccount (BusinessEntityID,rowguid,ModifiedDate,UserName,PasswordHash)
SELECT 1000017, '832A2598-6F28-428E-B6E4-E2CE548B3CE4', SYSUTCDATETIME(), 'holly.flax', '' 
WHERE NOT EXISTS (SELECT 1 FROM Shield.UserAccount WHERE BusinessEntityID = 1000017)

INSERT INTO Shield.UserAccount (BusinessEntityID,rowguid,ModifiedDate,UserName,PasswordHash)
SELECT 1000018, '392F753A-2621-4DC3-99C8-BF48A365444B', SYSUTCDATETIME(), 'toby.flenderson', '' 
WHERE NOT EXISTS (SELECT 1 FROM Shield.UserAccount WHERE BusinessEntityID = 1000018)

INSERT INTO Shield.UserAccount (BusinessEntityID,rowguid,ModifiedDate,UserName,PasswordHash)
SELECT 1000019, 'D4F694D0-485D-4373-94F6-91899D0E7FFE', SYSUTCDATETIME(), 'david.wallace', '' 
WHERE NOT EXISTS (SELECT 1 FROM Shield.UserAccount WHERE BusinessEntityID = 1000019)

INSERT INTO Shield.UserAccount (BusinessEntityID,rowguid,ModifiedDate,UserName,PasswordHash)
SELECT 1000020, '6D0DAA29-7802-4E8C-8AE0-07BEDA636C17', SYSUTCDATETIME(), 'jan.levinson', '' 
WHERE NOT EXISTS (SELECT 1 FROM Shield.UserAccount WHERE BusinessEntityID = 1000020)

INSERT INTO Shield.UserAccount (BusinessEntityID,rowguid,ModifiedDate,UserName,PasswordHash)
SELECT 1000021, '5C91E051-4763-4032-A160-E0FE49E3A06F', SYSUTCDATETIME(), 'deangelo.vickers', '' 
WHERE NOT EXISTS (SELECT 1 FROM Shield.UserAccount WHERE BusinessEntityID = 1000021)

INSERT INTO Shield.UserAccount (BusinessEntityID,rowguid,ModifiedDate,UserName,PasswordHash)
SELECT 1000022, 'C27DB178-A67E-45BC-B416-136211E5D28C', SYSUTCDATETIME(), 'karen.filippelli', '' 
WHERE NOT EXISTS (SELECT 1 FROM Shield.UserAccount WHERE BusinessEntityID = 1000022)

INSERT INTO Shield.UserAccount (BusinessEntityID,rowguid,ModifiedDate,UserName,PasswordHash)
SELECT 1000023, '24CDAD3E-F7E6-4B80-8E79-A2BDD5D35A45', SYSUTCDATETIME(), 'todd.packer', '' 
WHERE NOT EXISTS (SELECT 1 FROM Shield.UserAccount WHERE BusinessEntityID = 1000023)

INSERT INTO Shield.UserAccount (BusinessEntityID,rowguid,ModifiedDate,UserName,PasswordHash)
SELECT 1000024, 'B18CA322-8297-4594-BE9C-DD9DCAD7E811', SYSUTCDATETIME(), 'clark.green', '' 
WHERE NOT EXISTS (SELECT 1 FROM Shield.UserAccount WHERE BusinessEntityID = 1000024)

INSERT INTO Shield.UserAccount (BusinessEntityID,rowguid,ModifiedDate,UserName,PasswordHash)
SELECT 1000025, '4E788977-1E69-4DCE-B912-5BCEFB6A7810', SYSUTCDATETIME(), 'pete.miller', '' 
WHERE NOT EXISTS (SELECT 1 FROM Shield.UserAccount WHERE BusinessEntityID = 1000025)

INSERT INTO Shield.UserAccount (BusinessEntityID,rowguid,ModifiedDate,UserName,PasswordHash)
SELECT 1000026, '70DDF1AC-F12B-4602-9719-A399528F9DF8', SYSUTCDATETIME(), 'nellie.bertram', '' 
WHERE NOT EXISTS (SELECT 1 FROM Shield.UserAccount WHERE BusinessEntityID = 1000026)

INSERT INTO Shield.UserAccount (BusinessEntityID,rowguid,ModifiedDate,UserName,PasswordHash)
SELECT 1000027, '6DB51C98-697F-40B3-9EF2-127F9C6D5656', SYSUTCDATETIME(), 'phyllis.vance', '' 
WHERE NOT EXISTS (SELECT 1 FROM Shield.UserAccount WHERE BusinessEntityID = 1000027)

INSERT INTO Shield.UserAccount (BusinessEntityID,rowguid,ModifiedDate,UserName,PasswordHash)
SELECT 1000028, '8EC39F1D-9610-408A-9D52-28F5A7118541', SYSUTCDATETIME(), 'andy.bernard', '' 
WHERE NOT EXISTS (SELECT 1 FROM Shield.UserAccount WHERE BusinessEntityID = 1000028)

```

### person.EmailAddress

```sql
INSERT INTO person.EmailAddress (BusinessEntityID,rowguid,ModifiedDate,EmailAddress)
SELECT 1000001, 'CA17E05E-712A-46CE-875C-C99A1E2665A1', SYSUTCDATETIME(), 'mick.letofsky@adventure-works.com' 
WHERE NOT EXISTS (SELECT 1 FROM person.EmailAddress WHERE BusinessEntityID = 1000001)

INSERT INTO person.EmailAddress (BusinessEntityID,rowguid,ModifiedDate,EmailAddress)
SELECT 1000002, '3D9AF33B-11E2-43C0-AF37-F7AE13BBBAC1', SYSUTCDATETIME(), 'jim.halpert@adventure-works.com' 
WHERE NOT EXISTS (SELECT 1 FROM person.EmailAddress WHERE BusinessEntityID = 1000002)

INSERT INTO person.EmailAddress (BusinessEntityID,rowguid,ModifiedDate,EmailAddress)
SELECT 1000003, 'ADA7D85E-8C6D-4781-BC58-F41BCE203266', SYSUTCDATETIME(), 'michael.scott@adventure-works.com' 
WHERE NOT EXISTS (SELECT 1 FROM person.EmailAddress WHERE BusinessEntityID = 1000003)

INSERT INTO person.EmailAddress (BusinessEntityID,rowguid,ModifiedDate,EmailAddress)
SELECT 1000004, 'D6F6189B-6E32-4974-8E83-E11946C81341', SYSUTCDATETIME(), 'pam.beesly (halpert)@adventure-works.com' 
WHERE NOT EXISTS (SELECT 1 FROM person.EmailAddress WHERE BusinessEntityID = 1000004)

INSERT INTO person.EmailAddress (BusinessEntityID,rowguid,ModifiedDate,EmailAddress)
SELECT 1000005, 'CBA96689-89B9-42CD-B9EF-2EAF862099EF', SYSUTCDATETIME(), 'dwight.schrute@adventure-works.com' 
WHERE NOT EXISTS (SELECT 1 FROM person.EmailAddress WHERE BusinessEntityID = 1000005)

INSERT INTO person.EmailAddress (BusinessEntityID,rowguid,ModifiedDate,EmailAddress)
SELECT 1000006, '39D92517-8698-47A7-80C1-27FF1B24539B', SYSUTCDATETIME(), 'stanley.hudson@adventure-works.com' 
WHERE NOT EXISTS (SELECT 1 FROM person.EmailAddress WHERE BusinessEntityID = 1000006)

INSERT INTO person.EmailAddress (BusinessEntityID,rowguid,ModifiedDate,EmailAddress)
SELECT 1000007, '4CF9D07A-7E3C-452B-B001-F04FEB6E3389', SYSUTCDATETIME(), 'kevin.malone@adventure-works.com' 
WHERE NOT EXISTS (SELECT 1 FROM person.EmailAddress WHERE BusinessEntityID = 1000007)

INSERT INTO person.EmailAddress (BusinessEntityID,rowguid,ModifiedDate,EmailAddress)
SELECT 1000008, '85AFD059-0E33-4B40-B062-F08157932AA5', SYSUTCDATETIME(), 'ryan.howard@adventure-works.com' 
WHERE NOT EXISTS (SELECT 1 FROM person.EmailAddress WHERE BusinessEntityID = 1000008)

INSERT INTO person.EmailAddress (BusinessEntityID,rowguid,ModifiedDate,EmailAddress)
SELECT 1000009, 'DEEDD284-C287-451E-9686-B546BB8AC5CC', SYSUTCDATETIME(), 'angela.schrute (martin)@adventure-works.com' 
WHERE NOT EXISTS (SELECT 1 FROM person.EmailAddress WHERE BusinessEntityID = 1000009)

INSERT INTO person.EmailAddress (BusinessEntityID,rowguid,ModifiedDate,EmailAddress)
SELECT 1000010, 'A89CCC5C-70BD-4FF1-B26D-A8F64F89224E', SYSUTCDATETIME(), 'oscar.martinez@adventure-works.com' 
WHERE NOT EXISTS (SELECT 1 FROM person.EmailAddress WHERE BusinessEntityID = 1000010)

INSERT INTO person.EmailAddress (BusinessEntityID,rowguid,ModifiedDate,EmailAddress)
SELECT 1000011, 'C3D9C5EB-C6AB-4BD1-B04A-40B50622CE12', SYSUTCDATETIME(), 'kelly.kapoor@adventure-works.com' 
WHERE NOT EXISTS (SELECT 1 FROM person.EmailAddress WHERE BusinessEntityID = 1000011)

INSERT INTO person.EmailAddress (BusinessEntityID,rowguid,ModifiedDate,EmailAddress)
SELECT 1000012, '7F9BF182-7A52-4DAC-BFC7-F20BA01D86E8', SYSUTCDATETIME(), 'meredith.palmer@adventure-works.com' 
WHERE NOT EXISTS (SELECT 1 FROM person.EmailAddress WHERE BusinessEntityID = 1000012)

INSERT INTO person.EmailAddress (BusinessEntityID,rowguid,ModifiedDate,EmailAddress)
SELECT 1000013, '550EAE00-BAD9-4FC0-AD19-F33B566F1630', SYSUTCDATETIME(), 'creed.bratton@adventure-works.com' 
WHERE NOT EXISTS (SELECT 1 FROM person.EmailAddress WHERE BusinessEntityID = 1000013)

INSERT INTO person.EmailAddress (BusinessEntityID,rowguid,ModifiedDate,EmailAddress)
SELECT 1000014, '8A0CD6C8-EE5D-4DBD-A798-959B4C5E550F', SYSUTCDATETIME(), 'erin.hannon@adventure-works.com' 
WHERE NOT EXISTS (SELECT 1 FROM person.EmailAddress WHERE BusinessEntityID = 1000014)

INSERT INTO person.EmailAddress (BusinessEntityID,rowguid,ModifiedDate,EmailAddress)
SELECT 1000015, '6F28CD44-B61B-4398-B34B-6B9B9928F483', SYSUTCDATETIME(), 'darryl.philbin@adventure-works.com' 
WHERE NOT EXISTS (SELECT 1 FROM person.EmailAddress WHERE BusinessEntityID = 1000015)

INSERT INTO person.EmailAddress (BusinessEntityID,rowguid,ModifiedDate,EmailAddress)
SELECT 1000016, 'D643ECCD-C699-445B-90D3-0B86121F9354', SYSUTCDATETIME(), 'gabe.lewis@adventure-works.com' 
WHERE NOT EXISTS (SELECT 1 FROM person.EmailAddress WHERE BusinessEntityID = 1000016)

INSERT INTO person.EmailAddress (BusinessEntityID,rowguid,ModifiedDate,EmailAddress)
SELECT 1000017, 'DF424511-8560-4DB1-A4F7-79B43E212968', SYSUTCDATETIME(), 'holly.flax@adventure-works.com' 
WHERE NOT EXISTS (SELECT 1 FROM person.EmailAddress WHERE BusinessEntityID = 1000017)

INSERT INTO person.EmailAddress (BusinessEntityID,rowguid,ModifiedDate,EmailAddress)
SELECT 1000018, '878FE336-13F5-41EC-9BB5-7DF1ECBC2C62', SYSUTCDATETIME(), 'toby.flenderson@adventure-works.com' 
WHERE NOT EXISTS (SELECT 1 FROM person.EmailAddress WHERE BusinessEntityID = 1000018)

INSERT INTO person.EmailAddress (BusinessEntityID,rowguid,ModifiedDate,EmailAddress)
SELECT 1000019, '779C3200-4512-4C31-9E34-CBE96A6333D8', SYSUTCDATETIME(), 'david.wallace@adventure-works.com' 
WHERE NOT EXISTS (SELECT 1 FROM person.EmailAddress WHERE BusinessEntityID = 1000019)

INSERT INTO person.EmailAddress (BusinessEntityID,rowguid,ModifiedDate,EmailAddress)
SELECT 1000020, '0EECBE8B-68C6-46E7-8D8B-1AE6F525F5D2', SYSUTCDATETIME(), 'jan.levinson@adventure-works.com' 
WHERE NOT EXISTS (SELECT 1 FROM person.EmailAddress WHERE BusinessEntityID = 1000020)

INSERT INTO person.EmailAddress (BusinessEntityID,rowguid,ModifiedDate,EmailAddress)
SELECT 1000021, '2F032425-0321-4E38-9FB3-6DD7089EC41B', SYSUTCDATETIME(), 'deangelo.vickers@adventure-works.com' 
WHERE NOT EXISTS (SELECT 1 FROM person.EmailAddress WHERE BusinessEntityID = 1000021)

INSERT INTO person.EmailAddress (BusinessEntityID,rowguid,ModifiedDate,EmailAddress)
SELECT 1000022, '7DC27F34-2E98-4498-AB42-097AD10D96C2', SYSUTCDATETIME(), 'karen.filippelli@adventure-works.com' 
WHERE NOT EXISTS (SELECT 1 FROM person.EmailAddress WHERE BusinessEntityID = 1000022)

INSERT INTO person.EmailAddress (BusinessEntityID,rowguid,ModifiedDate,EmailAddress)
SELECT 1000023, 'CB147DD8-82CD-4841-B916-C4EECD383DC8', SYSUTCDATETIME(), 'todd.packer@adventure-works.com' 
WHERE NOT EXISTS (SELECT 1 FROM person.EmailAddress WHERE BusinessEntityID = 1000023)

INSERT INTO person.EmailAddress (BusinessEntityID,rowguid,ModifiedDate,EmailAddress)
SELECT 1000024, 'DEFA4797-E5A4-4690-8793-7131DECEC1F8', SYSUTCDATETIME(), 'clark.green@adventure-works.com' 
WHERE NOT EXISTS (SELECT 1 FROM person.EmailAddress WHERE BusinessEntityID = 1000024)

INSERT INTO person.EmailAddress (BusinessEntityID,rowguid,ModifiedDate,EmailAddress)
SELECT 1000025, '0E95E900-261D-4E9C-92D8-AB3F2A6E17FA', SYSUTCDATETIME(), 'pete.miller@adventure-works.com' 
WHERE NOT EXISTS (SELECT 1 FROM person.EmailAddress WHERE BusinessEntityID = 1000025)

INSERT INTO person.EmailAddress (BusinessEntityID,rowguid,ModifiedDate,EmailAddress)
SELECT 1000026, 'D489D6A7-B813-44D8-A409-8143F8ADA2B1', SYSUTCDATETIME(), 'nellie.bertram@adventure-works.com' 
WHERE NOT EXISTS (SELECT 1 FROM person.EmailAddress WHERE BusinessEntityID = 1000026)

INSERT INTO person.EmailAddress (BusinessEntityID,rowguid,ModifiedDate,EmailAddress)
SELECT 1000027, '8A4EDD74-F882-45DD-ADDE-A4D82D52B2D7', SYSUTCDATETIME(), 'phyllis.vance@adventure-works.com' 
WHERE NOT EXISTS (SELECT 1 FROM person.EmailAddress WHERE BusinessEntityID = 1000027)

INSERT INTO person.EmailAddress (BusinessEntityID,rowguid,ModifiedDate,EmailAddress)
SELECT 1000028, '666E6DA3-F904-4F6E-A7BA-CCB52008DA04', SYSUTCDATETIME(), 'andy.bernard@adventure-works.com' 
WHERE NOT EXISTS (SELECT 1 FROM person.EmailAddress WHERE BusinessEntityID = 1000028)

```

## Create records for new authorization tables

### Shield.SecurityRole

```sql
SET IDENTITY_INSERT Shield.SecurityRole ON;

INSERT INTO Shield.SecurityRole (RoleId,RoleGuid,RoleName,RoleDescription,CreatedBy,CreatedOn,ModifiedBy,ModifiedOn)
SELECT 1001,'9f0068a4-46a1-44bf-a9a0-b6a1e25a2108','Global Administrator','Full access to all features within the applicaton.',1000001,SYSUTCDATETIME(),1000001,SYSUTCDATETIME()
WHERE NOT EXISTS (SELECT 1 FROM Shield.SecurityRole WHERE RoleId = 1001)

INSERT INTO Shield.SecurityRole (RoleId,RoleGuid,RoleName,RoleDescription,CreatedBy,CreatedOn,ModifiedBy,ModifiedOn)
SELECT 1002,'54521272-7763-467d-8e04-1573ea0dae56','Help Desk Administrator','Full access to all security configuration and user access features.',1000001,SYSUTCDATETIME(),1000001,SYSUTCDATETIME()
WHERE NOT EXISTS (SELECT 1 FROM Shield.SecurityRole WHERE RoleId = 1002)

INSERT INTO Shield.SecurityRole (RoleId,RoleGuid,RoleName,RoleDescription,CreatedBy,CreatedOn,ModifiedBy,ModifiedOn)
SELECT 1003,'7314f1cc-c28c-443b-ae5a-383135c8648c','Human Resources Employee','Limited access to human resources features.',1000001,SYSUTCDATETIME(),1000001,SYSUTCDATETIME()
WHERE NOT EXISTS (SELECT 1 FROM Shield.SecurityRole WHERE RoleId = 1003)

INSERT INTO Shield.SecurityRole (RoleId,RoleGuid,RoleName,RoleDescription,CreatedBy,CreatedOn,ModifiedBy,ModifiedOn)
SELECT 1004,'9b07e30d-1cf0-4cb9-afca-0bd3fba3c232','Human Resources Manager','Full access to human resources features.',1000001,SYSUTCDATETIME(),1000001,SYSUTCDATETIME()
WHERE NOT EXISTS (SELECT 1 FROM Shield.SecurityRole WHERE RoleId = 1004)

INSERT INTO Shield.SecurityRole (RoleId,RoleGuid,RoleName,RoleDescription,CreatedBy,CreatedOn,ModifiedBy,ModifiedOn)
SELECT 1005,'4a3c7403-e2fa-4017-8600-f4deb90feb16','Employee','Full access to employee only features. Limited access to customer information.',1000001,SYSUTCDATETIME(),1000001,SYSUTCDATETIME()
WHERE NOT EXISTS (SELECT 1 FROM Shield.SecurityRole WHERE RoleId = 1005)

INSERT INTO Shield.SecurityRole (RoleId,RoleGuid,RoleName,RoleDescription,CreatedBy,CreatedOn,ModifiedBy,ModifiedOn)
SELECT 1006,'de6dd70f-9b2d-4fd7-b7b9-0aed5c36b669','Customer','Full access to customer only features. No access to internal company features',1000001,SYSUTCDATETIME(),1000001,SYSUTCDATETIME()
WHERE NOT EXISTS (SELECT 1 FROM Shield.SecurityRole WHERE RoleId = 1006)

INSERT INTO Shield.SecurityRole (RoleId,RoleGuid,RoleName,RoleDescription,CreatedBy,CreatedOn,ModifiedBy,ModifiedOn)
SELECT 1007,'74791e93-d90f-4998-ad38-4126ae000394','Store Contact','Full access to individual store features. Limited access to customer features',1000001,SYSUTCDATETIME(),1000001,SYSUTCDATETIME()
WHERE NOT EXISTS (SELECT 1 FROM Shield.SecurityRole WHERE RoleId = 1007)

INSERT INTO Shield.SecurityRole (RoleId,RoleGuid,RoleName,RoleDescription,CreatedBy,CreatedOn,ModifiedBy,ModifiedOn)
SELECT 1008,'62b23ec1-062f-4f42-916a-9e9700c9bd87','Sales Associate','Full access to individual sales person features. Full access to customer features.',1000001,SYSUTCDATETIME(),1000001,SYSUTCDATETIME()
WHERE NOT EXISTS (SELECT 1 FROM Shield.SecurityRole WHERE RoleId = 1008)

INSERT INTO Shield.SecurityRole (RoleId,RoleGuid,RoleName,RoleDescription,CreatedBy,CreatedOn,ModifiedBy,ModifiedOn)
SELECT 1009,'e21c528c-56ff-40e1-8f5b-5973bb1e7585','Sales Manager','Full access to all sales person features. Full access to customer features.',1000001,SYSUTCDATETIME(),1000001,SYSUTCDATETIME()
WHERE NOT EXISTS (SELECT 1 FROM Shield.SecurityRole WHERE RoleId = 1009)

SET IDENTITY_INSERT Shield.SecurityRole OFF;
```

### Shield.SecurityGroup

```sql
SET IDENTITY_INSERT Shield.SecurityGroup ON;

INSERT INTO Shield.SecurityGroup(GroupId, GroupGuid, GroupName, GroupDescription, CreatedBy, CreatedOn, ModifiedBy, ModifiedOn)
SELECT 2001,'4c33e231-d072-4761-b534-e3bd72259002','Global Administrators','Users in this group are intended to have complete system access. Care should be taken to limit membership.',1000001,SYSUTCDATETIME(),1000001,SYSUTCDATETIME()
WHERE NOT EXISTS (SELECT 1 FROM Shield.SecurityGroup WHERE GroupId = 2001)

INSERT INTO Shield.SecurityGroup(GroupId, GroupGuid, GroupName, GroupDescription, CreatedBy, CreatedOn, ModifiedBy, ModifiedOn)
SELECT 2002,'ad38c9d3-4080-408a-b29c-b9f835833dff','Help Desk Administrators','Users in this group are intended to have access to assist end-users in security concerns (e.g. resetting passwords). Care should be taken to limit membership.',1000001,SYSUTCDATETIME(),1000001,SYSUTCDATETIME()
WHERE NOT EXISTS (SELECT 1 FROM Shield.SecurityGroup WHERE GroupId = 2002)

INSERT INTO Shield.SecurityGroup(GroupId, GroupGuid, GroupName, GroupDescription, CreatedBy, CreatedOn, ModifiedBy, ModifiedOn)
SELECT 2003,'7e2d84be-54ac-44b0-be14-4de184ce6951','Human Resources - Northeast Team','Users in this group are dedicated to supporting employees in the Northeast territory',1000001,SYSUTCDATETIME(),1000001,SYSUTCDATETIME()
WHERE NOT EXISTS (SELECT 1 FROM Shield.SecurityGroup WHERE GroupId = 2003)

INSERT INTO Shield.SecurityGroup(GroupId, GroupGuid, GroupName, GroupDescription, CreatedBy, CreatedOn, ModifiedBy, ModifiedOn)
SELECT 2004,'67d62160-cccb-4736-8c58-134bc89a595c','Human Resources - Northeast Team','Users in this group are dedicated to supporting employees in the Northeast territory',1000001,SYSUTCDATETIME(),1000001,SYSUTCDATETIME()
WHERE NOT EXISTS (SELECT 1 FROM Shield.SecurityGroup WHERE GroupId = 2004)

INSERT INTO Shield.SecurityGroup(GroupId, GroupGuid, GroupName, GroupDescription, CreatedBy, CreatedOn, ModifiedBy, ModifiedOn)
SELECT 2005,'d61db981-14a2-4985-82d9-77b7b592a43b','Human Resources - Southeast Team','Users in this group are dedicated to supporting employees in the Southeast territory',1000001,SYSUTCDATETIME(),1000001,SYSUTCDATETIME()
WHERE NOT EXISTS (SELECT 1 FROM Shield.SecurityGroup WHERE GroupId = 2005)

INSERT INTO Shield.SecurityGroup(GroupId, GroupGuid, GroupName, GroupDescription, CreatedBy, CreatedOn, ModifiedBy, ModifiedOn)
SELECT 2006,'32c325e2-09db-4c4c-8ee6-65e9703e1752','Human Resources - Central Team','Users in this group are dedicated to supporting employees in the Central territory',1000001,SYSUTCDATETIME(),1000001,SYSUTCDATETIME()
WHERE NOT EXISTS (SELECT 1 FROM Shield.SecurityGroup WHERE GroupId = 2006)

INSERT INTO Shield.SecurityGroup(GroupId, GroupGuid, GroupName, GroupDescription, CreatedBy, CreatedOn, ModifiedBy, ModifiedOn)
SELECT 2007,'8c1cc4d3-132e-417f-bf0c-9830c25b2ec2','Human Resources - Northwest Team','Users in this group are dedicated to supporting employees in the Northwest territory',1000001,SYSUTCDATETIME(),1000001,SYSUTCDATETIME()
WHERE NOT EXISTS (SELECT 1 FROM Shield.SecurityGroup WHERE GroupId = 2007)

INSERT INTO Shield.SecurityGroup(GroupId, GroupGuid, GroupName, GroupDescription, CreatedBy, CreatedOn, ModifiedBy, ModifiedOn)
SELECT 2008,'38848701-7caa-41ba-90a6-4f027b33a39e','Human Resources - Southwest Team','Users in this group are dedicated to supporting employees in the Southwest territory',1000001,SYSUTCDATETIME(),1000001,SYSUTCDATETIME()
WHERE NOT EXISTS (SELECT 1 FROM Shield.SecurityGroup WHERE GroupId = 2008)

INSERT INTO Shield.SecurityGroup(GroupId, GroupGuid, GroupName, GroupDescription, CreatedBy, CreatedOn, ModifiedBy, ModifiedOn)
SELECT 2009,'3a59323d-7dd4-4618-8716-b618dd2cbfd5','Human Resources - International Team','Users in this group are dedicated to supporting employees in the International territories',1000001,SYSUTCDATETIME(),1000001,SYSUTCDATETIME()
WHERE NOT EXISTS (SELECT 1 FROM Shield.SecurityGroup WHERE GroupId = 2009)

INSERT INTO Shield.SecurityGroup(GroupId, GroupGuid, GroupName, GroupDescription, CreatedBy, CreatedOn, ModifiedBy, ModifiedOn)
SELECT 2010,'a7dcd0c5-774c-4409-84c1-a276eea13376','Human Resources - Management Team','Users in this group are HR managers at AdventureWorks Cycling',1000001,SYSUTCDATETIME(),1000001,SYSUTCDATETIME()
WHERE NOT EXISTS (SELECT 1 FROM Shield.SecurityGroup WHERE GroupId = 2010)

INSERT INTO Shield.SecurityGroup(GroupId, GroupGuid, GroupName, GroupDescription, CreatedBy, CreatedOn, ModifiedBy, ModifiedOn)
SELECT 2011,'d8fe6946-91f4-46fd-86c2-a2816b5f50ca','AdventureWorks - All Employees','Users in this group are employees at AdventureWorks Cycling',1000001,SYSUTCDATETIME(),1000001,SYSUTCDATETIME()
WHERE NOT EXISTS (SELECT 1 FROM Shield.SecurityGroup WHERE GroupId = 2011)

INSERT INTO Shield.SecurityGroup(GroupId, GroupGuid, GroupName, GroupDescription, CreatedBy, CreatedOn, ModifiedBy, ModifiedOn)
SELECT 2012,'adf7f7f7-6cd5-4936-be56-4d16df156380','AdventureWorks - All Customers','Users in this group are customers at AdventureWorks Cycling',1000001,SYSUTCDATETIME(),1000001,SYSUTCDATETIME()
WHERE NOT EXISTS (SELECT 1 FROM Shield.SecurityGroup WHERE GroupId = 2012)

INSERT INTO Shield.SecurityGroup(GroupId, GroupGuid, GroupName, GroupDescription, CreatedBy, CreatedOn, ModifiedBy, ModifiedOn)
SELECT 2013,'2fea450a-4e5a-4671-9cf0-e80bd5dc575e','AdventureWorks - USA Customers','Users in this group are customers within the United States of America',1000001,SYSUTCDATETIME(),1000001,SYSUTCDATETIME()
WHERE NOT EXISTS (SELECT 1 FROM Shield.SecurityGroup WHERE GroupId = 2013)

INSERT INTO Shield.SecurityGroup(GroupId, GroupGuid, GroupName, GroupDescription, CreatedBy, CreatedOn, ModifiedBy, ModifiedOn)
SELECT 2014,'d4e594aa-ff80-4ab6-b434-b1707ae12529','AdventureWorks - International Customers','Users in this group are customers outside the United States of America',1000001,SYSUTCDATETIME(),1000001,SYSUTCDATETIME()
WHERE NOT EXISTS (SELECT 1 FROM Shield.SecurityGroup WHERE GroupId = 2014)

INSERT INTO Shield.SecurityGroup(GroupId, GroupGuid, GroupName, GroupDescription, CreatedBy, CreatedOn, ModifiedBy, ModifiedOn)
SELECT 2015,'81874477-e5b8-4f3a-88dd-89915ffed4b8','Sales Associates - Northeast Team','Users in this group are dedicated to sales efforts in the Northeast territory',1000001,SYSUTCDATETIME(),1000001,SYSUTCDATETIME()
WHERE NOT EXISTS (SELECT 1 FROM Shield.SecurityGroup WHERE GroupId = 2015)

INSERT INTO Shield.SecurityGroup(GroupId, GroupGuid, GroupName, GroupDescription, CreatedBy, CreatedOn, ModifiedBy, ModifiedOn)
SELECT 2016,'f4c40081-2ec9-4083-ac14-db392c6f00f5','Sales Associates - Northeast Team','Users in this group are dedicated to sales efforts in the Northeast territory',1000001,SYSUTCDATETIME(),1000001,SYSUTCDATETIME()
WHERE NOT EXISTS (SELECT 1 FROM Shield.SecurityGroup WHERE GroupId = 2016)

INSERT INTO Shield.SecurityGroup(GroupId, GroupGuid, GroupName, GroupDescription, CreatedBy, CreatedOn, ModifiedBy, ModifiedOn)
SELECT 2017,'ababf0ce-1e82-432f-9cd6-d1663dd72630','Sales Associates - Southeast Team','Users in this group are dedicated to sales efforts in the Southeast territory',1000001,SYSUTCDATETIME(),1000001,SYSUTCDATETIME()
WHERE NOT EXISTS (SELECT 1 FROM Shield.SecurityGroup WHERE GroupId = 2017)

INSERT INTO Shield.SecurityGroup(GroupId, GroupGuid, GroupName, GroupDescription, CreatedBy, CreatedOn, ModifiedBy, ModifiedOn)
SELECT 2018,'ddac5480-ddaf-46d2-8d88-0500fe912641','Sales Associates - Central Team','Users in this group are dedicated to sales efforts in the Central territory',1000001,SYSUTCDATETIME(),1000001,SYSUTCDATETIME()
WHERE NOT EXISTS (SELECT 1 FROM Shield.SecurityGroup WHERE GroupId = 2018)

INSERT INTO Shield.SecurityGroup(GroupId, GroupGuid, GroupName, GroupDescription, CreatedBy, CreatedOn, ModifiedBy, ModifiedOn)
SELECT 2019,'55126c9f-a438-4b26-849a-6616c5f5f0d4','Sales Associates - Northwest Team','Users in this group are dedicated to sales efforts in the Northwest territory',1000001,SYSUTCDATETIME(),1000001,SYSUTCDATETIME()
WHERE NOT EXISTS (SELECT 1 FROM Shield.SecurityGroup WHERE GroupId = 2019)

INSERT INTO Shield.SecurityGroup(GroupId, GroupGuid, GroupName, GroupDescription, CreatedBy, CreatedOn, ModifiedBy, ModifiedOn)
SELECT 2020,'1bda9562-ad62-48c2-9eda-9a11bf5a1ce6','Sales Associates - Southwest Team','Users in this group are dedicated to sales efforts in the Southwest territory',1000001,SYSUTCDATETIME(),1000001,SYSUTCDATETIME()
WHERE NOT EXISTS (SELECT 1 FROM Shield.SecurityGroup WHERE GroupId = 2020)

INSERT INTO Shield.SecurityGroup(GroupId, GroupGuid, GroupName, GroupDescription, CreatedBy, CreatedOn, ModifiedBy, ModifiedOn)
SELECT 2021,'c9955b16-a148-4f46-ba47-8eae08bb6f15','Sales Associates - International Team','Users in this group are dedicated to sales efforts in the International territories',1000001,SYSUTCDATETIME(),1000001,SYSUTCDATETIME()
WHERE NOT EXISTS (SELECT 1 FROM Shield.SecurityGroup WHERE GroupId = 2021)

INSERT INTO Shield.SecurityGroup(GroupId, GroupGuid, GroupName, GroupDescription, CreatedBy, CreatedOn, ModifiedBy, ModifiedOn)
SELECT 2022,'bfc1354c-2dcc-43a9-a75a-ced02900ec48','Sales Management Team','Users in this group are Sales Managers at AdventureWorks Cycling',1000001,SYSUTCDATETIME(),1000001,SYSUTCDATETIME()
WHERE NOT EXISTS (SELECT 1 FROM Shield.SecurityGroup WHERE GroupId = 2022)

SET IDENTITY_INSERT Shield.SecurityGroup OFF;
```

### Shield.SecurityGroupSecurityRole

```sql
SET IDENTITY_INSERT Shield.SecurityGroupSecurityRole ON;

INSERT INTO Shield.SecurityGroupSecurityRole( SecurityGroupSecurityRoleId, SecurityGroupSecurityRoleGuid, GroupId, RoleId, CreatedBy, CreatedOn, ModifiedBy, ModifiedOn)
SELECT 10001, '10f64664-dc02-4544-bf93-29319073665c', 2001, 1001, 1000001,SYSUTCDATETIME(),1000001,SYSUTCDATETIME()
WHERE NOT EXISTS (SELECT 1 FROM Shield.SecurityGroupSecurityRole WHERE SecurityGroupSecurityRoleId = 10001)

INSERT INTO Shield.SecurityGroupSecurityRole( SecurityGroupSecurityRoleId, SecurityGroupSecurityRoleGuid, GroupId, RoleId, CreatedBy, CreatedOn, ModifiedBy, ModifiedOn)
SELECT 10002, 'f20d6c5c-7660-41e8-a15c-7b5f50ce4638', 2002, 1002, 1000001,SYSUTCDATETIME(),1000001,SYSUTCDATETIME()
WHERE NOT EXISTS (SELECT 1 FROM Shield.SecurityGroupSecurityRole WHERE SecurityGroupSecurityRoleId = 10002)

INSERT INTO Shield.SecurityGroupSecurityRole( SecurityGroupSecurityRoleId, SecurityGroupSecurityRoleGuid, GroupId, RoleId, CreatedBy, CreatedOn, ModifiedBy, ModifiedOn)
SELECT 10003, '57e41ef1-4dee-4cd1-9519-6803cefb1d77', 2003, 1003, 1000001,SYSUTCDATETIME(),1000001,SYSUTCDATETIME()
WHERE NOT EXISTS (SELECT 1 FROM Shield.SecurityGroupSecurityRole WHERE SecurityGroupSecurityRoleId = 10003)

INSERT INTO Shield.SecurityGroupSecurityRole( SecurityGroupSecurityRoleId, SecurityGroupSecurityRoleGuid, GroupId, RoleId, CreatedBy, CreatedOn, ModifiedBy, ModifiedOn)
SELECT 10004, '16d24176-72ff-4d7e-a92f-0d93236bad03', 2004, 1003, 1000001,SYSUTCDATETIME(),1000001,SYSUTCDATETIME()
WHERE NOT EXISTS (SELECT 1 FROM Shield.SecurityGroupSecurityRole WHERE SecurityGroupSecurityRoleId = 10004)

INSERT INTO Shield.SecurityGroupSecurityRole( SecurityGroupSecurityRoleId, SecurityGroupSecurityRoleGuid, GroupId, RoleId, CreatedBy, CreatedOn, ModifiedBy, ModifiedOn)
SELECT 10005, '87fb204c-f579-459a-b041-49b1236fb2e1', 2005, 1003, 1000001,SYSUTCDATETIME(),1000001,SYSUTCDATETIME()
WHERE NOT EXISTS (SELECT 1 FROM Shield.SecurityGroupSecurityRole WHERE SecurityGroupSecurityRoleId = 10005)

INSERT INTO Shield.SecurityGroupSecurityRole( SecurityGroupSecurityRoleId, SecurityGroupSecurityRoleGuid, GroupId, RoleId, CreatedBy, CreatedOn, ModifiedBy, ModifiedOn)
SELECT 10006, '957bc829-3078-4b78-97cf-aa60e40c7baa', 2006, 1003, 1000001,SYSUTCDATETIME(),1000001,SYSUTCDATETIME()
WHERE NOT EXISTS (SELECT 1 FROM Shield.SecurityGroupSecurityRole WHERE SecurityGroupSecurityRoleId = 10006)

INSERT INTO Shield.SecurityGroupSecurityRole( SecurityGroupSecurityRoleId, SecurityGroupSecurityRoleGuid, GroupId, RoleId, CreatedBy, CreatedOn, ModifiedBy, ModifiedOn)
SELECT 10007, 'cef03fa1-fc8f-4418-a6e9-7e2d28652520', 2007, 1003, 1000001,SYSUTCDATETIME(),1000001,SYSUTCDATETIME()
WHERE NOT EXISTS (SELECT 1 FROM Shield.SecurityGroupSecurityRole WHERE SecurityGroupSecurityRoleId = 10007)

INSERT INTO Shield.SecurityGroupSecurityRole( SecurityGroupSecurityRoleId, SecurityGroupSecurityRoleGuid, GroupId, RoleId, CreatedBy, CreatedOn, ModifiedBy, ModifiedOn)
SELECT 10008, 'b00c57aa-3f54-4afc-a6d3-8a534a29dd55', 2008, 1003, 1000001,SYSUTCDATETIME(),1000001,SYSUTCDATETIME()
WHERE NOT EXISTS (SELECT 1 FROM Shield.SecurityGroupSecurityRole WHERE SecurityGroupSecurityRoleId = 10008)

INSERT INTO Shield.SecurityGroupSecurityRole( SecurityGroupSecurityRoleId, SecurityGroupSecurityRoleGuid, GroupId, RoleId, CreatedBy, CreatedOn, ModifiedBy, ModifiedOn)
SELECT 10009, '97b34634-382a-4c55-9518-99897b0c0896', 2009, 1003, 1000001,SYSUTCDATETIME(),1000001,SYSUTCDATETIME()
WHERE NOT EXISTS (SELECT 1 FROM Shield.SecurityGroupSecurityRole WHERE SecurityGroupSecurityRoleId = 10009)

INSERT INTO Shield.SecurityGroupSecurityRole( SecurityGroupSecurityRoleId, SecurityGroupSecurityRoleGuid, GroupId, RoleId, CreatedBy, CreatedOn, ModifiedBy, ModifiedOn)
SELECT 10010, '17e4e530-e019-4064-91b4-694f5c205c4c', 2010, 1004, 1000001,SYSUTCDATETIME(),1000001,SYSUTCDATETIME()
WHERE NOT EXISTS (SELECT 1 FROM Shield.SecurityGroupSecurityRole WHERE SecurityGroupSecurityRoleId = 10010)

INSERT INTO Shield.SecurityGroupSecurityRole( SecurityGroupSecurityRoleId, SecurityGroupSecurityRoleGuid, GroupId, RoleId, CreatedBy, CreatedOn, ModifiedBy, ModifiedOn)
SELECT 10011, '4b203bd1-dfa9-43ad-833b-ace88a0d5b3b', 2011, 1005, 1000001,SYSUTCDATETIME(),1000001,SYSUTCDATETIME()
WHERE NOT EXISTS (SELECT 1 FROM Shield.SecurityGroupSecurityRole WHERE SecurityGroupSecurityRoleId = 10011)

INSERT INTO Shield.SecurityGroupSecurityRole( SecurityGroupSecurityRoleId, SecurityGroupSecurityRoleGuid, GroupId, RoleId, CreatedBy, CreatedOn, ModifiedBy, ModifiedOn)
SELECT 10012, '637bd768-2c58-443e-87a6-2c87dac7064e', 2012, 1006, 1000001,SYSUTCDATETIME(),1000001,SYSUTCDATETIME()
WHERE NOT EXISTS (SELECT 1 FROM Shield.SecurityGroupSecurityRole WHERE SecurityGroupSecurityRoleId = 10012)

INSERT INTO Shield.SecurityGroupSecurityRole( SecurityGroupSecurityRoleId, SecurityGroupSecurityRoleGuid, GroupId, RoleId, CreatedBy, CreatedOn, ModifiedBy, ModifiedOn)
SELECT 10013, 'b86adc7a-4eaa-4664-afe2-a35c86288e06', 2013, 1006, 1000001,SYSUTCDATETIME(),1000001,SYSUTCDATETIME()
WHERE NOT EXISTS (SELECT 1 FROM Shield.SecurityGroupSecurityRole WHERE SecurityGroupSecurityRoleId = 10013)

INSERT INTO Shield.SecurityGroupSecurityRole( SecurityGroupSecurityRoleId, SecurityGroupSecurityRoleGuid, GroupId, RoleId, CreatedBy, CreatedOn, ModifiedBy, ModifiedOn)
SELECT 10014, 'ada35bc1-07f4-4433-a477-3f9a20b89e60', 2014, 1006, 1000001,SYSUTCDATETIME(),1000001,SYSUTCDATETIME()
WHERE NOT EXISTS (SELECT 1 FROM Shield.SecurityGroupSecurityRole WHERE SecurityGroupSecurityRoleId = 10014)

INSERT INTO Shield.SecurityGroupSecurityRole( SecurityGroupSecurityRoleId, SecurityGroupSecurityRoleGuid, GroupId, RoleId, CreatedBy, CreatedOn, ModifiedBy, ModifiedOn)
SELECT 10015, '3deab5ef-6de6-495c-92ac-2b6becd33d3b', 2015, 1008, 1000001,SYSUTCDATETIME(),1000001,SYSUTCDATETIME()
WHERE NOT EXISTS (SELECT 1 FROM Shield.SecurityGroupSecurityRole WHERE SecurityGroupSecurityRoleId = 10015)

INSERT INTO Shield.SecurityGroupSecurityRole( SecurityGroupSecurityRoleId, SecurityGroupSecurityRoleGuid, GroupId, RoleId, CreatedBy, CreatedOn, ModifiedBy, ModifiedOn)
SELECT 10016, 'ae8c48c2-f0f7-48c5-8ef3-04fe10cad2b2', 2016, 1008, 1000001,SYSUTCDATETIME(),1000001,SYSUTCDATETIME()
WHERE NOT EXISTS (SELECT 1 FROM Shield.SecurityGroupSecurityRole WHERE SecurityGroupSecurityRoleId = 10016)

INSERT INTO Shield.SecurityGroupSecurityRole( SecurityGroupSecurityRoleId, SecurityGroupSecurityRoleGuid, GroupId, RoleId, CreatedBy, CreatedOn, ModifiedBy, ModifiedOn)
SELECT 10017, '2295e9c6-f600-440c-8684-88d362dcee81', 2017, 1008, 1000001,SYSUTCDATETIME(),1000001,SYSUTCDATETIME()
WHERE NOT EXISTS (SELECT 1 FROM Shield.SecurityGroupSecurityRole WHERE SecurityGroupSecurityRoleId = 10017)

INSERT INTO Shield.SecurityGroupSecurityRole( SecurityGroupSecurityRoleId, SecurityGroupSecurityRoleGuid, GroupId, RoleId, CreatedBy, CreatedOn, ModifiedBy, ModifiedOn)
SELECT 10018, '8e836c8a-b477-4342-bf88-ba4ff18463e4', 2018, 1008, 1000001,SYSUTCDATETIME(),1000001,SYSUTCDATETIME()
WHERE NOT EXISTS (SELECT 1 FROM Shield.SecurityGroupSecurityRole WHERE SecurityGroupSecurityRoleId = 10018)

INSERT INTO Shield.SecurityGroupSecurityRole( SecurityGroupSecurityRoleId, SecurityGroupSecurityRoleGuid, GroupId, RoleId, CreatedBy, CreatedOn, ModifiedBy, ModifiedOn)
SELECT 10019, '60692acf-330e-48db-a3c5-bd42022e794a', 2019, 1008, 1000001,SYSUTCDATETIME(),1000001,SYSUTCDATETIME()
WHERE NOT EXISTS (SELECT 1 FROM Shield.SecurityGroupSecurityRole WHERE SecurityGroupSecurityRoleId = 10019)

INSERT INTO Shield.SecurityGroupSecurityRole( SecurityGroupSecurityRoleId, SecurityGroupSecurityRoleGuid, GroupId, RoleId, CreatedBy, CreatedOn, ModifiedBy, ModifiedOn)
SELECT 10020, 'd14db92e-2185-40e6-afdf-5717304fbc52', 2020, 1008, 1000001,SYSUTCDATETIME(),1000001,SYSUTCDATETIME()
WHERE NOT EXISTS (SELECT 1 FROM Shield.SecurityGroupSecurityRole WHERE SecurityGroupSecurityRoleId = 10020)

INSERT INTO Shield.SecurityGroupSecurityRole( SecurityGroupSecurityRoleId, SecurityGroupSecurityRoleGuid, GroupId, RoleId, CreatedBy, CreatedOn, ModifiedBy, ModifiedOn)
SELECT 10021, '4a00f584-1943-43c7-b623-be85c09e3639', 2021, 1008, 1000001,SYSUTCDATETIME(),1000001,SYSUTCDATETIME()
WHERE NOT EXISTS (SELECT 1 FROM Shield.SecurityGroupSecurityRole WHERE SecurityGroupSecurityRoleId = 10021)

INSERT INTO Shield.SecurityGroupSecurityRole( SecurityGroupSecurityRoleId, SecurityGroupSecurityRoleGuid, GroupId, RoleId, CreatedBy, CreatedOn, ModifiedBy, ModifiedOn)
SELECT 10022, 'b0d04fbb-8f5f-48b4-94ee-c38d55f13137', 2022, 1008, 1000001,SYSUTCDATETIME(),1000001,SYSUTCDATETIME()
WHERE NOT EXISTS (SELECT 1 FROM Shield.SecurityGroupSecurityRole WHERE SecurityGroupSecurityRoleId = 10022)

INSERT INTO Shield.SecurityGroupSecurityRole( SecurityGroupSecurityRoleId, SecurityGroupSecurityRoleGuid, GroupId, RoleId, CreatedBy, CreatedOn, ModifiedBy, ModifiedOn)
SELECT 10023, 'd905deaa-27c0-4e5e-9227-2e8ed7d4f2e5', 2022, 1009, 1000001,SYSUTCDATETIME(),1000001,SYSUTCDATETIME()
WHERE NOT EXISTS (SELECT 1 FROM Shield.SecurityGroupSecurityRole WHERE SecurityGroupSecurityRoleId = 10023)

INSERT INTO Shield.SecurityGroupSecurityRole( SecurityGroupSecurityRoleId, SecurityGroupSecurityRoleGuid, GroupId, RoleId, CreatedBy, CreatedOn, ModifiedBy, ModifiedOn)
SELECT 10024, 'd6099a1c-7f2b-4815-9317-b2256be3fee5', 2010, 1003, 1000001,SYSUTCDATETIME(),1000001,SYSUTCDATETIME()
WHERE NOT EXISTS (SELECT 1 FROM Shield.SecurityGroupSecurityRole WHERE SecurityGroupSecurityRoleId = 10024)

SET IDENTITY_INSERT Shield.SecurityGroupSecurityRole OFF;
```

### Helper t-SQL queries 

Verify that all inserts are looking correct thus far

```sql
SELECT	xref.SecurityGroupSecurityRoleId
		,xref.SecurityGroupSecurityRoleGuid
		,xref.GroupId
		,xref.RoleId
		,sr.RoleName
		,sr.RoleDescription
		,sg.GroupName
		,sg.GroupDescription
FROM	Shield.SecurityGroupSecurityRole xref
		INNER JOIN Shield.SecurityRole sr ON xref.RoleId = sr.RoleId
		INNER JOIN Shield.SecurityGroup sg ON xref.GroupId = sg.GroupId


SELECT	* 
FROM	Shield.SecurityGroup sg
WHERE	NOT EXISTS (SELECT	1 
					FROM	Shield.SecurityGroupSecurityRole xref
					WHERE	sg.GroupId = xref.GroupId)

SELECT	* 
FROM	Shield.SecurityRole sr
WHERE	NOT EXISTS (SELECT	1 
					FROM	Shield.SecurityGroupSecurityRole xref
					WHERE	sr.RoleId = xref.RoleId)

```

## Add Foreign Keys

```sql

ALTER TABLE Shield.SecurityGroupSecurityFunction 
	WITH CHECK ADD CONSTRAINT FK_SecurityGroupSecurityFunction_GroupId FOREIGN KEY (GroupId)
	REFERENCES Shield.SecurityGroup (GroupId)

ALTER TABLE Shield.SecurityGroupSecurityFunction 
	WITH CHECK ADD CONSTRAINT FK_SecurityGroupSecurityFunction_FunctionId FOREIGN KEY (FunctionId)
	REFERENCES Shield.SecurityFunction (FunctionId)

ALTER TABLE Shield.SecurityGroupSecurityRole 
	WITH CHECK ADD CONSTRAINT FK_SecurityGroupSecurityRole_GroupId FOREIGN KEY (GroupId)
	REFERENCES Shield.SecurityGroup (GroupId)

ALTER TABLE Shield.SecurityGroupSecurityRole 
	WITH CHECK ADD CONSTRAINT FK_SecurityGroupSecurityRole_RoleId FOREIGN KEY (RoleId)
	REFERENCES Shield.SecurityRole (RoleId)

ALTER TABLE Shield.SecurityGroupUserAccount 
	WITH CHECK ADD CONSTRAINT FK_SecurityGroupUserAccount_BusinessEntityID FOREIGN KEY (BusinessEntityID)
	REFERENCES Person.Person (BusinessEntityID)

ALTER TABLE Shield.SecurityGroupUserAccount 
	WITH CHECK ADD CONSTRAINT FK_SecurityGroupUserAccount_GroupId FOREIGN KEY (GroupId)
	REFERENCES Shield.SecurityGroup (GroupId)
	
```
