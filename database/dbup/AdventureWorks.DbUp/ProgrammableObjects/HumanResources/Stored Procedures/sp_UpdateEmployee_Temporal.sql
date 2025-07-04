IF OBJECT_ID('[HumanResources].[sp_UpdateEmployee_Temporal]') IS NOT NULL
	DROP PROCEDURE [HumanResources].[sp_UpdateEmployee_Temporal];

GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_NULLS ON
GO

/*
	Stored procedure for updating columns of [HumanResources].[Employee_Temporal]
	If all parameters except @BusinessEntityID are NULL no update is performed 
	For NON NULL columns NULL values are ignored (i.e. existing values is applied)
*/
CREATE PROCEDURE [HumanResources].[sp_UpdateEmployee_Temporal]
 @BusinessEntityID INT
,@LoginID nvarchar(256) = NULL   
,@JobTitle nvarchar(50) = NULL
,@MaritalStatus nchar(1) = NULL
,@Gender nchar(1) = NULL
,@VacationHours smallint = 0
,@SickLeaveHours smallint = 0

AS
IF @LoginID IS NOT NULL OR @JobTitle IS NOT NULL OR @MaritalStatus IS NOT NULL 
OR @Gender IS NOT NULL OR @VacationHours IS NOT NULL OR @SickLeaveHours IS NOT NULL 

	UPDATE [HumanResources].[Employee_Temporal]
	SET  [LoginID] = ISNULL (@LoginID, LoginID),
	JobTitle = ISNULL (@JobTitle, JobTitle),
	MaritalStatus = ISNULL (@MaritalStatus, MaritalStatus),
	Gender = ISNULL (@Gender, Gender),
	VacationHours = ISNULL (@VacationHours, VacationHours),
	SickLeaveHours = ISNULL (@SickLeaveHours, SickLeaveHours)	
	WHERE BusinessEntityID = @BusinessEntityID;	
	
GO
