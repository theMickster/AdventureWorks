SET Nocount on; 
DECLARE @NewBusinessEntityId INTEGER = 0
		, @UserId UNIQUEIDENTIFIER
		, @ModifiedDate DATETIME = '2025-07-02 11:11:11'
		,@email varchar(50)
		,@fname varchar(50)
		,@lname varchar(50)
		,@title varchar(8)

SET @UserId = 'bf53a6ce-f7fc-40bf-91bb-45158644fc20'
set @email = 'michael.scott.AdventureWorks@mickletofsky.com'
set @fname = 'Michael'
set @lname = 'Scott'
set @title = 'Mr.'

INSERT INTO person.BusinessEntity(rowguid, ModifiedDate, IsEntraUser) VALUES (@UserId, @ModifiedDate, 1)
SET @NewBusinessEntityId = SCOPE_IDENTITY();

INSERT INTO Person.Person (BusinessEntityID, PersonTypeId, NameStyle, Title, FirstName,LastName, EmailPromotion, rowguid, ModifiedDate)
                   VALUES (@NewBusinessEntityId, 4, 0, @title, @fname, @lname , 0, @UserId, @ModifiedDate )

INSERT INTO Person.EmailAddress (BusinessEntityID,EmailAddress,rowguid,ModifiedDate)
     VALUES(@NewBusinessEntityId, @email, @UserId, @ModifiedDate)

SET @UserId = '4d29851a-8043-4c1a-8369-891ac9259c89'
set @email = 'jim.halpert.AdventureWorks@mickletofsky.com'
set @fname = 'Jim'
set @lname = 'Halpert'
set @title = 'Mr.'

INSERT INTO person.BusinessEntity(rowguid, ModifiedDate, IsEntraUser) VALUES (@UserId, @ModifiedDate, 1)
SET @NewBusinessEntityId = SCOPE_IDENTITY();

INSERT INTO Person.Person (BusinessEntityID, PersonTypeId, NameStyle, Title, FirstName,LastName, EmailPromotion, rowguid, ModifiedDate)
                   VALUES (@NewBusinessEntityId, 4, 0, @title, @fname, @lname , 0, @UserId, @ModifiedDate )

INSERT INTO Person.EmailAddress (BusinessEntityID,EmailAddress,rowguid,ModifiedDate)
     VALUES(@NewBusinessEntityId, @email, @UserId, @ModifiedDate)

SET @UserId = 'd5ce27c9-d9df-42a3-95dc-be099b873a7d'
set @email = 'dwight.schrute.AdventureWorks@mickletofsky.com'
set @fname = 'Dwight'
set @lname = 'Schrute'
set @title = 'Mr.'

INSERT INTO person.BusinessEntity(rowguid, ModifiedDate, IsEntraUser) VALUES (@UserId, @ModifiedDate, 1)
SET @NewBusinessEntityId = SCOPE_IDENTITY();

INSERT INTO Person.Person (BusinessEntityID, PersonTypeId, NameStyle, Title, FirstName,LastName, EmailPromotion, rowguid, ModifiedDate)
                   VALUES (@NewBusinessEntityId, 4, 0, @title, @fname, @lname , 0, @UserId, @ModifiedDate )

INSERT INTO Person.EmailAddress (BusinessEntityID,EmailAddress,rowguid,ModifiedDate)
     VALUES(@NewBusinessEntityId, @email, @UserId, @ModifiedDate)

SET @UserId = 'bb29b7c4-7e47-4df4-a3be-fb057c7622e0'
set @email = 'andy.bernard.AdventureWorks@mickletofsky.com'
set @fname = 'Andy'
set @lname = 'Bernard'
set @title = 'Mr.'

INSERT INTO person.BusinessEntity(rowguid, ModifiedDate, IsEntraUser) VALUES (@UserId, @ModifiedDate, 1)
SET @NewBusinessEntityId = SCOPE_IDENTITY();

INSERT INTO Person.Person (BusinessEntityID, PersonTypeId, NameStyle, Title, FirstName,LastName, EmailPromotion, rowguid, ModifiedDate)
                   VALUES (@NewBusinessEntityId, 4, 0, @title, @fname, @lname , 0, @UserId, @ModifiedDate )

INSERT INTO Person.EmailAddress (BusinessEntityID,EmailAddress,rowguid,ModifiedDate)
     VALUES(@NewBusinessEntityId, @email, @UserId, @ModifiedDate)

SET @UserId = '5ec8dd89-26ca-4266-89a3-c7578c43ab4b'
set @email = 'pam.halpert.AdventureWorks@mickletofsky.com'
set @fname = 'Pam'
set @lname = 'Halpert'
set @title = 'Mrs.'

INSERT INTO person.BusinessEntity(rowguid, ModifiedDate, IsEntraUser) VALUES (@UserId, @ModifiedDate, 1)
SET @NewBusinessEntityId = SCOPE_IDENTITY();

INSERT INTO Person.Person (BusinessEntityID, PersonTypeId, NameStyle, Title, FirstName,LastName, EmailPromotion, rowguid, ModifiedDate)
                   VALUES (@NewBusinessEntityId, 4, 0, @title, @fname, @lname , 0, @UserId, @ModifiedDate )

INSERT INTO Person.EmailAddress (BusinessEntityID,EmailAddress,rowguid,ModifiedDate)
     VALUES(@NewBusinessEntityId, @email, @UserId, @ModifiedDate)

SET @UserId = '04e808dc-d3b3-4eae-80a6-3aa695f0037e'
set @email = 'angela.schrute.AdventureWorks@mickletofsky.com'
set @fname = 'Angela'
set @lname = 'Schrute'
set @title = 'Mrs.'

INSERT INTO person.BusinessEntity(rowguid, ModifiedDate, IsEntraUser) VALUES (@UserId, @ModifiedDate, 1)
SET @NewBusinessEntityId = SCOPE_IDENTITY();

INSERT INTO Person.Person (BusinessEntityID, PersonTypeId, NameStyle, Title, FirstName,LastName, EmailPromotion, rowguid, ModifiedDate)
                   VALUES (@NewBusinessEntityId, 4, 0, @title, @fname, @lname , 0, @UserId, @ModifiedDate )

INSERT INTO Person.EmailAddress (BusinessEntityID,EmailAddress,rowguid,ModifiedDate)
     VALUES(@NewBusinessEntityId, @email, @UserId, @ModifiedDate)

SET @UserId = 'c9c68ef5-1ce7-4ebb-b973-a3b155b651ae'
set @email = 'holly.flax.AdventureWorks@mickletofsky.com'
set @fname = 'Holly'
set @lname = 'Flax'
set @title = 'Mrs.'

INSERT INTO person.BusinessEntity(rowguid, ModifiedDate, IsEntraUser) VALUES (@UserId, @ModifiedDate, 1)
SET @NewBusinessEntityId = SCOPE_IDENTITY();

INSERT INTO Person.Person (BusinessEntityID, PersonTypeId, NameStyle, Title, FirstName,LastName, EmailPromotion, rowguid, ModifiedDate)
                   VALUES (@NewBusinessEntityId, 4, 0, @title, @fname, @lname , 0, @UserId, @ModifiedDate )

INSERT INTO Person.EmailAddress (BusinessEntityID,EmailAddress,rowguid,ModifiedDate)
     VALUES(@NewBusinessEntityId, @email, @UserId, @ModifiedDate)
