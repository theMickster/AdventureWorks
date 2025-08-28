IF OBJECT_ID('[Sales].[vSalesPersonPerformance]') IS NOT NULL
    DROP VIEW [Sales].[vSalesPersonPerformance];

GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_NULLS ON
GO

CREATE VIEW [Sales].[vSalesPersonPerformance]
AS
SELECT
    sp.[BusinessEntityID] AS [SalesPersonID]
    ,LTRIM(RTRIM(CONCAT(p.[FirstName], ' ', COALESCE(p.[MiddleName] + ' ', ''), p.[LastName]))) AS [SalesPersonName]
    ,e.[JobTitle]
    ,sp.[TerritoryID]
    ,st.[Name] AS [TerritoryName]
    ,st.[Group] AS [TerritoryGroup]
    ,sp.[SalesQuota]
    ,sp.[Bonus]
    ,sp.[CommissionPct]
    ,sp.[SalesYTD]
    ,sp.[SalesLastYear]
    ,COUNT(soh.[SalesOrderID]) AS [SalesOrderCount]
    ,COALESCE(SUM(soh.[SubTotal]), 0.00) AS [TotalSubTotal]
    ,COALESCE(SUM(soh.[TaxAmt]), 0.00) AS [TotalTaxAmt]
    ,COALESCE(SUM(soh.[Freight]), 0.00) AS [TotalFreight]
    ,COALESCE(SUM(soh.[TotalDue]), 0.00) AS [TotalTotalDue]
    ,MIN(soh.[OrderDate]) AS [FirstOrderDate]
    ,MAX(soh.[OrderDate]) AS [LastOrderDate]
FROM [Sales].[SalesPerson] sp
    INNER JOIN [Person].[Person] p
    ON p.[BusinessEntityID] = sp.[BusinessEntityID]
    INNER JOIN [HumanResources].[Employee] e
    ON e.[BusinessEntityID] = sp.[BusinessEntityID]
    LEFT OUTER JOIN [Sales].[SalesTerritory] st
    ON st.[TerritoryID] = sp.[TerritoryID]
    LEFT OUTER JOIN [Sales].[SalesOrderHeader] soh
    ON soh.[SalesPersonID] = sp.[BusinessEntityID]
GROUP BY
    sp.[BusinessEntityID]
    ,p.[FirstName]
    ,p.[MiddleName]
    ,p.[LastName]
    ,e.[JobTitle]
    ,sp.[TerritoryID]
    ,st.[Name]
    ,st.[Group]
    ,sp.[SalesQuota]
    ,sp.[Bonus]
    ,sp.[CommissionPct]
    ,sp.[SalesYTD]
    ,sp.[SalesLastYear];
GO
EXEC sp_addextendedproperty
    N'MS_Description',
    N'Aggregated sales order totals by salesperson with current territory context for reporting and dashboard scenarios.',
    'SCHEMA', N'Sales',
    'VIEW', N'vSalesPersonPerformance',
    NULL, NULL;
GO
