DROP VIEW IF EXISTS [HumanResources].[vEmployeeOrgChart];
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_NULLS ON
GO

CREATE VIEW [HumanResources].[vEmployeeOrgChart]
AS
WITH [EmployeeHierarchy] AS (
    SELECT
        e.[BusinessEntityID],
        e.[JobTitle],
        e.[OrganizationNode],
        COALESCE(e.[OrganizationLevel], CONVERT(smallint, 0)) AS [OrganizationLevel],
        CASE
            WHEN e.[OrganizationNode] IS NULL THEN NULL
            ELSE e.[OrganizationNode].GetAncestor(1)
        END AS [ManagerOrganizationNode]
    FROM [HumanResources].[Employee] e
)
SELECT
    e.[BusinessEntityID],
    p.[Title],
    p.[FirstName],
    p.[MiddleName],
    p.[LastName],
    p.[Suffix],
    e.[JobTitle],
    d.[DepartmentID],
    d.[Name] AS [DepartmentName],
    d.[GroupName],
    CASE
        WHEN e.[OrganizationNode] IS NULL THEN N'/'
        ELSE e.[OrganizationNode].ToString()
    END AS [OrganizationNode],
    e.[OrganizationLevel],
    managerLookup.[BusinessEntityID] AS [ManagerBusinessEntityID]
FROM [EmployeeHierarchy] e
INNER JOIN [Person].[Person] p
    ON p.[BusinessEntityID] = e.[BusinessEntityID]
LEFT JOIN [HumanResources].[EmployeeDepartmentHistory] edh
    ON edh.[BusinessEntityID] = e.[BusinessEntityID]
    AND edh.[EndDate] IS NULL
LEFT JOIN [HumanResources].[Department] d
    ON d.[DepartmentID] = edh.[DepartmentID]
OUTER APPLY (
    SELECT TOP (1)
        manager.[BusinessEntityID]
    FROM [HumanResources].[Employee] manager
    WHERE manager.[OrganizationNode] = e.[ManagerOrganizationNode]
        OR (
            e.[ManagerOrganizationNode] IS NULL
            AND e.[OrganizationLevel] = 1
            AND manager.[OrganizationNode] IS NULL
        )
    ORDER BY
        CASE
            WHEN manager.[OrganizationNode] = e.[ManagerOrganizationNode] THEN 0
            ELSE 1
        END,
        manager.[BusinessEntityID]
) managerLookup;
GO
EXEC sp_addextendedproperty
    N'MS_Description',
    N'Returns one row per employee with current department data and hierarchy metadata for org chart rendering.',
    'SCHEMA',
    N'HumanResources',
    'VIEW',
    N'vEmployeeOrgChart',
    NULL,
    NULL;
GO
