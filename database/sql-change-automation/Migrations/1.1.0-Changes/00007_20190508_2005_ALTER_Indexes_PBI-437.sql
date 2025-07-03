-- <Migration ID="608eae46-5315-452b-8008-9f6b213fdd67" />
GO
/****************************************************************************************************************
** CREATED BY:   Mick Letofsky
** CREATED DATE: 2019.05.08
** CREATED FOR:  PBI 437
** CREATED:      Baseline code for AdventureWorks SQL Change Automation Project.
****************************************************************************************************************/

DROP INDEX [ix_Employee_Temporal_History] ON [HumanResources].[Employee_Temporal_History]

CREATE CLUSTERED INDEX [ix_Employee_Temporal_History] ON [HumanResources].[Employee_Temporal_History] ([BusinessEntityID], [ValidFrom], [ValidTo]) WITH (DATA_COMPRESSION = PAGE)

ALTER TABLE [HumanResources].[Employee_Temporal_History] REBUILD PARTITION = ALL
WITH (DATA_COMPRESSION = PAGE)

DROP INDEX [ix_Person_Temporal_History] ON [Person].[Person_Temporal_History]

CREATE CLUSTERED INDEX [ix_Person_Temporal_History] ON [Person].[Person_Temporal_History] ([BusinessEntityID], [ValidFrom], [ValidTo]) WITH (DATA_COMPRESSION = PAGE)

ALTER TABLE [Person].[Person_Temporal_History] REBUILD PARTITION = ALL WITH (DATA_COMPRESSION = PAGE);

IF HAS_PERMS_BY_NAME(N'sys.xp_logevent', N'OBJECT', N'EXECUTE') = 1
BEGIN
    DECLARE @databaseName AS nvarchar(2048), @eventMessage AS nvarchar(2048)
    SET @databaseName = REPLACE(REPLACE(DB_NAME(), N'\', N'\\'), N'"', N'\"')
    SET @eventMessage = N'Redgate SQL Compare: { "deployment": { "description": "Redgate SQL Compare deployed to ' + @databaseName + N'", "database": "' + @databaseName + N'" }}'
    EXECUTE sys.xp_logevent 55000, @eventMessage
END

