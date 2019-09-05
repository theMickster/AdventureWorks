IF OBJECT_ID('[Person].[sp_UpdatePerson_Temporal]') IS NOT NULL
	DROP PROCEDURE [Person].[sp_UpdatePerson_Temporal];

GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_NULLS ON
GO

/*
	Stored procedure for updating columns of Person_Temporal
	If all parameters except @BusinessEntityID are NULL no update is performed 
	For NON NULL columns NULL values are ignored (i.e. existing values is applied)
*/
CREATE PROCEDURE [Person].[sp_UpdatePerson_Temporal]
@BusinessEntityID INT,
@PersonType nchar(2) = NULL,
@Title nvarchar(8) = NULL,
@FirstName nvarchar(50) = NULL,
@MiddleName nvarchar(50) = NULL,
@LastName nvarchar(50) = NULL,
@Suffix nvarchar(10) = NULL,
@EmailPromotion smallint = NULL

AS

IF @PersonType IS NOT NULL OR @Title IS NOT NULL OR @FirstName IS NOT NULL OR @MiddleName IS NOT NULL
OR @LastName IS NOT NULL OR @Suffix IS NOT NULL OR @EmailPromotion IS NOT NULL 

	UPDATE Person.Person_Temporal
	SET PersonType = ISNULL (@PersonType, PersonType),
	Title = @Title,
	FirstName = ISNULL (@FirstName, FirstName),
	MiddleName = ISNULL (@MiddleName, MiddleName),
	LastName = ISNULL (@LastName, LastName),
	Suffix = @Suffix,
	EmailPromotion = ISNULL(@EmailPromotion, EmailPromotion)
	WHERE BusinessEntityID = @BusinessEntityID;
	
GO
