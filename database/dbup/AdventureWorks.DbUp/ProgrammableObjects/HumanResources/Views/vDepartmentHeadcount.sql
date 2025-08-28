DROP VIEW IF EXISTS [HumanResources].[vDepartmentHeadcount];
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_NULLS ON
GO

CREATE VIEW [HumanResources].[vDepartmentHeadcount]
AS
SELECT
    d.[DepartmentID],
    d.[Name] AS [DepartmentName],
    d.[GroupName],
    COALESCE(currentAssignments.[CurrentHeadcount], 0) AS [CurrentHeadcount]
FROM [HumanResources].[Department] d
LEFT JOIN (
    SELECT
        edh.[DepartmentID],
        COUNT(*) AS [CurrentHeadcount]
    FROM [HumanResources].[EmployeeDepartmentHistory] edh
    WHERE edh.[EndDate] IS NULL
    GROUP BY
        edh.[DepartmentID]
) currentAssignments
    ON d.[DepartmentID] = currentAssignments.[DepartmentID];
GO
EXEC sp_addextendedproperty
    N'MS_Description',
    N'Returns one row per department with the current headcount based on active EmployeeDepartmentHistory assignments.',
    'SCHEMA',
    N'HumanResources',
    'VIEW',
    N'vDepartmentHeadcount',
    NULL,
    NULL;
GO
