IF OBJECT_ID('[Person].[sp_DeletePerson_Temporal]') IS NOT NULL
	DROP PROCEDURE [Person].[sp_DeletePerson_Temporal];

GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_NULLS ON
GO

/*
	Stored procedure that deletes row in [Person].[Person_Temporal]
	and corresponding row in [HumanResources].[Employee_Temporal]
*/
CREATE PROCEDURE [Person].[sp_DeletePerson_Temporal]
@BusinessEntityID INT
AS

DELETE FROM [HumanResources].[Employee_Temporal] WHERE [BusinessEntityID] = @BusinessEntityID;
DELETE FROM [Person].[Person_Temporal] WHERE [BusinessEntityID] = @BusinessEntityID;

GO
