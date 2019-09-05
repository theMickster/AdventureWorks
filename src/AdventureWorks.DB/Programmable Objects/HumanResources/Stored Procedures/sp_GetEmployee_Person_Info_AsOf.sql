IF OBJECT_ID('[HumanResources].[sp_GetEmployee_Person_Info_AsOf]') IS NOT NULL
	DROP PROCEDURE [HumanResources].[sp_GetEmployee_Person_Info_AsOf];

GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_NULLS ON
GO

/*
	Stored procedure used for querying Employee and Person data AS OF
	If @AsOf parameter is NULL, current data is queried
	otherwise both current and historical data is queried
*/
CREATE  PROCEDURE [HumanResources].[sp_GetEmployee_Person_Info_AsOf]
@asOf datetime2 = NULL
AS
IF @asOf IS NULL
	SELECT * FROM [HumanResources].[vEmployeePersonTemporalInfo]
ELSE
	SELECT * FROM [HumanResources].[vEmployeePersonTemporalInfo] FOR SYSTEM_TIME AS OF @asOf;
GO
