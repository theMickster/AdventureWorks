IF OBJECT_ID('[HumanResources].[vEmployeePersonTemporalInfo]') IS NOT NULL
	DROP VIEW [HumanResources].[vEmployeePersonTemporalInfo];

GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_NULLS ON
GO

/*
	View that joins [Person].[Person_Temporal] [HumanResources].[Employee_Temporal]
	This view can be later used in temporal querying which is extremely flexible and convenient
	given that participating tables are temporal and can be changed independently
*/
CREATE VIEW [HumanResources].[vEmployeePersonTemporalInfo]
AS
SELECT P.BusinessEntityID, P.Title, P. FirstName, P.LastName, P.MiddleName
, E.JobTitle, E.MaritalStatus, E.Gender, E.VacationHours, E.SickLeaveHours
FROM [Person].Person_Temporal P
JOIN  [HumanResources].[Employee_Temporal] E
ON P.[BusinessEntityID] = E.[BusinessEntityID]

GO
