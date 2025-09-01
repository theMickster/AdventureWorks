-- Migration: Create Sales.StoreSalesPersonHistory
-- Story #887 — Sales Person Assignment Tracking foundation
-- Unblocks: #888 (Reassign Sales Person), #889 (Get History)

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'StoreSalesPersonHistory' AND schema_id = SCHEMA_ID('Sales'))
BEGIN
    CREATE TABLE [Sales].[StoreSalesPersonHistory]
    (
        [BusinessEntityId] INT              NOT NULL,
        [SalesPersonId]    INT              NOT NULL,
        [StartDate]        DATETIME2        NOT NULL,
        [EndDate]          DATETIME2            NULL,
        [ModifiedDate]     DATETIME2        NOT NULL,
        [Rowguid]          UNIQUEIDENTIFIER NOT NULL CONSTRAINT [df_store_sales_person_history_rowguid] DEFAULT NEWID(),
        CONSTRAINT [pk_store_sales_person_history] PRIMARY KEY CLUSTERED ([BusinessEntityId], [SalesPersonId], [StartDate])
    )
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys
               WHERE name = 'FK_StoreSalesPersonHistory_BusinessEntityId'
                 AND parent_object_id = OBJECT_ID('[Sales].[StoreSalesPersonHistory]'))
BEGIN
    ALTER TABLE [Sales].[StoreSalesPersonHistory]
        ADD CONSTRAINT [FK_StoreSalesPersonHistory_BusinessEntityId]
        FOREIGN KEY ([BusinessEntityId])
        REFERENCES [Sales].[Store] ([BusinessEntityId])
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys
               WHERE name = 'FK_StoreSalesPersonHistory_SalesPersonId'
                 AND parent_object_id = OBJECT_ID('[Sales].[StoreSalesPersonHistory]'))
BEGIN
    ALTER TABLE [Sales].[StoreSalesPersonHistory]
        ADD CONSTRAINT [FK_StoreSalesPersonHistory_SalesPersonId]
        FOREIGN KEY ([SalesPersonId])
        REFERENCES [Sales].[SalesPerson] ([BusinessEntityId])
END
GO
