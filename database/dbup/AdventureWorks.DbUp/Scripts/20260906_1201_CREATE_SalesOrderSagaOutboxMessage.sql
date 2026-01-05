IF OBJECT_ID(N'[dbo].[SalesOrderSagaOutboxMessage]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[SalesOrderSagaOutboxMessage]
    (
        [MessageId] nvarchar(180) NOT NULL,
        [EventName] nvarchar(80) NOT NULL,
        [Payload] nvarchar(max) NOT NULL,
        [OccurredAt] datetime2 NOT NULL,
        [DispatchedAt] datetime2 NULL,
        [DispatchAttemptCount] int NOT NULL CONSTRAINT [DF_SalesOrderSagaOutboxMessage_DispatchAttemptCount] DEFAULT 0,
        [LastDispatchError] nvarchar(2048) NULL,
        CONSTRAINT [PK_SalesOrderSagaOutboxMessage] PRIMARY KEY ([MessageId])
    );
    CREATE INDEX [IX_SalesOrderSagaOutboxMessage_DispatchedAt]
        ON [dbo].[SalesOrderSagaOutboxMessage] ([DispatchedAt]);
END;
