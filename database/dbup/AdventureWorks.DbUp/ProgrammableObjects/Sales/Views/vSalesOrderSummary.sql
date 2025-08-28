IF OBJECT_ID('[Sales].[vSalesOrderSummary]') IS NOT NULL
    DROP VIEW [Sales].[vSalesOrderSummary];

GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_NULLS ON
GO

CREATE VIEW [Sales].[vSalesOrderSummary]
AS
WITH [LineItemCounts] AS
(
    SELECT
        sod.[SalesOrderID]
        ,COUNT(*) AS [LineItemCount]
    FROM [Sales].[SalesOrderDetail] sod
    GROUP BY sod.[SalesOrderID]
)
SELECT
    soh.[SalesOrderID]
    ,soh.[RevisionNumber]
    ,soh.[OrderDate]
    ,soh.[DueDate]
    ,soh.[ShipDate]
    ,soh.[Status]
    ,soh.[OnlineOrderFlag]
    ,soh.[SalesOrderNumber]
    ,soh.[PurchaseOrderNumber]
    ,soh.[AccountNumber]
    ,soh.[CustomerID]
    ,CASE
        WHEN c.[PersonID] IS NOT NULL THEN LTRIM(RTRIM(CONCAT(cp.[FirstName], ' ', COALESCE(cp.[MiddleName] + ' ', ''), cp.[LastName])))
        WHEN c.[StoreID] IS NOT NULL THEN storeCustomer.[Name]
        ELSE NULL
     END AS [CustomerName]
    ,soh.[SalesPersonID]
    ,CASE
        WHEN sp.[BusinessEntityID] IS NOT NULL THEN LTRIM(RTRIM(CONCAT(sp.[FirstName], ' ', COALESCE(sp.[MiddleName] + ' ', ''), sp.[LastName])))
        ELSE NULL
     END AS [SalesPersonName]
    ,soh.[TerritoryID]
    ,st.[Name] AS [TerritoryName]
    ,COALESCE(lic.[LineItemCount], 0) AS [LineItemCount]
    ,soh.[SubTotal]
    ,soh.[TaxAmt]
    ,soh.[Freight]
    ,soh.[TotalDue]
FROM [Sales].[SalesOrderHeader] soh
    INNER JOIN [Sales].[Customer] c
    ON c.[CustomerID] = soh.[CustomerID]
    LEFT OUTER JOIN [Person].[Person] cp
    ON cp.[BusinessEntityID] = c.[PersonID]
    LEFT OUTER JOIN [Sales].[Store] storeCustomer
    ON storeCustomer.[BusinessEntityID] = c.[StoreID]
    LEFT OUTER JOIN [Person].[Person] sp
    ON sp.[BusinessEntityID] = soh.[SalesPersonID]
    LEFT OUTER JOIN [Sales].[SalesTerritory] st
    ON st.[TerritoryID] = soh.[TerritoryID]
    LEFT OUTER JOIN [LineItemCounts] lic
    ON lic.[SalesOrderID] = soh.[SalesOrderID];
GO
EXEC sp_addextendedproperty
    N'MS_Description',
    N'One row per sales order with denormalized customer, salesperson, territory, and line-item summary fields for reporting and browser grids.',
    'SCHEMA', N'Sales',
    'VIEW', N'vSalesOrderSummary',
    NULL, NULL;
GO
