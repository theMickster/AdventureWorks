-- Creates dbo.ActivityLog table for real-time event audit trail (Feature 773 / US-780).
IF NOT EXISTS (
    SELECT * FROM sys.tables
    WHERE name = 'ActivityLog' AND schema_id = SCHEMA_ID('dbo')
)
BEGIN
    CREATE TABLE [dbo].[ActivityLog]
    (
        [ActivityLogId] INT IDENTITY(1,1) NOT NULL,
        [EntityType]    NVARCHAR(100)      NOT NULL,
        [EntityId]      INT                NOT NULL,
        [Action]        NVARCHAR(50)       NOT NULL,
        [UserName]      NVARCHAR(256)      NOT NULL,
        [Timestamp]     DATETIME2          NOT NULL,

        CONSTRAINT [PK_ActivityLog_ActivityLogId] PRIMARY KEY CLUSTERED ([ActivityLogId] ASC)
    )
END
GO

IF NOT EXISTS (
    SELECT * FROM sys.indexes
    WHERE name = 'IX_ActivityLog_Timestamp'
      AND object_id = OBJECT_ID('[dbo].[ActivityLog]')
)
BEGIN
    CREATE NONCLUSTERED INDEX [IX_ActivityLog_Timestamp]
    ON [dbo].[ActivityLog]([Timestamp] DESC)
END
GO

IF NOT EXISTS (
    SELECT * FROM sys.indexes
    WHERE name = 'IX_ActivityLog_EntityType_EntityId'
      AND object_id = OBJECT_ID('[dbo].[ActivityLog]')
)
BEGIN
    CREATE NONCLUSTERED INDEX [IX_ActivityLog_EntityType_EntityId]
    ON [dbo].[ActivityLog]([EntityType] ASC, [EntityId] ASC)
END
GO
