IF OBJECT_ID(N'[dbo].[SalesOrderSagaInventoryAllocation]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[SalesOrderSagaInventoryAllocation]
    (
        [SagaInstanceId] nvarchar(128) NOT NULL,
        [SalesOrderId] int NOT NULL,
        [LineNumber] int NOT NULL,
        [ProductId] int NOT NULL,
        [LocationId] smallint NOT NULL,
        [Quantity] smallint NOT NULL,
        [ReservedAt] datetime2 NOT NULL,
        [ReversedAt] datetime2 NULL,
        CONSTRAINT [PK_SalesOrderSagaInventoryAllocation]
            PRIMARY KEY ([SagaInstanceId], [LineNumber], [ProductId], [LocationId])
    );
    CREATE INDEX [IX_SalesOrderSagaInventoryAllocation_SalesOrderId]
        ON [dbo].[SalesOrderSagaInventoryAllocation] ([SalesOrderId]);
END;
