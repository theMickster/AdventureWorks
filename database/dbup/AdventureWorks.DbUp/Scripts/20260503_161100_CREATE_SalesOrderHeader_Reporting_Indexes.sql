/*
 * Migration: Add SalesOrderHeader reporting indexes
 * Date: 2026-05-03
 * Author: GitHub Copilot
 * Issue: Azure DevOps #715
 *
 * Purpose:
 * Support the Sales reporting views and browser-grid workloads by adding
 * targeted nonclustered indexes for OrderDate and Status predicates on
 * Sales.SalesOrderHeader.
 *
 * Changes:
 * - Adds IX_SalesOrderHeader_OrderDate
 * - Adds IX_SalesOrderHeader_Status
 * - Adds IX_SalesOrderHeader_OrderDate_Status
 *
 * Rollback Strategy:
 * Create a compensating migration that drops the three indexes if the
 * reporting workload changes or the indexes need to be removed.
 */

IF NOT EXISTS
(
    SELECT 1
    FROM sys.indexes
    WHERE [name] = 'IX_SalesOrderHeader_OrderDate'
      AND [object_id] = OBJECT_ID('[Sales].[SalesOrderHeader]')
)
BEGIN
    CREATE NONCLUSTERED INDEX [IX_SalesOrderHeader_OrderDate]
        ON [Sales].[SalesOrderHeader] ([OrderDate])
        INCLUDE ([SalesOrderID], [CustomerID], [SalesPersonID], [TerritoryID], [Status], [TotalDue]);
END
GO

IF NOT EXISTS
(
    SELECT 1
    FROM sys.indexes
    WHERE [name] = 'IX_SalesOrderHeader_Status'
      AND [object_id] = OBJECT_ID('[Sales].[SalesOrderHeader]')
)
BEGIN
    CREATE NONCLUSTERED INDEX [IX_SalesOrderHeader_Status]
        ON [Sales].[SalesOrderHeader] ([Status])
        INCLUDE ([SalesOrderID], [OrderDate], [CustomerID], [SalesPersonID], [TerritoryID], [TotalDue]);
END
GO

IF NOT EXISTS
(
    SELECT 1
    FROM sys.indexes
    WHERE [name] = 'IX_SalesOrderHeader_OrderDate_Status'
      AND [object_id] = OBJECT_ID('[Sales].[SalesOrderHeader]')
)
BEGIN
    CREATE NONCLUSTERED INDEX [IX_SalesOrderHeader_OrderDate_Status]
        ON [Sales].[SalesOrderHeader] ([OrderDate], [Status])
        INCLUDE
        (
            [SalesOrderID],
            [SalesOrderNumber],
            [DueDate],
            [ShipDate],
            [OnlineOrderFlag],
            [CustomerID],
            [SalesPersonID],
            [TerritoryID],
            [SubTotal],
            [TaxAmt],
            [Freight],
            [TotalDue]
        );
END
GO
