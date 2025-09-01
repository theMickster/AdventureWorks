-- Migration: Add nonclustered index on SalesPersonId for StoreSalesPersonHistory
-- Story #887 — supports FK enforcement and salesperson-centric history queries

IF NOT EXISTS (SELECT 1 FROM sys.indexes
               WHERE name = 'IX_StoreSalesPersonHistory_SalesPersonId'
                 AND object_id = OBJECT_ID('[Sales].[StoreSalesPersonHistory]'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_StoreSalesPersonHistory_SalesPersonId]
        ON [Sales].[StoreSalesPersonHistory] ([SalesPersonId])
        INCLUDE ([BusinessEntityId], [StartDate], [EndDate])
END
GO
