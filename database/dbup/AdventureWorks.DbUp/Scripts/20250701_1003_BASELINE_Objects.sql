-- <Migration ID="5f37846f-fd84-438b-a83f-5db0c85d12b5" TransactionHandling="Custom" />
GO
/****************************************************************************************************************
** CREATED BY:   Mick Letofsky
** CREATED DATE: 2019.05.08
** CREATED FOR:  PBI 437
** CREATED:      Baseline code for AdventureWorks SQL Change Automation Project.
****************************************************************************************************************/			

PRINT N'Creating types'
GO
IF TYPE_ID(N'[dbo].[Phone]') IS NULL
CREATE TYPE [dbo].[Phone] FROM nvarchar (25) NULL
GO
IF TYPE_ID(N'[dbo].[OrderNumber]') IS NULL
CREATE TYPE [dbo].[OrderNumber] FROM nvarchar (25) NULL
GO
IF TYPE_ID(N'[dbo].[Name]') IS NULL
CREATE TYPE [dbo].[Name] FROM nvarchar (50) NULL
GO
IF TYPE_ID(N'[dbo].[NameStyle]') IS NULL
CREATE TYPE [dbo].[NameStyle] FROM bit NOT NULL
GO
IF TYPE_ID(N'[dbo].[AccountNumber]') IS NULL
CREATE TYPE [dbo].[AccountNumber] FROM nvarchar (15) NULL
GO
IF TYPE_ID(N'[Sales].[SalesOrderDetailType_inmem]') IS NULL
CREATE TYPE [Sales].[SalesOrderDetailType_inmem] AS TABLE
(
[OrderQty] [smallint] NOT NULL,
[ProductID] [int] NOT NULL,
[SpecialOfferID] [int] NOT NULL,
INDEX [IX_ProductID] NONCLUSTERED HASH ([ProductID]) WITH (BUCKET_COUNT=8),
INDEX [IX_SpecialOfferID] NONCLUSTERED HASH ([SpecialOfferID]) WITH (BUCKET_COUNT=8)
)
WITH
(
MEMORY_OPTIMIZED = ON
)
GO
IF TYPE_ID(N'[dbo].[Flag]') IS NULL
CREATE TYPE [dbo].[Flag] FROM bit NOT NULL
GO
IF TYPE_ID(N'[Sales].[SalesOrderDetailType_ondisk]') IS NULL
CREATE TYPE [Sales].[SalesOrderDetailType_ondisk] AS TABLE
(
[OrderQty] [smallint] NOT NULL,
[ProductID] [int] NOT NULL,
[SpecialOfferID] [int] NOT NULL,
INDEX [IX_ProductID] CLUSTERED ([ProductID]),
INDEX [IX_SpecialOfferID] NONCLUSTERED ([SpecialOfferID])
)
GO
PRINT N'Creating [HumanResources].[Employee]'
GO
IF OBJECT_ID(N'[HumanResources].[Employee]', 'U') IS NULL
CREATE TABLE [HumanResources].[Employee]
(
[BusinessEntityID] [int] NOT NULL,
[NationalIDNumber] [nvarchar] (15) NOT NULL,
[LoginID] [nvarchar] (256) NOT NULL,
[OrganizationNode] [sys].[hierarchyid] NULL,
[OrganizationLevel] AS ([OrganizationNode].[GetLevel]()),
[JobTitle] [nvarchar] (50) NOT NULL,
[BirthDate] [date] NOT NULL,
[MaritalStatus] [nchar] (1) NOT NULL,
[Gender] [nchar] (1) NOT NULL,
[HireDate] [date] NOT NULL,
[SalariedFlag] [dbo].[Flag] NOT NULL CONSTRAINT [DF_Employee_SalariedFlag] DEFAULT ((1)),
[VacationHours] [smallint] NOT NULL CONSTRAINT [DF_Employee_VacationHours] DEFAULT ((0)),
[SickLeaveHours] [smallint] NOT NULL CONSTRAINT [DF_Employee_SickLeaveHours] DEFAULT ((0)),
[CurrentFlag] [dbo].[Flag] NOT NULL CONSTRAINT [DF_Employee_CurrentFlag] DEFAULT ((1)),
[rowguid] [uniqueidentifier] NOT NULL ROWGUIDCOL CONSTRAINT [DF_Employee_rowguid] DEFAULT (newid()),
[ModifiedDate] [datetime] NOT NULL CONSTRAINT [DF_Employee_ModifiedDate] DEFAULT (getdate())
)
GO
PRINT N'Creating primary key [PK_Employee_BusinessEntityID] on [HumanResources].[Employee]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[HumanResources].[PK_Employee_BusinessEntityID]', 'PK') AND parent_object_id = OBJECT_ID(N'[HumanResources].[Employee]', 'U'))
ALTER TABLE [HumanResources].[Employee] ADD CONSTRAINT [PK_Employee_BusinessEntityID] PRIMARY KEY CLUSTERED  ([BusinessEntityID])
GO
PRINT N'Creating index [AK_Employee_LoginID] on [HumanResources].[Employee]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'AK_Employee_LoginID' AND object_id = OBJECT_ID(N'[HumanResources].[Employee]'))
CREATE UNIQUE NONCLUSTERED INDEX [AK_Employee_LoginID] ON [HumanResources].[Employee] ([LoginID])
GO
PRINT N'Creating index [AK_Employee_NationalIDNumber] on [HumanResources].[Employee]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'AK_Employee_NationalIDNumber' AND object_id = OBJECT_ID(N'[HumanResources].[Employee]'))
CREATE UNIQUE NONCLUSTERED INDEX [AK_Employee_NationalIDNumber] ON [HumanResources].[Employee] ([NationalIDNumber])
GO
PRINT N'Creating index [IX_Employee_OrganizationLevel_OrganizationNode] on [HumanResources].[Employee]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Employee_OrganizationLevel_OrganizationNode' AND object_id = OBJECT_ID(N'[HumanResources].[Employee]'))
CREATE NONCLUSTERED INDEX [IX_Employee_OrganizationLevel_OrganizationNode] ON [HumanResources].[Employee] ([OrganizationLevel], [OrganizationNode])
GO
PRINT N'Creating index [IX_Employee_OrganizationNode] on [HumanResources].[Employee]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Employee_OrganizationNode' AND object_id = OBJECT_ID(N'[HumanResources].[Employee]'))
CREATE NONCLUSTERED INDEX [IX_Employee_OrganizationNode] ON [HumanResources].[Employee] ([OrganizationNode])
GO
PRINT N'Creating index [AK_Employee_rowguid] on [HumanResources].[Employee]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'AK_Employee_rowguid' AND object_id = OBJECT_ID(N'[HumanResources].[Employee]'))
CREATE UNIQUE NONCLUSTERED INDEX [AK_Employee_rowguid] ON [HumanResources].[Employee] ([rowguid])
GO
PRINT N'Creating trigger [HumanResources].[dEmployee] on [HumanResources].[Employee]'
GO
IF OBJECT_ID(N'[HumanResources].[dEmployee]', 'TR') IS NULL
EXEC sp_executesql N'
CREATE TRIGGER [HumanResources].[dEmployee] ON [HumanResources].[Employee] 
INSTEAD OF DELETE NOT FOR REPLICATION AS 
BEGIN
    DECLARE @Count int;

    SET @Count = @@ROWCOUNT;
    IF @Count = 0 
        RETURN;

    SET NOCOUNT ON;

    BEGIN
        RAISERROR
            (N''Employees cannot be deleted. They can only be marked as not current.'', -- Message
            10, -- Severity.
            1); -- State.

        -- Rollback any active or uncommittable transactions
        IF @@TRANCOUNT > 0
        BEGIN
            ROLLBACK TRANSACTION;
        END
    END;
END;
'
GO
PRINT N'Creating [Person].[Person]'
GO
IF OBJECT_ID(N'[Person].[Person]', 'U') IS NULL
CREATE TABLE [Person].[Person]
(
[BusinessEntityID] [int] NOT NULL,
[PersonType] [nchar] (2) NOT NULL,
[NameStyle] [dbo].[NameStyle] NOT NULL CONSTRAINT [DF_Person_NameStyle] DEFAULT ((0)),
[Title] [nvarchar] (8) NULL,
[FirstName] [dbo].[Name] NOT NULL,
[MiddleName] [dbo].[Name] NULL,
[LastName] [dbo].[Name] NOT NULL,
[Suffix] [nvarchar] (10) NULL,
[EmailPromotion] [int] NOT NULL CONSTRAINT [DF_Person_EmailPromotion] DEFAULT ((0)),
[AdditionalContactInfo] [xml] (CONTENT [Person].[AdditionalContactInfoSchemaCollection]) NULL,
[Demographics] [xml] (CONTENT [Person].[IndividualSurveySchemaCollection]) NULL,
[rowguid] [uniqueidentifier] NOT NULL ROWGUIDCOL CONSTRAINT [DF_Person_rowguid] DEFAULT (newid()),
[ModifiedDate] [datetime] NOT NULL CONSTRAINT [DF_Person_ModifiedDate] DEFAULT (getdate())
)
GO
PRINT N'Creating primary key [PK_Person_BusinessEntityID] on [Person].[Person]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[Person].[PK_Person_BusinessEntityID]', 'PK') AND parent_object_id = OBJECT_ID(N'[Person].[Person]', 'U'))
ALTER TABLE [Person].[Person] ADD CONSTRAINT [PK_Person_BusinessEntityID] PRIMARY KEY CLUSTERED  ([BusinessEntityID])
GO
PRINT N'Creating index [IX_Person_LastName_FirstName_MiddleName] on [Person].[Person]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Person_LastName_FirstName_MiddleName' AND object_id = OBJECT_ID(N'[Person].[Person]'))
CREATE NONCLUSTERED INDEX [IX_Person_LastName_FirstName_MiddleName] ON [Person].[Person] ([LastName], [FirstName], [MiddleName])
GO
PRINT N'Creating index [AK_Person_rowguid] on [Person].[Person]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'AK_Person_rowguid' AND object_id = OBJECT_ID(N'[Person].[Person]'))
CREATE UNIQUE NONCLUSTERED INDEX [AK_Person_rowguid] ON [Person].[Person] ([rowguid])
GO
PRINT N'Creating index [PXML_Person_AddContact] on [Person].[Person]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'PXML_Person_AddContact' AND object_id = OBJECT_ID(N'[Person].[Person]'))
CREATE PRIMARY XML INDEX [PXML_Person_AddContact]
ON [Person].[Person] ([AdditionalContactInfo])
GO
PRINT N'Creating index [PXML_Person_Demographics] on [Person].[Person]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'PXML_Person_Demographics' AND object_id = OBJECT_ID(N'[Person].[Person]'))
CREATE PRIMARY XML INDEX [PXML_Person_Demographics]
ON [Person].[Person] ([Demographics])
GO
PRINT N'Creating index [XMLPATH_Person_Demographics] on [Person].[Person]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'XMLPATH_Person_Demographics' AND object_id = OBJECT_ID(N'[Person].[Person]'))
CREATE XML INDEX [XMLPATH_Person_Demographics]
ON [Person].[Person] ([Demographics])
USING XML INDEX [PXML_Person_Demographics]
FOR PATH
GO
PRINT N'Creating index [XMLPROPERTY_Person_Demographics] on [Person].[Person]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'XMLPROPERTY_Person_Demographics' AND object_id = OBJECT_ID(N'[Person].[Person]'))
CREATE XML INDEX [XMLPROPERTY_Person_Demographics]
ON [Person].[Person] ([Demographics])
USING XML INDEX [PXML_Person_Demographics]
FOR PROPERTY
GO
PRINT N'Creating index [XMLVALUE_Person_Demographics] on [Person].[Person]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'XMLVALUE_Person_Demographics' AND object_id = OBJECT_ID(N'[Person].[Person]'))
CREATE XML INDEX [XMLVALUE_Person_Demographics]
ON [Person].[Person] ([Demographics])
USING XML INDEX [PXML_Person_Demographics]
FOR VALUE
GO
PRINT N'Creating trigger [Person].[iuPerson] on [Person].[Person]'
GO
IF OBJECT_ID(N'[Person].[iuPerson]', 'TR') IS NULL
EXEC sp_executesql N'
CREATE TRIGGER [Person].[iuPerson] ON [Person].[Person] 
AFTER INSERT, UPDATE NOT FOR REPLICATION AS 
BEGIN
    DECLARE @Count int;

    SET @Count = @@ROWCOUNT;
    IF @Count = 0 
        RETURN;

    SET NOCOUNT ON;

    IF UPDATE([BusinessEntityID]) OR UPDATE([Demographics]) 
    BEGIN
        UPDATE [Person].[Person] 
        SET [Person].[Person].[Demographics] = N''<IndividualSurvey xmlns="http://schemas.microsoft.com/sqlserver/2004/07/adventure-works/IndividualSurvey"> 
            <TotalPurchaseYTD>0.00</TotalPurchaseYTD> 
            </IndividualSurvey>'' 
        FROM inserted 
        WHERE [Person].[Person].[BusinessEntityID] = inserted.[BusinessEntityID] 
            AND inserted.[Demographics] IS NULL;
        
        UPDATE [Person].[Person] 
        SET [Demographics].modify(N''declare default element namespace "http://schemas.microsoft.com/sqlserver/2004/07/adventure-works/IndividualSurvey"; 
            insert <TotalPurchaseYTD>0.00</TotalPurchaseYTD> 
            as first 
            into (/IndividualSurvey)[1]'') 
        FROM inserted 
        WHERE [Person].[Person].[BusinessEntityID] = inserted.[BusinessEntityID] 
            AND inserted.[Demographics] IS NOT NULL 
            AND inserted.[Demographics].exist(N''declare default element namespace 
                "http://schemas.microsoft.com/sqlserver/2004/07/adventure-works/IndividualSurvey"; 
                /IndividualSurvey/TotalPurchaseYTD'') <> 1;
    END;
END;
'
GO
PRINT N'Creating [Purchasing].[PurchaseOrderDetail]'
GO
IF OBJECT_ID(N'[Purchasing].[PurchaseOrderDetail]', 'U') IS NULL
CREATE TABLE [Purchasing].[PurchaseOrderDetail]
(
[PurchaseOrderID] [int] NOT NULL,
[PurchaseOrderDetailID] [int] NOT NULL IDENTITY(1, 1),
[DueDate] [datetime] NOT NULL,
[OrderQty] [smallint] NOT NULL,
[ProductID] [int] NOT NULL,
[UnitPrice] [money] NOT NULL,
[LineTotal] AS (isnull([OrderQty]*[UnitPrice],(0.00))),
[ReceivedQty] [decimal] (8, 2) NOT NULL,
[RejectedQty] [decimal] (8, 2) NOT NULL,
[StockedQty] AS (isnull([ReceivedQty]-[RejectedQty],(0.00))),
[ModifiedDate] [datetime] NOT NULL CONSTRAINT [DF_PurchaseOrderDetail_ModifiedDate] DEFAULT (getdate())
)
GO
PRINT N'Creating primary key [PK_PurchaseOrderDetail_PurchaseOrderID_PurchaseOrderDetailID] on [Purchasing].[PurchaseOrderDetail]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[Purchasing].[PK_PurchaseOrderDetail_PurchaseOrderID_PurchaseOrderDetailID]', 'PK') AND parent_object_id = OBJECT_ID(N'[Purchasing].[PurchaseOrderDetail]', 'U'))
ALTER TABLE [Purchasing].[PurchaseOrderDetail] ADD CONSTRAINT [PK_PurchaseOrderDetail_PurchaseOrderID_PurchaseOrderDetailID] PRIMARY KEY CLUSTERED  ([PurchaseOrderID], [PurchaseOrderDetailID])
GO
PRINT N'Creating index [IX_PurchaseOrderDetail_ProductID] on [Purchasing].[PurchaseOrderDetail]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_PurchaseOrderDetail_ProductID' AND object_id = OBJECT_ID(N'[Purchasing].[PurchaseOrderDetail]'))
CREATE NONCLUSTERED INDEX [IX_PurchaseOrderDetail_ProductID] ON [Purchasing].[PurchaseOrderDetail] ([ProductID])
GO
PRINT N'Creating [dbo].[uspPrintError]'
GO
IF OBJECT_ID(N'[dbo].[uspPrintError]', 'P') IS NULL
EXEC sp_executesql N'
-- uspPrintError prints error information about the error that caused 
-- execution to jump to the CATCH block of a TRY...CATCH construct. 
-- Should be executed from within the scope of a CATCH block otherwise 
-- it will return without printing any error information.
CREATE PROCEDURE [dbo].[uspPrintError] 
AS
BEGIN
    SET NOCOUNT ON;

    -- Print error information. 
    PRINT ''Error '' + CONVERT(varchar(50), ERROR_NUMBER()) +
          '', Severity '' + CONVERT(varchar(5), ERROR_SEVERITY()) +
          '', State '' + CONVERT(varchar(5), ERROR_STATE()) + 
          '', Procedure '' + ISNULL(ERROR_PROCEDURE(), ''-'') + 
          '', Line '' + CONVERT(varchar(5), ERROR_LINE());
    PRINT ERROR_MESSAGE();
END;
'
GO
PRINT N'Creating [dbo].[ErrorLog]'
GO
IF OBJECT_ID(N'[dbo].[ErrorLog]', 'U') IS NULL
CREATE TABLE [dbo].[ErrorLog]
(
[ErrorLogID] [int] NOT NULL IDENTITY(1, 1),
[ErrorTime] [datetime] NOT NULL CONSTRAINT [DF_ErrorLog_ErrorTime] DEFAULT (getdate()),
[UserName] [sys].[sysname] NOT NULL,
[ErrorNumber] [int] NOT NULL,
[ErrorSeverity] [int] NULL,
[ErrorState] [int] NULL,
[ErrorProcedure] [nvarchar] (126) NULL,
[ErrorLine] [int] NULL,
[ErrorMessage] [nvarchar] (4000) NOT NULL
)
GO
PRINT N'Creating primary key [PK_ErrorLog_ErrorLogID] on [dbo].[ErrorLog]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[PK_ErrorLog_ErrorLogID]', 'PK') AND parent_object_id = OBJECT_ID(N'[dbo].[ErrorLog]', 'U'))
ALTER TABLE [dbo].[ErrorLog] ADD CONSTRAINT [PK_ErrorLog_ErrorLogID] PRIMARY KEY CLUSTERED  ([ErrorLogID])
GO
PRINT N'Creating [dbo].[uspLogError]'
GO
IF OBJECT_ID(N'[dbo].[uspLogError]', 'P') IS NULL
EXEC sp_executesql N'
-- uspLogError logs error information in the ErrorLog table about the 
-- error that caused execution to jump to the CATCH block of a 
-- TRY...CATCH construct. This should be executed from within the scope 
-- of a CATCH block otherwise it will return without inserting error 
-- information. 
CREATE PROCEDURE [dbo].[uspLogError] 
    @ErrorLogID [int] = 0 OUTPUT -- contains the ErrorLogID of the row inserted
AS                               -- by uspLogError in the ErrorLog table
BEGIN
    SET NOCOUNT ON;

    -- Output parameter value of 0 indicates that error 
    -- information was not logged
    SET @ErrorLogID = 0;

    BEGIN TRY
        -- Return if there is no error information to log
        IF ERROR_NUMBER() IS NULL
            RETURN;

        -- Return if inside an uncommittable transaction.
        -- Data insertion/modification is not allowed when 
        -- a transaction is in an uncommittable state.
        IF XACT_STATE() = -1
        BEGIN
            PRINT ''Cannot log error since the current transaction is in an uncommittable state. '' 
                + ''Rollback the transaction before executing uspLogError in order to successfully log error information.'';
            RETURN;
        END

        INSERT [dbo].[ErrorLog] 
            (
            [UserName], 
            [ErrorNumber], 
            [ErrorSeverity], 
            [ErrorState], 
            [ErrorProcedure], 
            [ErrorLine], 
            [ErrorMessage]
            ) 
        VALUES 
            (
            CONVERT(sysname, CURRENT_USER), 
            ERROR_NUMBER(),
            ERROR_SEVERITY(),
            ERROR_STATE(),
            ERROR_PROCEDURE(),
            ERROR_LINE(),
            ERROR_MESSAGE()
            );

        -- Pass back the ErrorLogID of the row inserted
        SET @ErrorLogID = @@IDENTITY;
    END TRY
    BEGIN CATCH
        PRINT ''An error occurred in stored procedure uspLogError: '';
        EXECUTE [dbo].[uspPrintError];
        RETURN -1;
    END CATCH
END;
'
GO
PRINT N'Creating [Production].[TransactionHistory]'
GO
IF OBJECT_ID(N'[Production].[TransactionHistory]', 'U') IS NULL
CREATE TABLE [Production].[TransactionHistory]
(
[TransactionID] [int] NOT NULL IDENTITY(100000, 1),
[ProductID] [int] NOT NULL,
[ReferenceOrderID] [int] NOT NULL,
[ReferenceOrderLineID] [int] NOT NULL CONSTRAINT [DF_TransactionHistory_ReferenceOrderLineID] DEFAULT ((0)),
[TransactionDate] [datetime] NOT NULL CONSTRAINT [DF_TransactionHistory_TransactionDate] DEFAULT (getdate()),
[TransactionType] [nchar] (1) NOT NULL,
[Quantity] [int] NOT NULL,
[ActualCost] [money] NOT NULL,
[ModifiedDate] [datetime] NOT NULL CONSTRAINT [DF_TransactionHistory_ModifiedDate] DEFAULT (getdate())
)
GO
PRINT N'Creating primary key [PK_TransactionHistory_TransactionID] on [Production].[TransactionHistory]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[Production].[PK_TransactionHistory_TransactionID]', 'PK') AND parent_object_id = OBJECT_ID(N'[Production].[TransactionHistory]', 'U'))
ALTER TABLE [Production].[TransactionHistory] ADD CONSTRAINT [PK_TransactionHistory_TransactionID] PRIMARY KEY CLUSTERED  ([TransactionID])
GO
PRINT N'Creating index [IX_TransactionHistory_ProductID] on [Production].[TransactionHistory]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_TransactionHistory_ProductID' AND object_id = OBJECT_ID(N'[Production].[TransactionHistory]'))
CREATE NONCLUSTERED INDEX [IX_TransactionHistory_ProductID] ON [Production].[TransactionHistory] ([ProductID])
GO
PRINT N'Creating index [IX_TransactionHistory_ReferenceOrderID_ReferenceOrderLineID] on [Production].[TransactionHistory]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_TransactionHistory_ReferenceOrderID_ReferenceOrderLineID' AND object_id = OBJECT_ID(N'[Production].[TransactionHistory]'))
CREATE NONCLUSTERED INDEX [IX_TransactionHistory_ReferenceOrderID_ReferenceOrderLineID] ON [Production].[TransactionHistory] ([ReferenceOrderID], [ReferenceOrderLineID])
GO
PRINT N'Creating [Purchasing].[PurchaseOrderHeader]'
GO
IF OBJECT_ID(N'[Purchasing].[PurchaseOrderHeader]', 'U') IS NULL
CREATE TABLE [Purchasing].[PurchaseOrderHeader]
(
[PurchaseOrderID] [int] NOT NULL IDENTITY(1, 1),
[RevisionNumber] [tinyint] NOT NULL CONSTRAINT [DF_PurchaseOrderHeader_RevisionNumber] DEFAULT ((0)),
[Status] [tinyint] NOT NULL CONSTRAINT [DF_PurchaseOrderHeader_Status] DEFAULT ((1)),
[EmployeeID] [int] NOT NULL,
[VendorID] [int] NOT NULL,
[ShipMethodID] [int] NOT NULL,
[OrderDate] [datetime] NOT NULL CONSTRAINT [DF_PurchaseOrderHeader_OrderDate] DEFAULT (getdate()),
[ShipDate] [datetime] NULL,
[SubTotal] [money] NOT NULL CONSTRAINT [DF_PurchaseOrderHeader_SubTotal] DEFAULT ((0.00)),
[TaxAmt] [money] NOT NULL CONSTRAINT [DF_PurchaseOrderHeader_TaxAmt] DEFAULT ((0.00)),
[Freight] [money] NOT NULL CONSTRAINT [DF_PurchaseOrderHeader_Freight] DEFAULT ((0.00)),
[TotalDue] AS (isnull(([SubTotal]+[TaxAmt])+[Freight],(0))) PERSISTED NOT NULL,
[ModifiedDate] [datetime] NOT NULL CONSTRAINT [DF_PurchaseOrderHeader_ModifiedDate] DEFAULT (getdate())
)
GO
PRINT N'Creating primary key [PK_PurchaseOrderHeader_PurchaseOrderID] on [Purchasing].[PurchaseOrderHeader]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[Purchasing].[PK_PurchaseOrderHeader_PurchaseOrderID]', 'PK') AND parent_object_id = OBJECT_ID(N'[Purchasing].[PurchaseOrderHeader]', 'U'))
ALTER TABLE [Purchasing].[PurchaseOrderHeader] ADD CONSTRAINT [PK_PurchaseOrderHeader_PurchaseOrderID] PRIMARY KEY CLUSTERED  ([PurchaseOrderID])
GO
PRINT N'Creating index [IX_PurchaseOrderHeader_EmployeeID] on [Purchasing].[PurchaseOrderHeader]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_PurchaseOrderHeader_EmployeeID' AND object_id = OBJECT_ID(N'[Purchasing].[PurchaseOrderHeader]'))
CREATE NONCLUSTERED INDEX [IX_PurchaseOrderHeader_EmployeeID] ON [Purchasing].[PurchaseOrderHeader] ([EmployeeID])
GO
PRINT N'Creating index [IX_PurchaseOrderHeader_VendorID] on [Purchasing].[PurchaseOrderHeader]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_PurchaseOrderHeader_VendorID' AND object_id = OBJECT_ID(N'[Purchasing].[PurchaseOrderHeader]'))
CREATE NONCLUSTERED INDEX [IX_PurchaseOrderHeader_VendorID] ON [Purchasing].[PurchaseOrderHeader] ([VendorID])
GO
PRINT N'Creating trigger [Purchasing].[iPurchaseOrderDetail] on [Purchasing].[PurchaseOrderDetail]'
GO
IF OBJECT_ID(N'[Purchasing].[iPurchaseOrderDetail]', 'TR') IS NULL
EXEC sp_executesql N'
CREATE TRIGGER [Purchasing].[iPurchaseOrderDetail] ON [Purchasing].[PurchaseOrderDetail] 
AFTER INSERT AS
BEGIN
    DECLARE @Count int;

    SET @Count = @@ROWCOUNT;
    IF @Count = 0 
        RETURN;

    SET NOCOUNT ON;

    BEGIN TRY
        INSERT INTO [Production].[TransactionHistory]
            ([ProductID]
            ,[ReferenceOrderID]
            ,[ReferenceOrderLineID]
            ,[TransactionType]
            ,[TransactionDate]
            ,[Quantity]
            ,[ActualCost])
        SELECT 
            inserted.[ProductID]
            ,inserted.[PurchaseOrderID]
            ,inserted.[PurchaseOrderDetailID]
            ,''P''
            ,GETDATE()
            ,inserted.[OrderQty]
            ,inserted.[UnitPrice]
        FROM inserted 
            INNER JOIN [Purchasing].[PurchaseOrderHeader] 
            ON inserted.[PurchaseOrderID] = [Purchasing].[PurchaseOrderHeader].[PurchaseOrderID];

        -- Update SubTotal in PurchaseOrderHeader record. Note that this causes the 
        -- PurchaseOrderHeader trigger to fire which will update the RevisionNumber.
        UPDATE [Purchasing].[PurchaseOrderHeader]
        SET [Purchasing].[PurchaseOrderHeader].[SubTotal] = 
            (SELECT SUM([Purchasing].[PurchaseOrderDetail].[LineTotal])
                FROM [Purchasing].[PurchaseOrderDetail]
                WHERE [Purchasing].[PurchaseOrderHeader].[PurchaseOrderID] = [Purchasing].[PurchaseOrderDetail].[PurchaseOrderID])
        WHERE [Purchasing].[PurchaseOrderHeader].[PurchaseOrderID] IN (SELECT inserted.[PurchaseOrderID] FROM inserted);
    END TRY
    BEGIN CATCH
        EXECUTE [dbo].[uspPrintError];

        -- Rollback any active or uncommittable transactions before
        -- inserting information in the ErrorLog
        IF @@TRANCOUNT > 0
        BEGIN
            ROLLBACK TRANSACTION;
        END

        EXECUTE [dbo].[uspLogError];
    END CATCH;
END;
'
GO
PRINT N'Creating trigger [Purchasing].[uPurchaseOrderDetail] on [Purchasing].[PurchaseOrderDetail]'
GO
IF OBJECT_ID(N'[Purchasing].[uPurchaseOrderDetail]', 'TR') IS NULL
EXEC sp_executesql N'
CREATE TRIGGER [Purchasing].[uPurchaseOrderDetail] ON [Purchasing].[PurchaseOrderDetail] 
AFTER UPDATE AS 
BEGIN
    DECLARE @Count int;

    SET @Count = @@ROWCOUNT;
    IF @Count = 0 
        RETURN;

    SET NOCOUNT ON;

    BEGIN TRY
        IF UPDATE([ProductID]) OR UPDATE([OrderQty]) OR UPDATE([UnitPrice])
        -- Insert record into TransactionHistory 
        BEGIN
            INSERT INTO [Production].[TransactionHistory]
                ([ProductID]
                ,[ReferenceOrderID]
                ,[ReferenceOrderLineID]
                ,[TransactionType]
                ,[TransactionDate]
                ,[Quantity]
                ,[ActualCost])
            SELECT 
                inserted.[ProductID]
                ,inserted.[PurchaseOrderID]
                ,inserted.[PurchaseOrderDetailID]
                ,''P''
                ,GETDATE()
                ,inserted.[OrderQty]
                ,inserted.[UnitPrice]
            FROM inserted 
                INNER JOIN [Purchasing].[PurchaseOrderDetail] 
                ON inserted.[PurchaseOrderID] = [Purchasing].[PurchaseOrderDetail].[PurchaseOrderID];

            -- Update SubTotal in PurchaseOrderHeader record. Note that this causes the 
            -- PurchaseOrderHeader trigger to fire which will update the RevisionNumber.
            UPDATE [Purchasing].[PurchaseOrderHeader]
            SET [Purchasing].[PurchaseOrderHeader].[SubTotal] = 
                (SELECT SUM([Purchasing].[PurchaseOrderDetail].[LineTotal])
                    FROM [Purchasing].[PurchaseOrderDetail]
                    WHERE [Purchasing].[PurchaseOrderHeader].[PurchaseOrderID] 
                        = [Purchasing].[PurchaseOrderDetail].[PurchaseOrderID])
            WHERE [Purchasing].[PurchaseOrderHeader].[PurchaseOrderID] 
                IN (SELECT inserted.[PurchaseOrderID] FROM inserted);

            UPDATE [Purchasing].[PurchaseOrderDetail]
            SET [Purchasing].[PurchaseOrderDetail].[ModifiedDate] = GETDATE()
            FROM inserted
            WHERE inserted.[PurchaseOrderID] = [Purchasing].[PurchaseOrderDetail].[PurchaseOrderID]
                AND inserted.[PurchaseOrderDetailID] = [Purchasing].[PurchaseOrderDetail].[PurchaseOrderDetailID];
        END;
    END TRY
    BEGIN CATCH
        EXECUTE [dbo].[uspPrintError];

        -- Rollback any active or uncommittable transactions before
        -- inserting information in the ErrorLog
        IF @@TRANCOUNT > 0
        BEGIN
            ROLLBACK TRANSACTION;
        END

        EXECUTE [dbo].[uspLogError];
    END CATCH;
END;
'
GO
PRINT N'Creating trigger [Purchasing].[uPurchaseOrderHeader] on [Purchasing].[PurchaseOrderHeader]'
GO
IF OBJECT_ID(N'[Purchasing].[uPurchaseOrderHeader]', 'TR') IS NULL
EXEC sp_executesql N'
CREATE TRIGGER [Purchasing].[uPurchaseOrderHeader] ON [Purchasing].[PurchaseOrderHeader] 
AFTER UPDATE AS 
BEGIN
    DECLARE @Count int;

    SET @Count = @@ROWCOUNT;
    IF @Count = 0 
        RETURN;

    SET NOCOUNT ON;

    BEGIN TRY
        -- Update RevisionNumber for modification of any field EXCEPT the Status.
        IF NOT UPDATE([Status])
        BEGIN
            UPDATE [Purchasing].[PurchaseOrderHeader]
            SET [Purchasing].[PurchaseOrderHeader].[RevisionNumber] = 
                [Purchasing].[PurchaseOrderHeader].[RevisionNumber] + 1
            WHERE [Purchasing].[PurchaseOrderHeader].[PurchaseOrderID] IN 
                (SELECT inserted.[PurchaseOrderID] FROM inserted);
        END;
    END TRY
    BEGIN CATCH
        EXECUTE [dbo].[uspPrintError];

        -- Rollback any active or uncommittable transactions before
        -- inserting information in the ErrorLog
        IF @@TRANCOUNT > 0
        BEGIN
            ROLLBACK TRANSACTION;
        END

        EXECUTE [dbo].[uspLogError];
    END CATCH;
END;
'
GO
PRINT N'Creating [Sales].[SalesOrderDetail]'
GO
IF OBJECT_ID(N'[Sales].[SalesOrderDetail]', 'U') IS NULL
CREATE TABLE [Sales].[SalesOrderDetail]
(
[SalesOrderID] [int] NOT NULL,
[SalesOrderDetailID] [int] NOT NULL IDENTITY(1, 1),
[CarrierTrackingNumber] [nvarchar] (25) NULL,
[OrderQty] [smallint] NOT NULL,
[ProductID] [int] NOT NULL,
[SpecialOfferID] [int] NOT NULL,
[UnitPrice] [money] NOT NULL,
[UnitPriceDiscount] [money] NOT NULL CONSTRAINT [DF_SalesOrderDetail_UnitPriceDiscount] DEFAULT ((0.0)),
[LineTotal] AS (isnull(([UnitPrice]*((1.0)-[UnitPriceDiscount]))*[OrderQty],(0.0))),
[rowguid] [uniqueidentifier] NOT NULL ROWGUIDCOL CONSTRAINT [DF_SalesOrderDetail_rowguid] DEFAULT (newid()),
[ModifiedDate] [datetime] NOT NULL CONSTRAINT [DF_SalesOrderDetail_ModifiedDate] DEFAULT (getdate())
)
GO
PRINT N'Creating primary key [PK_SalesOrderDetail_SalesOrderID_SalesOrderDetailID] on [Sales].[SalesOrderDetail]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[Sales].[PK_SalesOrderDetail_SalesOrderID_SalesOrderDetailID]', 'PK') AND parent_object_id = OBJECT_ID(N'[Sales].[SalesOrderDetail]', 'U'))
ALTER TABLE [Sales].[SalesOrderDetail] ADD CONSTRAINT [PK_SalesOrderDetail_SalesOrderID_SalesOrderDetailID] PRIMARY KEY CLUSTERED  ([SalesOrderID], [SalesOrderDetailID])
GO
PRINT N'Creating index [IX_SalesOrderDetail_ProductID] on [Sales].[SalesOrderDetail]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_SalesOrderDetail_ProductID' AND object_id = OBJECT_ID(N'[Sales].[SalesOrderDetail]'))
CREATE NONCLUSTERED INDEX [IX_SalesOrderDetail_ProductID] ON [Sales].[SalesOrderDetail] ([ProductID])
GO
PRINT N'Creating index [AK_SalesOrderDetail_rowguid] on [Sales].[SalesOrderDetail]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'AK_SalesOrderDetail_rowguid' AND object_id = OBJECT_ID(N'[Sales].[SalesOrderDetail]'))
CREATE UNIQUE NONCLUSTERED INDEX [AK_SalesOrderDetail_rowguid] ON [Sales].[SalesOrderDetail] ([rowguid])
GO
PRINT N'Creating [Sales].[SalesOrderHeader]'
GO
IF OBJECT_ID(N'[Sales].[SalesOrderHeader]', 'U') IS NULL
CREATE TABLE [Sales].[SalesOrderHeader]
(
[SalesOrderID] [int] NOT NULL IDENTITY(1, 1) NOT FOR REPLICATION,
[RevisionNumber] [tinyint] NOT NULL CONSTRAINT [DF_SalesOrderHeader_RevisionNumber] DEFAULT ((0)),
[OrderDate] [datetime] NOT NULL CONSTRAINT [DF_SalesOrderHeader_OrderDate] DEFAULT (getdate()),
[DueDate] [datetime] NOT NULL,
[ShipDate] [datetime] NULL,
[Status] [tinyint] NOT NULL CONSTRAINT [DF_SalesOrderHeader_Status] DEFAULT ((1)),
[OnlineOrderFlag] [dbo].[Flag] NOT NULL CONSTRAINT [DF_SalesOrderHeader_OnlineOrderFlag] DEFAULT ((1)),
[SalesOrderNumber] AS (isnull(N'SO'+CONVERT([nvarchar](23),[SalesOrderID],(0)),N'*** ERROR ***')),
[PurchaseOrderNumber] [dbo].[OrderNumber] NULL,
[AccountNumber] [dbo].[AccountNumber] NULL,
[CustomerID] [int] NOT NULL,
[SalesPersonID] [int] NULL,
[TerritoryID] [int] NULL,
[BillToAddressID] [int] NOT NULL,
[ShipToAddressID] [int] NOT NULL,
[ShipMethodID] [int] NOT NULL,
[CreditCardID] [int] NULL,
[CreditCardApprovalCode] [varchar] (15) NULL,
[CurrencyRateID] [int] NULL,
[SubTotal] [money] NOT NULL CONSTRAINT [DF_SalesOrderHeader_SubTotal] DEFAULT ((0.00)),
[TaxAmt] [money] NOT NULL CONSTRAINT [DF_SalesOrderHeader_TaxAmt] DEFAULT ((0.00)),
[Freight] [money] NOT NULL CONSTRAINT [DF_SalesOrderHeader_Freight] DEFAULT ((0.00)),
[TotalDue] AS (isnull(([SubTotal]+[TaxAmt])+[Freight],(0))),
[Comment] [nvarchar] (128) NULL,
[rowguid] [uniqueidentifier] NOT NULL ROWGUIDCOL CONSTRAINT [DF_SalesOrderHeader_rowguid] DEFAULT (newid()),
[ModifiedDate] [datetime] NOT NULL CONSTRAINT [DF_SalesOrderHeader_ModifiedDate] DEFAULT (getdate())
)
GO
PRINT N'Creating primary key [PK_SalesOrderHeader_SalesOrderID] on [Sales].[SalesOrderHeader]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[Sales].[PK_SalesOrderHeader_SalesOrderID]', 'PK') AND parent_object_id = OBJECT_ID(N'[Sales].[SalesOrderHeader]', 'U'))
ALTER TABLE [Sales].[SalesOrderHeader] ADD CONSTRAINT [PK_SalesOrderHeader_SalesOrderID] PRIMARY KEY CLUSTERED  ([SalesOrderID])
GO
PRINT N'Creating index [IX_SalesOrderHeader_CustomerID] on [Sales].[SalesOrderHeader]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_SalesOrderHeader_CustomerID' AND object_id = OBJECT_ID(N'[Sales].[SalesOrderHeader]'))
CREATE NONCLUSTERED INDEX [IX_SalesOrderHeader_CustomerID] ON [Sales].[SalesOrderHeader] ([CustomerID])
GO
PRINT N'Creating index [AK_SalesOrderHeader_rowguid] on [Sales].[SalesOrderHeader]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'AK_SalesOrderHeader_rowguid' AND object_id = OBJECT_ID(N'[Sales].[SalesOrderHeader]'))
CREATE UNIQUE NONCLUSTERED INDEX [AK_SalesOrderHeader_rowguid] ON [Sales].[SalesOrderHeader] ([rowguid])
GO
PRINT N'Creating index [AK_SalesOrderHeader_SalesOrderNumber] on [Sales].[SalesOrderHeader]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'AK_SalesOrderHeader_SalesOrderNumber' AND object_id = OBJECT_ID(N'[Sales].[SalesOrderHeader]'))
CREATE UNIQUE NONCLUSTERED INDEX [AK_SalesOrderHeader_SalesOrderNumber] ON [Sales].[SalesOrderHeader] ([SalesOrderNumber])
GO
PRINT N'Creating index [IX_SalesOrderHeader_SalesPersonID] on [Sales].[SalesOrderHeader]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_SalesOrderHeader_SalesPersonID' AND object_id = OBJECT_ID(N'[Sales].[SalesOrderHeader]'))
CREATE NONCLUSTERED INDEX [IX_SalesOrderHeader_SalesPersonID] ON [Sales].[SalesOrderHeader] ([SalesPersonID])
GO
PRINT N'Creating [dbo].[ufnLeadingZeros]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ufnLeadingZeros]') AND (type = 'IF' OR type = 'FN' OR type = 'TF'))
EXEC sp_executesql N'
CREATE FUNCTION [dbo].[ufnLeadingZeros](
    @Value int
) 
RETURNS varchar(8) 
WITH SCHEMABINDING 
AS 
BEGIN
    DECLARE @ReturnValue varchar(8);

    SET @ReturnValue = CONVERT(varchar(8), @Value);
    SET @ReturnValue = REPLICATE(''0'', 8 - DATALENGTH(@ReturnValue)) + @ReturnValue;

    RETURN (@ReturnValue);
END;
'
GO
PRINT N'Creating [Sales].[Customer]'
GO
IF OBJECT_ID(N'[Sales].[Customer]', 'U') IS NULL
CREATE TABLE [Sales].[Customer]
(
[CustomerID] [int] NOT NULL IDENTITY(1, 1) NOT FOR REPLICATION,
[PersonID] [int] NULL,
[StoreID] [int] NULL,
[TerritoryID] [int] NULL,
[AccountNumber] AS (isnull('AW'+[dbo].[ufnLeadingZeros]([CustomerID]),'')),
[rowguid] [uniqueidentifier] NOT NULL ROWGUIDCOL CONSTRAINT [DF_Customer_rowguid] DEFAULT (newid()),
[ModifiedDate] [datetime] NOT NULL CONSTRAINT [DF_Customer_ModifiedDate] DEFAULT (getdate())
)
GO
PRINT N'Creating primary key [PK_Customer_CustomerID] on [Sales].[Customer]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[Sales].[PK_Customer_CustomerID]', 'PK') AND parent_object_id = OBJECT_ID(N'[Sales].[Customer]', 'U'))
ALTER TABLE [Sales].[Customer] ADD CONSTRAINT [PK_Customer_CustomerID] PRIMARY KEY CLUSTERED  ([CustomerID])
GO
PRINT N'Creating index [AK_Customer_AccountNumber] on [Sales].[Customer]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'AK_Customer_AccountNumber' AND object_id = OBJECT_ID(N'[Sales].[Customer]'))
CREATE UNIQUE NONCLUSTERED INDEX [AK_Customer_AccountNumber] ON [Sales].[Customer] ([AccountNumber])
GO
PRINT N'Creating index [AK_Customer_rowguid] on [Sales].[Customer]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'AK_Customer_rowguid' AND object_id = OBJECT_ID(N'[Sales].[Customer]'))
CREATE UNIQUE NONCLUSTERED INDEX [AK_Customer_rowguid] ON [Sales].[Customer] ([rowguid])
GO
PRINT N'Creating index [IX_Customer_TerritoryID] on [Sales].[Customer]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Customer_TerritoryID' AND object_id = OBJECT_ID(N'[Sales].[Customer]'))
CREATE NONCLUSTERED INDEX [IX_Customer_TerritoryID] ON [Sales].[Customer] ([TerritoryID])
GO
PRINT N'Creating trigger [Sales].[iduSalesOrderDetail] on [Sales].[SalesOrderDetail]'
GO
IF OBJECT_ID(N'[Sales].[iduSalesOrderDetail]', 'TR') IS NULL
EXEC sp_executesql N'
CREATE TRIGGER [Sales].[iduSalesOrderDetail] ON [Sales].[SalesOrderDetail] 
AFTER INSERT, DELETE, UPDATE AS 
BEGIN
    DECLARE @Count int;

    SET @Count = @@ROWCOUNT;
    IF @Count = 0 
        RETURN;

    SET NOCOUNT ON;

    BEGIN TRY
        -- If inserting or updating these columns
        IF UPDATE([ProductID]) OR UPDATE([OrderQty]) OR UPDATE([UnitPrice]) OR UPDATE([UnitPriceDiscount]) 
        -- Insert record into TransactionHistory
        BEGIN
            INSERT INTO [Production].[TransactionHistory]
                ([ProductID]
                ,[ReferenceOrderID]
                ,[ReferenceOrderLineID]
                ,[TransactionType]
                ,[TransactionDate]
                ,[Quantity]
                ,[ActualCost])
            SELECT 
                inserted.[ProductID]
                ,inserted.[SalesOrderID]
                ,inserted.[SalesOrderDetailID]
                ,''S''
                ,GETDATE()
                ,inserted.[OrderQty]
                ,inserted.[UnitPrice]
            FROM inserted 
                INNER JOIN [Sales].[SalesOrderHeader] 
                ON inserted.[SalesOrderID] = [Sales].[SalesOrderHeader].[SalesOrderID];

            UPDATE [Person].[Person] 
            SET [Demographics].modify(''declare default element namespace 
                "http://schemas.microsoft.com/sqlserver/2004/07/adventure-works/IndividualSurvey"; 
                replace value of (/IndividualSurvey/TotalPurchaseYTD)[1] 
                with data(/IndividualSurvey/TotalPurchaseYTD)[1] + sql:column ("inserted.LineTotal")'') 
            FROM inserted 
                INNER JOIN [Sales].[SalesOrderHeader] AS SOH
                ON inserted.[SalesOrderID] = SOH.[SalesOrderID] 
                INNER JOIN [Sales].[Customer] AS C
                ON SOH.[CustomerID] = C.[CustomerID]
            WHERE C.[PersonID] = [Person].[Person].[BusinessEntityID];
        END;

        -- Update SubTotal in SalesOrderHeader record. Note that this causes the 
        -- SalesOrderHeader trigger to fire which will update the RevisionNumber.
        UPDATE [Sales].[SalesOrderHeader]
        SET [Sales].[SalesOrderHeader].[SubTotal] = 
            (SELECT SUM([Sales].[SalesOrderDetail].[LineTotal])
                FROM [Sales].[SalesOrderDetail]
                WHERE [Sales].[SalesOrderHeader].[SalesOrderID] = [Sales].[SalesOrderDetail].[SalesOrderID])
        WHERE [Sales].[SalesOrderHeader].[SalesOrderID] IN (SELECT inserted.[SalesOrderID] FROM inserted);

        UPDATE [Person].[Person] 
        SET [Demographics].modify(''declare default element namespace 
            "http://schemas.microsoft.com/sqlserver/2004/07/adventure-works/IndividualSurvey"; 
            replace value of (/IndividualSurvey/TotalPurchaseYTD)[1] 
            with data(/IndividualSurvey/TotalPurchaseYTD)[1] - sql:column("deleted.LineTotal")'') 
        FROM deleted 
            INNER JOIN [Sales].[SalesOrderHeader] 
            ON deleted.[SalesOrderID] = [Sales].[SalesOrderHeader].[SalesOrderID] 
            INNER JOIN [Sales].[Customer]
            ON [Sales].[Customer].[CustomerID] = [Sales].[SalesOrderHeader].[CustomerID]
        WHERE [Sales].[Customer].[PersonID] = [Person].[Person].[BusinessEntityID];
    END TRY
    BEGIN CATCH
        EXECUTE [dbo].[uspPrintError];

        -- Rollback any active or uncommittable transactions before
        -- inserting information in the ErrorLog
        IF @@TRANCOUNT > 0
        BEGIN
            ROLLBACK TRANSACTION;
        END

        EXECUTE [dbo].[uspLogError];
    END CATCH;
END;
'
GO
PRINT N'Creating [dbo].[ufnGetAccountingStartDate]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ufnGetAccountingStartDate]') AND (type = 'IF' OR type = 'FN' OR type = 'TF'))
EXEC sp_executesql N'
CREATE FUNCTION [dbo].[ufnGetAccountingStartDate]()
RETURNS [datetime] 
AS 
BEGIN
    RETURN CONVERT(datetime, ''20030701'', 112);
END;
'
GO
PRINT N'Creating [dbo].[ufnGetAccountingEndDate]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ufnGetAccountingEndDate]') AND (type = 'IF' OR type = 'FN' OR type = 'TF'))
EXEC sp_executesql N'
CREATE FUNCTION [dbo].[ufnGetAccountingEndDate]()
RETURNS [datetime] 
AS 
BEGIN
    RETURN DATEADD(millisecond, -2, CONVERT(datetime, ''20040701'', 112));
END;
'
GO
PRINT N'Creating [Sales].[SalesTerritory]'
GO
IF OBJECT_ID(N'[Sales].[SalesTerritory]', 'U') IS NULL
CREATE TABLE [Sales].[SalesTerritory]
(
[TerritoryID] [int] NOT NULL IDENTITY(1, 1),
[Name] [dbo].[Name] NOT NULL,
[CountryRegionCode] [nvarchar] (3) NOT NULL,
[Group] [nvarchar] (50) NOT NULL,
[SalesYTD] [money] NOT NULL CONSTRAINT [DF_SalesTerritory_SalesYTD] DEFAULT ((0.00)),
[SalesLastYear] [money] NOT NULL CONSTRAINT [DF_SalesTerritory_SalesLastYear] DEFAULT ((0.00)),
[CostYTD] [money] NOT NULL CONSTRAINT [DF_SalesTerritory_CostYTD] DEFAULT ((0.00)),
[CostLastYear] [money] NOT NULL CONSTRAINT [DF_SalesTerritory_CostLastYear] DEFAULT ((0.00)),
[rowguid] [uniqueidentifier] NOT NULL ROWGUIDCOL CONSTRAINT [DF_SalesTerritory_rowguid] DEFAULT (newid()),
[ModifiedDate] [datetime] NOT NULL CONSTRAINT [DF_SalesTerritory_ModifiedDate] DEFAULT (getdate())
)
GO
PRINT N'Creating primary key [PK_SalesTerritory_TerritoryID] on [Sales].[SalesTerritory]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[Sales].[PK_SalesTerritory_TerritoryID]', 'PK') AND parent_object_id = OBJECT_ID(N'[Sales].[SalesTerritory]', 'U'))
ALTER TABLE [Sales].[SalesTerritory] ADD CONSTRAINT [PK_SalesTerritory_TerritoryID] PRIMARY KEY CLUSTERED  ([TerritoryID])
GO
PRINT N'Creating index [AK_SalesTerritory_Name] on [Sales].[SalesTerritory]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'AK_SalesTerritory_Name' AND object_id = OBJECT_ID(N'[Sales].[SalesTerritory]'))
CREATE UNIQUE NONCLUSTERED INDEX [AK_SalesTerritory_Name] ON [Sales].[SalesTerritory] ([Name])
GO
PRINT N'Creating index [AK_SalesTerritory_rowguid] on [Sales].[SalesTerritory]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'AK_SalesTerritory_rowguid' AND object_id = OBJECT_ID(N'[Sales].[SalesTerritory]'))
CREATE UNIQUE NONCLUSTERED INDEX [AK_SalesTerritory_rowguid] ON [Sales].[SalesTerritory] ([rowguid])
GO
PRINT N'Creating [Sales].[SalesPerson]'
GO
IF OBJECT_ID(N'[Sales].[SalesPerson]', 'U') IS NULL
CREATE TABLE [Sales].[SalesPerson]
(
[BusinessEntityID] [int] NOT NULL,
[TerritoryID] [int] NULL,
[SalesQuota] [money] NULL,
[Bonus] [money] NOT NULL CONSTRAINT [DF_SalesPerson_Bonus] DEFAULT ((0.00)),
[CommissionPct] [smallmoney] NOT NULL CONSTRAINT [DF_SalesPerson_CommissionPct] DEFAULT ((0.00)),
[SalesYTD] [money] NOT NULL CONSTRAINT [DF_SalesPerson_SalesYTD] DEFAULT ((0.00)),
[SalesLastYear] [money] NOT NULL CONSTRAINT [DF_SalesPerson_SalesLastYear] DEFAULT ((0.00)),
[rowguid] [uniqueidentifier] NOT NULL ROWGUIDCOL CONSTRAINT [DF_SalesPerson_rowguid] DEFAULT (newid()),
[ModifiedDate] [datetime] NOT NULL CONSTRAINT [DF_SalesPerson_ModifiedDate] DEFAULT (getdate())
)
GO
PRINT N'Creating primary key [PK_SalesPerson_BusinessEntityID] on [Sales].[SalesPerson]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[Sales].[PK_SalesPerson_BusinessEntityID]', 'PK') AND parent_object_id = OBJECT_ID(N'[Sales].[SalesPerson]', 'U'))
ALTER TABLE [Sales].[SalesPerson] ADD CONSTRAINT [PK_SalesPerson_BusinessEntityID] PRIMARY KEY CLUSTERED  ([BusinessEntityID])
GO
PRINT N'Creating index [AK_SalesPerson_rowguid] on [Sales].[SalesPerson]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'AK_SalesPerson_rowguid' AND object_id = OBJECT_ID(N'[Sales].[SalesPerson]'))
CREATE UNIQUE NONCLUSTERED INDEX [AK_SalesPerson_rowguid] ON [Sales].[SalesPerson] ([rowguid])
GO
PRINT N'Creating trigger [Sales].[uSalesOrderHeader] on [Sales].[SalesOrderHeader]'
GO
IF OBJECT_ID(N'[Sales].[uSalesOrderHeader]', 'TR') IS NULL
EXEC sp_executesql N'
CREATE TRIGGER [Sales].[uSalesOrderHeader] ON [Sales].[SalesOrderHeader] 
AFTER UPDATE NOT FOR REPLICATION AS 
BEGIN
    DECLARE @Count int;

    SET @Count = @@ROWCOUNT;
    IF @Count = 0 
        RETURN;

    SET NOCOUNT ON;

    BEGIN TRY
        -- Update RevisionNumber for modification of any field EXCEPT the Status.
        IF NOT UPDATE([Status])
        BEGIN
            UPDATE [Sales].[SalesOrderHeader]
            SET [Sales].[SalesOrderHeader].[RevisionNumber] = 
                [Sales].[SalesOrderHeader].[RevisionNumber] + 1
            WHERE [Sales].[SalesOrderHeader].[SalesOrderID] IN 
                (SELECT inserted.[SalesOrderID] FROM inserted);
        END;

        -- Update the SalesPerson SalesYTD when SubTotal is updated
        IF UPDATE([SubTotal])
        BEGIN
            DECLARE @StartDate datetime,
                    @EndDate datetime

            SET @StartDate = [dbo].[ufnGetAccountingStartDate]();
            SET @EndDate = [dbo].[ufnGetAccountingEndDate]();

            UPDATE [Sales].[SalesPerson]
            SET [Sales].[SalesPerson].[SalesYTD] = 
                (SELECT SUM([Sales].[SalesOrderHeader].[SubTotal])
                FROM [Sales].[SalesOrderHeader] 
                WHERE [Sales].[SalesPerson].[BusinessEntityID] = [Sales].[SalesOrderHeader].[SalesPersonID]
                    AND ([Sales].[SalesOrderHeader].[Status] = 5) -- Shipped
                    AND [Sales].[SalesOrderHeader].[OrderDate] BETWEEN @StartDate AND @EndDate)
            WHERE [Sales].[SalesPerson].[BusinessEntityID] 
                IN (SELECT DISTINCT inserted.[SalesPersonID] FROM inserted 
                    WHERE inserted.[OrderDate] BETWEEN @StartDate AND @EndDate);

            -- Update the SalesTerritory SalesYTD when SubTotal is updated
            UPDATE [Sales].[SalesTerritory]
            SET [Sales].[SalesTerritory].[SalesYTD] = 
                (SELECT SUM([Sales].[SalesOrderHeader].[SubTotal])
                FROM [Sales].[SalesOrderHeader] 
                WHERE [Sales].[SalesTerritory].[TerritoryID] = [Sales].[SalesOrderHeader].[TerritoryID]
                    AND ([Sales].[SalesOrderHeader].[Status] = 5) -- Shipped
                    AND [Sales].[SalesOrderHeader].[OrderDate] BETWEEN @StartDate AND @EndDate)
            WHERE [Sales].[SalesTerritory].[TerritoryID] 
                IN (SELECT DISTINCT inserted.[TerritoryID] FROM inserted 
                    WHERE inserted.[OrderDate] BETWEEN @StartDate AND @EndDate);
        END;
    END TRY
    BEGIN CATCH
        EXECUTE [dbo].[uspPrintError];

        -- Rollback any active or uncommittable transactions before
        -- inserting information in the ErrorLog
        IF @@TRANCOUNT > 0
        BEGIN
            ROLLBACK TRANSACTION;
        END

        EXECUTE [dbo].[uspLogError];
    END CATCH;
END;
'
GO
PRINT N'Creating [Purchasing].[Vendor]'
GO
IF OBJECT_ID(N'[Purchasing].[Vendor]', 'U') IS NULL
CREATE TABLE [Purchasing].[Vendor]
(
[BusinessEntityID] [int] NOT NULL,
[AccountNumber] [dbo].[AccountNumber] NOT NULL,
[Name] [dbo].[Name] NOT NULL,
[CreditRating] [tinyint] NOT NULL,
[PreferredVendorStatus] [dbo].[Flag] NOT NULL CONSTRAINT [DF_Vendor_PreferredVendorStatus] DEFAULT ((1)),
[ActiveFlag] [dbo].[Flag] NOT NULL CONSTRAINT [DF_Vendor_ActiveFlag] DEFAULT ((1)),
[PurchasingWebServiceURL] [nvarchar] (1024) NULL,
[ModifiedDate] [datetime] NOT NULL CONSTRAINT [DF_Vendor_ModifiedDate] DEFAULT (getdate())
)
GO
PRINT N'Creating primary key [PK_Vendor_BusinessEntityID] on [Purchasing].[Vendor]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[Purchasing].[PK_Vendor_BusinessEntityID]', 'PK') AND parent_object_id = OBJECT_ID(N'[Purchasing].[Vendor]', 'U'))
ALTER TABLE [Purchasing].[Vendor] ADD CONSTRAINT [PK_Vendor_BusinessEntityID] PRIMARY KEY CLUSTERED  ([BusinessEntityID])
GO
PRINT N'Creating index [AK_Vendor_AccountNumber] on [Purchasing].[Vendor]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'AK_Vendor_AccountNumber' AND object_id = OBJECT_ID(N'[Purchasing].[Vendor]'))
CREATE UNIQUE NONCLUSTERED INDEX [AK_Vendor_AccountNumber] ON [Purchasing].[Vendor] ([AccountNumber])
GO
PRINT N'Creating trigger [Purchasing].[dVendor] on [Purchasing].[Vendor]'
GO
IF OBJECT_ID(N'[Purchasing].[dVendor]', 'TR') IS NULL
EXEC sp_executesql N'
CREATE TRIGGER [Purchasing].[dVendor] ON [Purchasing].[Vendor] 
INSTEAD OF DELETE NOT FOR REPLICATION AS 
BEGIN
    DECLARE @Count int;

    SET @Count = @@ROWCOUNT;
    IF @Count = 0 
        RETURN;

    SET NOCOUNT ON;

    BEGIN TRY
        DECLARE @DeleteCount int;

        SELECT @DeleteCount = COUNT(*) FROM deleted;
        IF @DeleteCount > 0 
        BEGIN
            RAISERROR
                (N''Vendors cannot be deleted. They can only be marked as not active.'', -- Message
                10, -- Severity.
                1); -- State.

        -- Rollback any active or uncommittable transactions
            IF @@TRANCOUNT > 0
            BEGIN
                ROLLBACK TRANSACTION;
            END
        END;
    END TRY
    BEGIN CATCH
        EXECUTE [dbo].[uspPrintError];

        -- Rollback any active or uncommittable transactions before
        -- inserting information in the ErrorLog
        IF @@TRANCOUNT > 0
        BEGIN
            ROLLBACK TRANSACTION;
        END

        EXECUTE [dbo].[uspLogError];
    END CATCH;
END;
'
GO
PRINT N'Creating [Production].[WorkOrder]'
GO
IF OBJECT_ID(N'[Production].[WorkOrder]', 'U') IS NULL
CREATE TABLE [Production].[WorkOrder]
(
[WorkOrderID] [int] NOT NULL IDENTITY(1, 1),
[ProductID] [int] NOT NULL,
[OrderQty] [int] NOT NULL,
[StockedQty] AS (isnull([OrderQty]-[ScrappedQty],(0))),
[ScrappedQty] [smallint] NOT NULL,
[StartDate] [datetime] NOT NULL,
[EndDate] [datetime] NULL,
[DueDate] [datetime] NOT NULL,
[ScrapReasonID] [smallint] NULL,
[ModifiedDate] [datetime] NOT NULL CONSTRAINT [DF_WorkOrder_ModifiedDate] DEFAULT (getdate())
)
GO
PRINT N'Creating primary key [PK_WorkOrder_WorkOrderID] on [Production].[WorkOrder]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[Production].[PK_WorkOrder_WorkOrderID]', 'PK') AND parent_object_id = OBJECT_ID(N'[Production].[WorkOrder]', 'U'))
ALTER TABLE [Production].[WorkOrder] ADD CONSTRAINT [PK_WorkOrder_WorkOrderID] PRIMARY KEY CLUSTERED  ([WorkOrderID])
GO
PRINT N'Creating index [IX_WorkOrder_ProductID] on [Production].[WorkOrder]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_WorkOrder_ProductID' AND object_id = OBJECT_ID(N'[Production].[WorkOrder]'))
CREATE NONCLUSTERED INDEX [IX_WorkOrder_ProductID] ON [Production].[WorkOrder] ([ProductID])
GO
PRINT N'Creating index [IX_WorkOrder_ScrapReasonID] on [Production].[WorkOrder]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_WorkOrder_ScrapReasonID' AND object_id = OBJECT_ID(N'[Production].[WorkOrder]'))
CREATE NONCLUSTERED INDEX [IX_WorkOrder_ScrapReasonID] ON [Production].[WorkOrder] ([ScrapReasonID])
GO
PRINT N'Creating trigger [Production].[iWorkOrder] on [Production].[WorkOrder]'
GO
IF OBJECT_ID(N'[Production].[iWorkOrder]', 'TR') IS NULL
EXEC sp_executesql N'
CREATE TRIGGER [Production].[iWorkOrder] ON [Production].[WorkOrder] 
AFTER INSERT AS 
BEGIN
    DECLARE @Count int;

    SET @Count = @@ROWCOUNT;
    IF @Count = 0 
        RETURN;

    SET NOCOUNT ON;

    BEGIN TRY
        INSERT INTO [Production].[TransactionHistory](
            [ProductID]
            ,[ReferenceOrderID]
            ,[TransactionType]
            ,[TransactionDate]
            ,[Quantity]
            ,[ActualCost])
        SELECT 
            inserted.[ProductID]
            ,inserted.[WorkOrderID]
            ,''W''
            ,GETDATE()
            ,inserted.[OrderQty]
            ,0
        FROM inserted;
    END TRY
    BEGIN CATCH
        EXECUTE [dbo].[uspPrintError];

        -- Rollback any active or uncommittable transactions before
        -- inserting information in the ErrorLog
        IF @@TRANCOUNT > 0
        BEGIN
            ROLLBACK TRANSACTION;
        END

        EXECUTE [dbo].[uspLogError];
    END CATCH;
END;
'
GO
PRINT N'Creating trigger [Production].[uWorkOrder] on [Production].[WorkOrder]'
GO
IF OBJECT_ID(N'[Production].[uWorkOrder]', 'TR') IS NULL
EXEC sp_executesql N'
CREATE TRIGGER [Production].[uWorkOrder] ON [Production].[WorkOrder] 
AFTER UPDATE AS 
BEGIN
    DECLARE @Count int;

    SET @Count = @@ROWCOUNT;
    IF @Count = 0 
        RETURN;

    SET NOCOUNT ON;

    BEGIN TRY
        IF UPDATE([ProductID]) OR UPDATE([OrderQty])
        BEGIN
            INSERT INTO [Production].[TransactionHistory](
                [ProductID]
                ,[ReferenceOrderID]
                ,[TransactionType]
                ,[TransactionDate]
                ,[Quantity])
            SELECT 
                inserted.[ProductID]
                ,inserted.[WorkOrderID]
                ,''W''
                ,GETDATE()
                ,inserted.[OrderQty]
            FROM inserted;
        END;
    END TRY
    BEGIN CATCH
        EXECUTE [dbo].[uspPrintError];

        -- Rollback any active or uncommittable transactions before
        -- inserting information in the ErrorLog
        IF @@TRANCOUNT > 0
        BEGIN
            ROLLBACK TRANSACTION;
        END

        EXECUTE [dbo].[uspLogError];
    END CATCH;
END;
'
GO
PRINT N'Creating [Person].[StateProvince]'
GO
IF OBJECT_ID(N'[Person].[StateProvince]', 'U') IS NULL
CREATE TABLE [Person].[StateProvince]
(
[StateProvinceID] [int] NOT NULL IDENTITY(1, 1),
[StateProvinceCode] [nchar] (3) NOT NULL,
[CountryRegionCode] [nvarchar] (3) NOT NULL,
[IsOnlyStateProvinceFlag] [dbo].[Flag] NOT NULL CONSTRAINT [DF_StateProvince_IsOnlyStateProvinceFlag] DEFAULT ((1)),
[Name] [dbo].[Name] NOT NULL,
[TerritoryID] [int] NOT NULL,
[rowguid] [uniqueidentifier] NOT NULL ROWGUIDCOL CONSTRAINT [DF_StateProvince_rowguid] DEFAULT (newid()),
[ModifiedDate] [datetime] NOT NULL CONSTRAINT [DF_StateProvince_ModifiedDate] DEFAULT (getdate())
)
GO
PRINT N'Creating primary key [PK_StateProvince_StateProvinceID] on [Person].[StateProvince]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[Person].[PK_StateProvince_StateProvinceID]', 'PK') AND parent_object_id = OBJECT_ID(N'[Person].[StateProvince]', 'U'))
ALTER TABLE [Person].[StateProvince] ADD CONSTRAINT [PK_StateProvince_StateProvinceID] PRIMARY KEY CLUSTERED  ([StateProvinceID])
GO
PRINT N'Creating index [AK_StateProvince_Name] on [Person].[StateProvince]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'AK_StateProvince_Name' AND object_id = OBJECT_ID(N'[Person].[StateProvince]'))
CREATE UNIQUE NONCLUSTERED INDEX [AK_StateProvince_Name] ON [Person].[StateProvince] ([Name])
GO
PRINT N'Creating index [AK_StateProvince_rowguid] on [Person].[StateProvince]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'AK_StateProvince_rowguid' AND object_id = OBJECT_ID(N'[Person].[StateProvince]'))
CREATE UNIQUE NONCLUSTERED INDEX [AK_StateProvince_rowguid] ON [Person].[StateProvince] ([rowguid])
GO
PRINT N'Creating index [AK_StateProvince_StateProvinceCode_CountryRegionCode] on [Person].[StateProvince]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'AK_StateProvince_StateProvinceCode_CountryRegionCode' AND object_id = OBJECT_ID(N'[Person].[StateProvince]'))
CREATE UNIQUE NONCLUSTERED INDEX [AK_StateProvince_StateProvinceCode_CountryRegionCode] ON [Person].[StateProvince] ([StateProvinceCode], [CountryRegionCode])
GO
PRINT N'Creating [Person].[Address]'
GO
IF OBJECT_ID(N'[Person].[Address]', 'U') IS NULL
CREATE TABLE [Person].[Address]
(
[AddressID] [int] NOT NULL IDENTITY(1, 1) NOT FOR REPLICATION,
[AddressLine1] [nvarchar] (60) NOT NULL,
[AddressLine2] [nvarchar] (60) NULL,
[City] [nvarchar] (30) NOT NULL,
[StateProvinceID] [int] NOT NULL,
[PostalCode] [nvarchar] (15) NOT NULL,
[SpatialLocation] [sys].[geography] NULL,
[rowguid] [uniqueidentifier] NOT NULL ROWGUIDCOL CONSTRAINT [DF_Address_rowguid] DEFAULT (newid()),
[ModifiedDate] [datetime] NOT NULL CONSTRAINT [DF_Address_ModifiedDate] DEFAULT (getdate())
)
GO
PRINT N'Creating primary key [PK_Address_AddressID] on [Person].[Address]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[Person].[PK_Address_AddressID]', 'PK') AND parent_object_id = OBJECT_ID(N'[Person].[Address]', 'U'))
ALTER TABLE [Person].[Address] ADD CONSTRAINT [PK_Address_AddressID] PRIMARY KEY CLUSTERED  ([AddressID])
GO
PRINT N'Creating index [IX_Address_AddressLine1_AddressLine2_City_StateProvinceID_PostalCode] on [Person].[Address]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Address_AddressLine1_AddressLine2_City_StateProvinceID_PostalCode' AND object_id = OBJECT_ID(N'[Person].[Address]'))
CREATE UNIQUE NONCLUSTERED INDEX [IX_Address_AddressLine1_AddressLine2_City_StateProvinceID_PostalCode] ON [Person].[Address] ([AddressLine1], [AddressLine2], [City], [StateProvinceID], [PostalCode])
GO
PRINT N'Creating index [AK_Address_rowguid] on [Person].[Address]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'AK_Address_rowguid' AND object_id = OBJECT_ID(N'[Person].[Address]'))
CREATE UNIQUE NONCLUSTERED INDEX [AK_Address_rowguid] ON [Person].[Address] ([rowguid])
GO
PRINT N'Creating index [IX_Address_StateProvinceID] on [Person].[Address]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Address_StateProvinceID' AND object_id = OBJECT_ID(N'[Person].[Address]'))
CREATE NONCLUSTERED INDEX [IX_Address_StateProvinceID] ON [Person].[Address] ([StateProvinceID])
GO
PRINT N'Creating [Production].[Product]'
GO
IF OBJECT_ID(N'[Production].[Product]', 'U') IS NULL
CREATE TABLE [Production].[Product]
(
[ProductID] [int] NOT NULL IDENTITY(1, 1),
[Name] [dbo].[Name] NOT NULL,
[ProductNumber] [nvarchar] (25) NOT NULL,
[MakeFlag] [dbo].[Flag] NOT NULL CONSTRAINT [DF_Product_MakeFlag] DEFAULT ((1)),
[FinishedGoodsFlag] [dbo].[Flag] NOT NULL CONSTRAINT [DF_Product_FinishedGoodsFlag] DEFAULT ((1)),
[Color] [nvarchar] (15) NULL,
[SafetyStockLevel] [smallint] NOT NULL,
[ReorderPoint] [smallint] NOT NULL,
[StandardCost] [money] NOT NULL,
[ListPrice] [money] NOT NULL,
[Size] [nvarchar] (5) NULL,
[SizeUnitMeasureCode] [nchar] (3) NULL,
[WeightUnitMeasureCode] [nchar] (3) NULL,
[Weight] [decimal] (8, 2) NULL,
[DaysToManufacture] [int] NOT NULL,
[ProductLine] [nchar] (2) NULL,
[Class] [nchar] (2) NULL,
[Style] [nchar] (2) NULL,
[ProductSubcategoryID] [int] NULL,
[ProductModelID] [int] NULL,
[SellStartDate] [datetime] NOT NULL,
[SellEndDate] [datetime] NULL,
[DiscontinuedDate] [datetime] NULL,
[rowguid] [uniqueidentifier] NOT NULL ROWGUIDCOL CONSTRAINT [DF_Product_rowguid] DEFAULT (newid()),
[ModifiedDate] [datetime] NOT NULL CONSTRAINT [DF_Product_ModifiedDate] DEFAULT (getdate())
)
GO
PRINT N'Creating primary key [PK_Product_ProductID] on [Production].[Product]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[Production].[PK_Product_ProductID]', 'PK') AND parent_object_id = OBJECT_ID(N'[Production].[Product]', 'U'))
ALTER TABLE [Production].[Product] ADD CONSTRAINT [PK_Product_ProductID] PRIMARY KEY CLUSTERED  ([ProductID])
GO
PRINT N'Creating index [AK_Product_Name] on [Production].[Product]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'AK_Product_Name' AND object_id = OBJECT_ID(N'[Production].[Product]'))
CREATE UNIQUE NONCLUSTERED INDEX [AK_Product_Name] ON [Production].[Product] ([Name])
GO
PRINT N'Creating index [AK_Product_ProductNumber] on [Production].[Product]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'AK_Product_ProductNumber' AND object_id = OBJECT_ID(N'[Production].[Product]'))
CREATE UNIQUE NONCLUSTERED INDEX [AK_Product_ProductNumber] ON [Production].[Product] ([ProductNumber])
GO
PRINT N'Creating index [AK_Product_rowguid] on [Production].[Product]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'AK_Product_rowguid' AND object_id = OBJECT_ID(N'[Production].[Product]'))
CREATE UNIQUE NONCLUSTERED INDEX [AK_Product_rowguid] ON [Production].[Product] ([rowguid])
GO
PRINT N'Creating [Production].[BillOfMaterials]'
GO
IF OBJECT_ID(N'[Production].[BillOfMaterials]', 'U') IS NULL
CREATE TABLE [Production].[BillOfMaterials]
(
[BillOfMaterialsID] [int] NOT NULL IDENTITY(1, 1),
[ProductAssemblyID] [int] NULL,
[ComponentID] [int] NOT NULL,
[StartDate] [datetime] NOT NULL CONSTRAINT [DF_BillOfMaterials_StartDate] DEFAULT (getdate()),
[EndDate] [datetime] NULL,
[UnitMeasureCode] [nchar] (3) NOT NULL,
[BOMLevel] [smallint] NOT NULL,
[PerAssemblyQty] [decimal] (8, 2) NOT NULL CONSTRAINT [DF_BillOfMaterials_PerAssemblyQty] DEFAULT ((1.00)),
[ModifiedDate] [datetime] NOT NULL CONSTRAINT [DF_BillOfMaterials_ModifiedDate] DEFAULT (getdate())
)
GO
PRINT N'Creating index [AK_BillOfMaterials_ProductAssemblyID_ComponentID_StartDate] on [Production].[BillOfMaterials]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'AK_BillOfMaterials_ProductAssemblyID_ComponentID_StartDate' AND object_id = OBJECT_ID(N'[Production].[BillOfMaterials]'))
CREATE UNIQUE CLUSTERED INDEX [AK_BillOfMaterials_ProductAssemblyID_ComponentID_StartDate] ON [Production].[BillOfMaterials] ([ProductAssemblyID], [ComponentID], [StartDate])
GO
PRINT N'Creating primary key [PK_BillOfMaterials_BillOfMaterialsID] on [Production].[BillOfMaterials]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[Production].[PK_BillOfMaterials_BillOfMaterialsID]', 'PK') AND parent_object_id = OBJECT_ID(N'[Production].[BillOfMaterials]', 'U'))
ALTER TABLE [Production].[BillOfMaterials] ADD CONSTRAINT [PK_BillOfMaterials_BillOfMaterialsID] PRIMARY KEY NONCLUSTERED  ([BillOfMaterialsID])
GO
PRINT N'Creating index [IX_BillOfMaterials_UnitMeasureCode] on [Production].[BillOfMaterials]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_BillOfMaterials_UnitMeasureCode' AND object_id = OBJECT_ID(N'[Production].[BillOfMaterials]'))
CREATE NONCLUSTERED INDEX [IX_BillOfMaterials_UnitMeasureCode] ON [Production].[BillOfMaterials] ([UnitMeasureCode])
GO
PRINT N'Creating [Production].[UnitMeasure]'
GO
IF OBJECT_ID(N'[Production].[UnitMeasure]', 'U') IS NULL
CREATE TABLE [Production].[UnitMeasure]
(
[UnitMeasureCode] [nchar] (3) NOT NULL,
[Name] [dbo].[Name] NOT NULL,
[ModifiedDate] [datetime] NOT NULL CONSTRAINT [DF_UnitMeasure_ModifiedDate] DEFAULT (getdate())
)
GO
PRINT N'Creating primary key [PK_UnitMeasure_UnitMeasureCode] on [Production].[UnitMeasure]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[Production].[PK_UnitMeasure_UnitMeasureCode]', 'PK') AND parent_object_id = OBJECT_ID(N'[Production].[UnitMeasure]', 'U'))
ALTER TABLE [Production].[UnitMeasure] ADD CONSTRAINT [PK_UnitMeasure_UnitMeasureCode] PRIMARY KEY CLUSTERED  ([UnitMeasureCode])
GO
PRINT N'Creating index [AK_UnitMeasure_Name] on [Production].[UnitMeasure]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'AK_UnitMeasure_Name' AND object_id = OBJECT_ID(N'[Production].[UnitMeasure]'))
CREATE UNIQUE NONCLUSTERED INDEX [AK_UnitMeasure_Name] ON [Production].[UnitMeasure] ([Name])
GO
PRINT N'Creating [Person].[BusinessEntityAddress]'
GO
IF OBJECT_ID(N'[Person].[BusinessEntityAddress]', 'U') IS NULL
CREATE TABLE [Person].[BusinessEntityAddress]
(
[BusinessEntityID] [int] NOT NULL,
[AddressID] [int] NOT NULL,
[AddressTypeID] [int] NOT NULL,
[rowguid] [uniqueidentifier] NOT NULL ROWGUIDCOL CONSTRAINT [DF_BusinessEntityAddress_rowguid] DEFAULT (newid()),
[ModifiedDate] [datetime] NOT NULL CONSTRAINT [DF_BusinessEntityAddress_ModifiedDate] DEFAULT (getdate())
)
GO
PRINT N'Creating primary key [PK_BusinessEntityAddress_BusinessEntityID_AddressID_AddressTypeID] on [Person].[BusinessEntityAddress]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[Person].[PK_BusinessEntityAddress_BusinessEntityID_AddressID_AddressTypeID]', 'PK') AND parent_object_id = OBJECT_ID(N'[Person].[BusinessEntityAddress]', 'U'))
ALTER TABLE [Person].[BusinessEntityAddress] ADD CONSTRAINT [PK_BusinessEntityAddress_BusinessEntityID_AddressID_AddressTypeID] PRIMARY KEY CLUSTERED  ([BusinessEntityID], [AddressID], [AddressTypeID])
GO
PRINT N'Creating index [IX_BusinessEntityAddress_AddressID] on [Person].[BusinessEntityAddress]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_BusinessEntityAddress_AddressID' AND object_id = OBJECT_ID(N'[Person].[BusinessEntityAddress]'))
CREATE NONCLUSTERED INDEX [IX_BusinessEntityAddress_AddressID] ON [Person].[BusinessEntityAddress] ([AddressID])
GO
PRINT N'Creating index [IX_BusinessEntityAddress_AddressTypeID] on [Person].[BusinessEntityAddress]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_BusinessEntityAddress_AddressTypeID' AND object_id = OBJECT_ID(N'[Person].[BusinessEntityAddress]'))
CREATE NONCLUSTERED INDEX [IX_BusinessEntityAddress_AddressTypeID] ON [Person].[BusinessEntityAddress] ([AddressTypeID])
GO
PRINT N'Creating index [AK_BusinessEntityAddress_rowguid] on [Person].[BusinessEntityAddress]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'AK_BusinessEntityAddress_rowguid' AND object_id = OBJECT_ID(N'[Person].[BusinessEntityAddress]'))
CREATE UNIQUE NONCLUSTERED INDEX [AK_BusinessEntityAddress_rowguid] ON [Person].[BusinessEntityAddress] ([rowguid])
GO
PRINT N'Creating [Person].[AddressType]'
GO
IF OBJECT_ID(N'[Person].[AddressType]', 'U') IS NULL
CREATE TABLE [Person].[AddressType]
(
[AddressTypeID] [int] NOT NULL IDENTITY(1, 1),
[Name] [dbo].[Name] NOT NULL,
[rowguid] [uniqueidentifier] NOT NULL ROWGUIDCOL CONSTRAINT [DF_AddressType_rowguid] DEFAULT (newid()),
[ModifiedDate] [datetime] NOT NULL CONSTRAINT [DF_AddressType_ModifiedDate] DEFAULT (getdate())
)
GO
PRINT N'Creating primary key [PK_AddressType_AddressTypeID] on [Person].[AddressType]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[Person].[PK_AddressType_AddressTypeID]', 'PK') AND parent_object_id = OBJECT_ID(N'[Person].[AddressType]', 'U'))
ALTER TABLE [Person].[AddressType] ADD CONSTRAINT [PK_AddressType_AddressTypeID] PRIMARY KEY CLUSTERED  ([AddressTypeID])
GO
PRINT N'Creating index [AK_AddressType_Name] on [Person].[AddressType]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'AK_AddressType_Name' AND object_id = OBJECT_ID(N'[Person].[AddressType]'))
CREATE UNIQUE NONCLUSTERED INDEX [AK_AddressType_Name] ON [Person].[AddressType] ([Name])
GO
PRINT N'Creating index [AK_AddressType_rowguid] on [Person].[AddressType]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'AK_AddressType_rowguid' AND object_id = OBJECT_ID(N'[Person].[AddressType]'))
CREATE UNIQUE NONCLUSTERED INDEX [AK_AddressType_rowguid] ON [Person].[AddressType] ([rowguid])
GO
PRINT N'Creating [Person].[BusinessEntity]'
GO
IF OBJECT_ID(N'[Person].[BusinessEntity]', 'U') IS NULL
CREATE TABLE [Person].[BusinessEntity]
(
[BusinessEntityID] [int] NOT NULL IDENTITY(1, 1) NOT FOR REPLICATION,
[rowguid] [uniqueidentifier] NOT NULL ROWGUIDCOL CONSTRAINT [DF_BusinessEntity_rowguid] DEFAULT (newid()),
[ModifiedDate] [datetime] NOT NULL CONSTRAINT [DF_BusinessEntity_ModifiedDate] DEFAULT (getdate())
)
GO
PRINT N'Creating primary key [PK_BusinessEntity_BusinessEntityID] on [Person].[BusinessEntity]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[Person].[PK_BusinessEntity_BusinessEntityID]', 'PK') AND parent_object_id = OBJECT_ID(N'[Person].[BusinessEntity]', 'U'))
ALTER TABLE [Person].[BusinessEntity] ADD CONSTRAINT [PK_BusinessEntity_BusinessEntityID] PRIMARY KEY CLUSTERED  ([BusinessEntityID])
GO
PRINT N'Creating index [AK_BusinessEntity_rowguid] on [Person].[BusinessEntity]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'AK_BusinessEntity_rowguid' AND object_id = OBJECT_ID(N'[Person].[BusinessEntity]'))
CREATE UNIQUE NONCLUSTERED INDEX [AK_BusinessEntity_rowguid] ON [Person].[BusinessEntity] ([rowguid])
GO
PRINT N'Creating [Person].[BusinessEntityContact]'
GO
IF OBJECT_ID(N'[Person].[BusinessEntityContact]', 'U') IS NULL
CREATE TABLE [Person].[BusinessEntityContact]
(
[BusinessEntityID] [int] NOT NULL,
[PersonID] [int] NOT NULL,
[ContactTypeID] [int] NOT NULL,
[rowguid] [uniqueidentifier] NOT NULL ROWGUIDCOL CONSTRAINT [DF_BusinessEntityContact_rowguid] DEFAULT (newid()),
[ModifiedDate] [datetime] NOT NULL CONSTRAINT [DF_BusinessEntityContact_ModifiedDate] DEFAULT (getdate())
)
GO
PRINT N'Creating primary key [PK_BusinessEntityContact_BusinessEntityID_PersonID_ContactTypeID] on [Person].[BusinessEntityContact]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[Person].[PK_BusinessEntityContact_BusinessEntityID_PersonID_ContactTypeID]', 'PK') AND parent_object_id = OBJECT_ID(N'[Person].[BusinessEntityContact]', 'U'))
ALTER TABLE [Person].[BusinessEntityContact] ADD CONSTRAINT [PK_BusinessEntityContact_BusinessEntityID_PersonID_ContactTypeID] PRIMARY KEY CLUSTERED  ([BusinessEntityID], [PersonID], [ContactTypeID])
GO
PRINT N'Creating index [IX_BusinessEntityContact_ContactTypeID] on [Person].[BusinessEntityContact]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_BusinessEntityContact_ContactTypeID' AND object_id = OBJECT_ID(N'[Person].[BusinessEntityContact]'))
CREATE NONCLUSTERED INDEX [IX_BusinessEntityContact_ContactTypeID] ON [Person].[BusinessEntityContact] ([ContactTypeID])
GO
PRINT N'Creating index [IX_BusinessEntityContact_PersonID] on [Person].[BusinessEntityContact]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_BusinessEntityContact_PersonID' AND object_id = OBJECT_ID(N'[Person].[BusinessEntityContact]'))
CREATE NONCLUSTERED INDEX [IX_BusinessEntityContact_PersonID] ON [Person].[BusinessEntityContact] ([PersonID])
GO
PRINT N'Creating index [AK_BusinessEntityContact_rowguid] on [Person].[BusinessEntityContact]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'AK_BusinessEntityContact_rowguid' AND object_id = OBJECT_ID(N'[Person].[BusinessEntityContact]'))
CREATE UNIQUE NONCLUSTERED INDEX [AK_BusinessEntityContact_rowguid] ON [Person].[BusinessEntityContact] ([rowguid])
GO
PRINT N'Creating [Person].[ContactType]'
GO
IF OBJECT_ID(N'[Person].[ContactType]', 'U') IS NULL
CREATE TABLE [Person].[ContactType]
(
[ContactTypeID] [int] NOT NULL IDENTITY(1, 1),
[Name] [dbo].[Name] NOT NULL,
[ModifiedDate] [datetime] NOT NULL CONSTRAINT [DF_ContactType_ModifiedDate] DEFAULT (getdate())
)
GO
PRINT N'Creating primary key [PK_ContactType_ContactTypeID] on [Person].[ContactType]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[Person].[PK_ContactType_ContactTypeID]', 'PK') AND parent_object_id = OBJECT_ID(N'[Person].[ContactType]', 'U'))
ALTER TABLE [Person].[ContactType] ADD CONSTRAINT [PK_ContactType_ContactTypeID] PRIMARY KEY CLUSTERED  ([ContactTypeID])
GO
PRINT N'Creating index [AK_ContactType_Name] on [Person].[ContactType]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'AK_ContactType_Name' AND object_id = OBJECT_ID(N'[Person].[ContactType]'))
CREATE UNIQUE NONCLUSTERED INDEX [AK_ContactType_Name] ON [Person].[ContactType] ([Name])
GO
PRINT N'Creating [Person].[CountryRegion]'
GO
IF OBJECT_ID(N'[Person].[CountryRegion]', 'U') IS NULL
CREATE TABLE [Person].[CountryRegion]
(
[CountryRegionCode] [nvarchar] (3) NOT NULL,
[Name] [dbo].[Name] NOT NULL,
[ModifiedDate] [datetime] NOT NULL CONSTRAINT [DF_CountryRegion_ModifiedDate] DEFAULT (getdate())
)
GO
PRINT N'Creating primary key [PK_CountryRegion_CountryRegionCode] on [Person].[CountryRegion]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[Person].[PK_CountryRegion_CountryRegionCode]', 'PK') AND parent_object_id = OBJECT_ID(N'[Person].[CountryRegion]', 'U'))
ALTER TABLE [Person].[CountryRegion] ADD CONSTRAINT [PK_CountryRegion_CountryRegionCode] PRIMARY KEY CLUSTERED  ([CountryRegionCode])
GO
PRINT N'Creating index [AK_CountryRegion_Name] on [Person].[CountryRegion]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'AK_CountryRegion_Name' AND object_id = OBJECT_ID(N'[Person].[CountryRegion]'))
CREATE UNIQUE NONCLUSTERED INDEX [AK_CountryRegion_Name] ON [Person].[CountryRegion] ([Name])
GO
PRINT N'Creating [Sales].[CountryRegionCurrency]'
GO
IF OBJECT_ID(N'[Sales].[CountryRegionCurrency]', 'U') IS NULL
CREATE TABLE [Sales].[CountryRegionCurrency]
(
[CountryRegionCode] [nvarchar] (3) NOT NULL,
[CurrencyCode] [nchar] (3) NOT NULL,
[ModifiedDate] [datetime] NOT NULL CONSTRAINT [DF_CountryRegionCurrency_ModifiedDate] DEFAULT (getdate())
)
GO
PRINT N'Creating primary key [PK_CountryRegionCurrency_CountryRegionCode_CurrencyCode] on [Sales].[CountryRegionCurrency]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[Sales].[PK_CountryRegionCurrency_CountryRegionCode_CurrencyCode]', 'PK') AND parent_object_id = OBJECT_ID(N'[Sales].[CountryRegionCurrency]', 'U'))
ALTER TABLE [Sales].[CountryRegionCurrency] ADD CONSTRAINT [PK_CountryRegionCurrency_CountryRegionCode_CurrencyCode] PRIMARY KEY CLUSTERED  ([CountryRegionCode], [CurrencyCode])
GO
PRINT N'Creating index [IX_CountryRegionCurrency_CurrencyCode] on [Sales].[CountryRegionCurrency]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_CountryRegionCurrency_CurrencyCode' AND object_id = OBJECT_ID(N'[Sales].[CountryRegionCurrency]'))
CREATE NONCLUSTERED INDEX [IX_CountryRegionCurrency_CurrencyCode] ON [Sales].[CountryRegionCurrency] ([CurrencyCode])
GO
PRINT N'Creating [Sales].[Currency]'
GO
IF OBJECT_ID(N'[Sales].[Currency]', 'U') IS NULL
CREATE TABLE [Sales].[Currency]
(
[CurrencyCode] [nchar] (3) NOT NULL,
[Name] [dbo].[Name] NOT NULL,
[ModifiedDate] [datetime] NOT NULL CONSTRAINT [DF_Currency_ModifiedDate] DEFAULT (getdate())
)
GO
PRINT N'Creating primary key [PK_Currency_CurrencyCode] on [Sales].[Currency]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[Sales].[PK_Currency_CurrencyCode]', 'PK') AND parent_object_id = OBJECT_ID(N'[Sales].[Currency]', 'U'))
ALTER TABLE [Sales].[Currency] ADD CONSTRAINT [PK_Currency_CurrencyCode] PRIMARY KEY CLUSTERED  ([CurrencyCode])
GO
PRINT N'Creating index [AK_Currency_Name] on [Sales].[Currency]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'AK_Currency_Name' AND object_id = OBJECT_ID(N'[Sales].[Currency]'))
CREATE UNIQUE NONCLUSTERED INDEX [AK_Currency_Name] ON [Sales].[Currency] ([Name])
GO
PRINT N'Creating [Sales].[CurrencyRate]'
GO
IF OBJECT_ID(N'[Sales].[CurrencyRate]', 'U') IS NULL
CREATE TABLE [Sales].[CurrencyRate]
(
[CurrencyRateID] [int] NOT NULL IDENTITY(1, 1),
[CurrencyRateDate] [datetime] NOT NULL,
[FromCurrencyCode] [nchar] (3) NOT NULL,
[ToCurrencyCode] [nchar] (3) NOT NULL,
[AverageRate] [money] NOT NULL,
[EndOfDayRate] [money] NOT NULL,
[ModifiedDate] [datetime] NOT NULL CONSTRAINT [DF_CurrencyRate_ModifiedDate] DEFAULT (getdate())
)
GO
PRINT N'Creating primary key [PK_CurrencyRate_CurrencyRateID] on [Sales].[CurrencyRate]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[Sales].[PK_CurrencyRate_CurrencyRateID]', 'PK') AND parent_object_id = OBJECT_ID(N'[Sales].[CurrencyRate]', 'U'))
ALTER TABLE [Sales].[CurrencyRate] ADD CONSTRAINT [PK_CurrencyRate_CurrencyRateID] PRIMARY KEY CLUSTERED  ([CurrencyRateID])
GO
PRINT N'Creating index [AK_CurrencyRate_CurrencyRateDate_FromCurrencyCode_ToCurrencyCode] on [Sales].[CurrencyRate]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'AK_CurrencyRate_CurrencyRateDate_FromCurrencyCode_ToCurrencyCode' AND object_id = OBJECT_ID(N'[Sales].[CurrencyRate]'))
CREATE UNIQUE NONCLUSTERED INDEX [AK_CurrencyRate_CurrencyRateDate_FromCurrencyCode_ToCurrencyCode] ON [Sales].[CurrencyRate] ([CurrencyRateDate], [FromCurrencyCode], [ToCurrencyCode])
GO
PRINT N'Creating [Sales].[Store]'
GO
IF OBJECT_ID(N'[Sales].[Store]', 'U') IS NULL
CREATE TABLE [Sales].[Store]
(
[BusinessEntityID] [int] NOT NULL,
[Name] [dbo].[Name] NOT NULL,
[SalesPersonID] [int] NULL,
[Demographics] [xml] (CONTENT [Sales].[StoreSurveySchemaCollection]) NULL,
[rowguid] [uniqueidentifier] NOT NULL ROWGUIDCOL CONSTRAINT [DF_Store_rowguid] DEFAULT (newid()),
[ModifiedDate] [datetime] NOT NULL CONSTRAINT [DF_Store_ModifiedDate] DEFAULT (getdate())
)
GO
PRINT N'Creating primary key [PK_Store_BusinessEntityID] on [Sales].[Store]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[Sales].[PK_Store_BusinessEntityID]', 'PK') AND parent_object_id = OBJECT_ID(N'[Sales].[Store]', 'U'))
ALTER TABLE [Sales].[Store] ADD CONSTRAINT [PK_Store_BusinessEntityID] PRIMARY KEY CLUSTERED  ([BusinessEntityID])
GO
PRINT N'Creating index [AK_Store_rowguid] on [Sales].[Store]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'AK_Store_rowguid' AND object_id = OBJECT_ID(N'[Sales].[Store]'))
CREATE UNIQUE NONCLUSTERED INDEX [AK_Store_rowguid] ON [Sales].[Store] ([rowguid])
GO
PRINT N'Creating index [IX_Store_SalesPersonID] on [Sales].[Store]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Store_SalesPersonID' AND object_id = OBJECT_ID(N'[Sales].[Store]'))
CREATE NONCLUSTERED INDEX [IX_Store_SalesPersonID] ON [Sales].[Store] ([SalesPersonID])
GO
PRINT N'Creating index [PXML_Store_Demographics] on [Sales].[Store]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'PXML_Store_Demographics' AND object_id = OBJECT_ID(N'[Sales].[Store]'))
CREATE PRIMARY XML INDEX [PXML_Store_Demographics]
ON [Sales].[Store] ([Demographics])
GO
PRINT N'Creating [Production].[Document]'
GO
IF OBJECT_ID(N'[Production].[Document]', 'U') IS NULL
CREATE TABLE [Production].[Document]
(
[DocumentNode] [sys].[hierarchyid] NOT NULL,
[DocumentLevel] AS ([DocumentNode].[GetLevel]()),
[Title] [nvarchar] (50) NOT NULL,
[Owner] [int] NOT NULL,
[FolderFlag] [bit] NOT NULL CONSTRAINT [DF_Document_FolderFlag] DEFAULT ((0)),
[FileName] [nvarchar] (400) NOT NULL,
[FileExtension] [nvarchar] (8) NOT NULL,
[Revision] [nchar] (5) NOT NULL,
[ChangeNumber] [int] NOT NULL CONSTRAINT [DF_Document_ChangeNumber] DEFAULT ((0)),
[Status] [tinyint] NOT NULL,
[DocumentSummary] [nvarchar] (max) NULL,
[Document] [varbinary] (max) NULL,
[rowguid] [uniqueidentifier] NOT NULL ROWGUIDCOL CONSTRAINT [DF_Document_rowguid] DEFAULT (newid()),
[ModifiedDate] [datetime] NOT NULL CONSTRAINT [DF_Document_ModifiedDate] DEFAULT (getdate())
)
GO
PRINT N'Creating primary key [PK_Document_DocumentNode] on [Production].[Document]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[Production].[PK_Document_DocumentNode]', 'PK') AND parent_object_id = OBJECT_ID(N'[Production].[Document]', 'U'))
ALTER TABLE [Production].[Document] ADD CONSTRAINT [PK_Document_DocumentNode] PRIMARY KEY CLUSTERED  ([DocumentNode])
GO
PRINT N'Creating index [AK_Document_DocumentLevel_DocumentNode] on [Production].[Document]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'AK_Document_DocumentLevel_DocumentNode' AND object_id = OBJECT_ID(N'[Production].[Document]'))
CREATE UNIQUE NONCLUSTERED INDEX [AK_Document_DocumentLevel_DocumentNode] ON [Production].[Document] ([DocumentLevel], [DocumentNode])
GO
PRINT N'Creating index [IX_Document_FileName_Revision] on [Production].[Document]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Document_FileName_Revision' AND object_id = OBJECT_ID(N'[Production].[Document]'))
CREATE NONCLUSTERED INDEX [IX_Document_FileName_Revision] ON [Production].[Document] ([FileName], [Revision])
GO
PRINT N'Creating index [AK_Document_rowguid] on [Production].[Document]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'AK_Document_rowguid' AND object_id = OBJECT_ID(N'[Production].[Document]'))
CREATE UNIQUE NONCLUSTERED INDEX [AK_Document_rowguid] ON [Production].[Document] ([rowguid])
GO
PRINT N'Adding constraints to [Production].[Document]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[Production].[UQ__Document__F73921F79196878C]', 'UQ') AND parent_object_id = OBJECT_ID(N'[Production].[Document]', 'U'))
ALTER TABLE [Production].[Document] ADD CONSTRAINT [UQ__Document__F73921F79196878C] UNIQUE NONCLUSTERED  ([rowguid])
GO
PRINT N'Adding full text indexing to tables'
GO
IF NOT EXISTS (SELECT 1 FROM sys.fulltext_indexes JOIN sys.fulltext_catalogs ON sys.fulltext_indexes.fulltext_catalog_id = sys.fulltext_catalogs.fulltext_catalog_id WHERE  name = N'AW2014FullTextCatalog' AND object_id = OBJECT_ID(N'[Production].[Document]', 'U'))
CREATE FULLTEXT INDEX ON [Production].[Document] KEY INDEX [PK_Document_DocumentNode] ON [AW2014FullTextCatalog]
GO
PRINT N'Adding full text indexing to columns'
GO
IF NOT EXISTS (SELECT 1 FROM sys.fulltext_index_columns JOIN sys.columns ON fulltext_index_columns.object_id = sys.columns.object_id AND fulltext_index_columns.column_id = sys.columns.column_id WHERE sys.columns.object_id = OBJECT_ID(N'[Production].[Document]') AND sys.columns.name = N'DocumentSummary')
ALTER FULLTEXT INDEX ON [Production].[Document] ADD ([DocumentSummary] LANGUAGE 1033)
GO
IF NOT EXISTS (SELECT 1 FROM sys.fulltext_index_columns JOIN sys.columns ON fulltext_index_columns.object_id = sys.columns.object_id AND fulltext_index_columns.column_id = sys.columns.column_id WHERE sys.columns.object_id = OBJECT_ID(N'[Production].[Document]') AND sys.columns.name = N'Document')
ALTER FULLTEXT INDEX ON [Production].[Document] ADD ([Document] TYPE COLUMN [FileExtension] LANGUAGE 1033)
GO
ALTER FULLTEXT INDEX ON [Production].[Document] ENABLE
GO
PRINT N'Creating [Person].[EmailAddress]'
GO
IF OBJECT_ID(N'[Person].[EmailAddress]', 'U') IS NULL
CREATE TABLE [Person].[EmailAddress]
(
[BusinessEntityID] [int] NOT NULL,
[EmailAddressID] [int] NOT NULL IDENTITY(1, 1),
[EmailAddress] [nvarchar] (50) NULL,
[rowguid] [uniqueidentifier] NOT NULL ROWGUIDCOL CONSTRAINT [DF_EmailAddress_rowguid] DEFAULT (newid()),
[ModifiedDate] [datetime] NOT NULL CONSTRAINT [DF_EmailAddress_ModifiedDate] DEFAULT (getdate())
)
GO
PRINT N'Creating primary key [PK_EmailAddress_BusinessEntityID_EmailAddressID] on [Person].[EmailAddress]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[Person].[PK_EmailAddress_BusinessEntityID_EmailAddressID]', 'PK') AND parent_object_id = OBJECT_ID(N'[Person].[EmailAddress]', 'U'))
ALTER TABLE [Person].[EmailAddress] ADD CONSTRAINT [PK_EmailAddress_BusinessEntityID_EmailAddressID] PRIMARY KEY CLUSTERED  ([BusinessEntityID], [EmailAddressID])
GO
PRINT N'Creating index [IX_EmailAddress_EmailAddress] on [Person].[EmailAddress]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_EmailAddress_EmailAddress' AND object_id = OBJECT_ID(N'[Person].[EmailAddress]'))
CREATE NONCLUSTERED INDEX [IX_EmailAddress_EmailAddress] ON [Person].[EmailAddress] ([EmailAddress])
GO
PRINT N'Creating [HumanResources].[Department]'
GO
IF OBJECT_ID(N'[HumanResources].[Department]', 'U') IS NULL
CREATE TABLE [HumanResources].[Department]
(
[DepartmentID] [smallint] NOT NULL IDENTITY(1, 1),
[Name] [dbo].[Name] NOT NULL,
[GroupName] [dbo].[Name] NOT NULL,
[ModifiedDate] [datetime] NOT NULL CONSTRAINT [DF_Department_ModifiedDate] DEFAULT (getdate())
)
GO
PRINT N'Creating primary key [PK_Department_DepartmentID] on [HumanResources].[Department]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[HumanResources].[PK_Department_DepartmentID]', 'PK') AND parent_object_id = OBJECT_ID(N'[HumanResources].[Department]', 'U'))
ALTER TABLE [HumanResources].[Department] ADD CONSTRAINT [PK_Department_DepartmentID] PRIMARY KEY CLUSTERED  ([DepartmentID])
GO
PRINT N'Creating index [AK_Department_Name] on [HumanResources].[Department]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'AK_Department_Name' AND object_id = OBJECT_ID(N'[HumanResources].[Department]'))
CREATE UNIQUE NONCLUSTERED INDEX [AK_Department_Name] ON [HumanResources].[Department] ([Name])
GO
PRINT N'Creating [HumanResources].[EmployeeDepartmentHistory]'
GO
IF OBJECT_ID(N'[HumanResources].[EmployeeDepartmentHistory]', 'U') IS NULL
CREATE TABLE [HumanResources].[EmployeeDepartmentHistory]
(
[BusinessEntityID] [int] NOT NULL,
[DepartmentID] [smallint] NOT NULL,
[ShiftID] [tinyint] NOT NULL,
[StartDate] [date] NOT NULL,
[EndDate] [date] NULL,
[ModifiedDate] [datetime] NOT NULL CONSTRAINT [DF_EmployeeDepartmentHistory_ModifiedDate] DEFAULT (getdate())
)
GO
PRINT N'Creating primary key [PK_EmployeeDepartmentHistory_BusinessEntityID_StartDate_DepartmentID] on [HumanResources].[EmployeeDepartmentHistory]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[HumanResources].[PK_EmployeeDepartmentHistory_BusinessEntityID_StartDate_DepartmentID]', 'PK') AND parent_object_id = OBJECT_ID(N'[HumanResources].[EmployeeDepartmentHistory]', 'U'))
ALTER TABLE [HumanResources].[EmployeeDepartmentHistory] ADD CONSTRAINT [PK_EmployeeDepartmentHistory_BusinessEntityID_StartDate_DepartmentID] PRIMARY KEY CLUSTERED  ([BusinessEntityID], [StartDate], [DepartmentID], [ShiftID])
GO
PRINT N'Creating index [IX_EmployeeDepartmentHistory_DepartmentID] on [HumanResources].[EmployeeDepartmentHistory]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_EmployeeDepartmentHistory_DepartmentID' AND object_id = OBJECT_ID(N'[HumanResources].[EmployeeDepartmentHistory]'))
CREATE NONCLUSTERED INDEX [IX_EmployeeDepartmentHistory_DepartmentID] ON [HumanResources].[EmployeeDepartmentHistory] ([DepartmentID])
GO
PRINT N'Creating index [IX_EmployeeDepartmentHistory_ShiftID] on [HumanResources].[EmployeeDepartmentHistory]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_EmployeeDepartmentHistory_ShiftID' AND object_id = OBJECT_ID(N'[HumanResources].[EmployeeDepartmentHistory]'))
CREATE NONCLUSTERED INDEX [IX_EmployeeDepartmentHistory_ShiftID] ON [HumanResources].[EmployeeDepartmentHistory] ([ShiftID])
GO
PRINT N'Creating [HumanResources].[Shift]'
GO
IF OBJECT_ID(N'[HumanResources].[Shift]', 'U') IS NULL
CREATE TABLE [HumanResources].[Shift]
(
[ShiftID] [tinyint] NOT NULL IDENTITY(1, 1),
[Name] [dbo].[Name] NOT NULL,
[StartTime] [time] NOT NULL,
[EndTime] [time] NOT NULL,
[ModifiedDate] [datetime] NOT NULL CONSTRAINT [DF_Shift_ModifiedDate] DEFAULT (getdate())
)
GO
PRINT N'Creating primary key [PK_Shift_ShiftID] on [HumanResources].[Shift]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[HumanResources].[PK_Shift_ShiftID]', 'PK') AND parent_object_id = OBJECT_ID(N'[HumanResources].[Shift]', 'U'))
ALTER TABLE [HumanResources].[Shift] ADD CONSTRAINT [PK_Shift_ShiftID] PRIMARY KEY CLUSTERED  ([ShiftID])
GO
PRINT N'Creating index [AK_Shift_Name] on [HumanResources].[Shift]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'AK_Shift_Name' AND object_id = OBJECT_ID(N'[HumanResources].[Shift]'))
CREATE UNIQUE NONCLUSTERED INDEX [AK_Shift_Name] ON [HumanResources].[Shift] ([Name])
GO
PRINT N'Creating index [AK_Shift_StartTime_EndTime] on [HumanResources].[Shift]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'AK_Shift_StartTime_EndTime' AND object_id = OBJECT_ID(N'[HumanResources].[Shift]'))
CREATE UNIQUE NONCLUSTERED INDEX [AK_Shift_StartTime_EndTime] ON [HumanResources].[Shift] ([StartTime], [EndTime])
GO
PRINT N'Creating [HumanResources].[EmployeePayHistory]'
GO
IF OBJECT_ID(N'[HumanResources].[EmployeePayHistory]', 'U') IS NULL
CREATE TABLE [HumanResources].[EmployeePayHistory]
(
[BusinessEntityID] [int] NOT NULL,
[RateChangeDate] [datetime] NOT NULL,
[Rate] [money] NOT NULL,
[PayFrequency] [tinyint] NOT NULL,
[ModifiedDate] [datetime] NOT NULL CONSTRAINT [DF_EmployeePayHistory_ModifiedDate] DEFAULT (getdate())
)
GO
PRINT N'Creating primary key [PK_EmployeePayHistory_BusinessEntityID_RateChangeDate] on [HumanResources].[EmployeePayHistory]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[HumanResources].[PK_EmployeePayHistory_BusinessEntityID_RateChangeDate]', 'PK') AND parent_object_id = OBJECT_ID(N'[HumanResources].[EmployeePayHistory]', 'U'))
ALTER TABLE [HumanResources].[EmployeePayHistory] ADD CONSTRAINT [PK_EmployeePayHistory_BusinessEntityID_RateChangeDate] PRIMARY KEY CLUSTERED  ([BusinessEntityID], [RateChangeDate])
GO
PRINT N'Creating [HumanResources].[JobCandidate]'
GO
IF OBJECT_ID(N'[HumanResources].[JobCandidate]', 'U') IS NULL
CREATE TABLE [HumanResources].[JobCandidate]
(
[JobCandidateID] [int] NOT NULL IDENTITY(1, 1),
[BusinessEntityID] [int] NULL,
[Resume] [xml] (CONTENT [HumanResources].[HRResumeSchemaCollection]) NULL,
[ModifiedDate] [datetime] NOT NULL CONSTRAINT [DF_JobCandidate_ModifiedDate] DEFAULT (getdate())
)
GO
PRINT N'Creating primary key [PK_JobCandidate_JobCandidateID] on [HumanResources].[JobCandidate]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[HumanResources].[PK_JobCandidate_JobCandidateID]', 'PK') AND parent_object_id = OBJECT_ID(N'[HumanResources].[JobCandidate]', 'U'))
ALTER TABLE [HumanResources].[JobCandidate] ADD CONSTRAINT [PK_JobCandidate_JobCandidateID] PRIMARY KEY CLUSTERED  ([JobCandidateID])
GO
PRINT N'Creating index [IX_JobCandidate_BusinessEntityID] on [HumanResources].[JobCandidate]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_JobCandidate_BusinessEntityID' AND object_id = OBJECT_ID(N'[HumanResources].[JobCandidate]'))
CREATE NONCLUSTERED INDEX [IX_JobCandidate_BusinessEntityID] ON [HumanResources].[JobCandidate] ([BusinessEntityID])
GO
PRINT N'Adding full text indexing to tables'
GO
IF NOT EXISTS (SELECT 1 FROM sys.fulltext_indexes JOIN sys.fulltext_catalogs ON sys.fulltext_indexes.fulltext_catalog_id = sys.fulltext_catalogs.fulltext_catalog_id WHERE  name = N'AW2014FullTextCatalog' AND object_id = OBJECT_ID(N'[HumanResources].[JobCandidate]', 'U'))
CREATE FULLTEXT INDEX ON [HumanResources].[JobCandidate] KEY INDEX [PK_JobCandidate_JobCandidateID] ON [AW2014FullTextCatalog]
GO
PRINT N'Adding full text indexing to columns'
GO
IF NOT EXISTS (SELECT 1 FROM sys.fulltext_index_columns JOIN sys.columns ON fulltext_index_columns.object_id = sys.columns.object_id AND fulltext_index_columns.column_id = sys.columns.column_id WHERE sys.columns.object_id = OBJECT_ID(N'[HumanResources].[JobCandidate]') AND sys.columns.name = N'Resume')
ALTER FULLTEXT INDEX ON [HumanResources].[JobCandidate] ADD ([Resume] LANGUAGE 1033)
GO
ALTER FULLTEXT INDEX ON [HumanResources].[JobCandidate] ENABLE
GO
PRINT N'Creating [Person].[Password]'
GO
IF OBJECT_ID(N'[Person].[Password]', 'U') IS NULL
CREATE TABLE [Person].[Password]
(
[BusinessEntityID] [int] NOT NULL,
[PasswordHash] [varchar] (128) NOT NULL,
[PasswordSalt] [varchar] (10) NOT NULL,
[rowguid] [uniqueidentifier] NOT NULL ROWGUIDCOL CONSTRAINT [DF_Password_rowguid] DEFAULT (newid()),
[ModifiedDate] [datetime] NOT NULL CONSTRAINT [DF_Password_ModifiedDate] DEFAULT (getdate())
)
GO
PRINT N'Creating primary key [PK_Password_BusinessEntityID] on [Person].[Password]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[Person].[PK_Password_BusinessEntityID]', 'PK') AND parent_object_id = OBJECT_ID(N'[Person].[Password]', 'U'))
ALTER TABLE [Person].[Password] ADD CONSTRAINT [PK_Password_BusinessEntityID] PRIMARY KEY CLUSTERED  ([BusinessEntityID])
GO
PRINT N'Creating [Sales].[CreditCard]'
GO
IF OBJECT_ID(N'[Sales].[CreditCard]', 'U') IS NULL
CREATE TABLE [Sales].[CreditCard]
(
[CreditCardID] [int] NOT NULL IDENTITY(1, 1),
[CardType] [nvarchar] (50) NOT NULL,
[CardNumber] [nvarchar] (25) NOT NULL,
[ExpMonth] [tinyint] NOT NULL,
[ExpYear] [smallint] NOT NULL,
[ModifiedDate] [datetime] NOT NULL CONSTRAINT [DF_CreditCard_ModifiedDate] DEFAULT (getdate())
)
GO
PRINT N'Creating primary key [PK_CreditCard_CreditCardID] on [Sales].[CreditCard]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[Sales].[PK_CreditCard_CreditCardID]', 'PK') AND parent_object_id = OBJECT_ID(N'[Sales].[CreditCard]', 'U'))
ALTER TABLE [Sales].[CreditCard] ADD CONSTRAINT [PK_CreditCard_CreditCardID] PRIMARY KEY CLUSTERED  ([CreditCardID])
GO
PRINT N'Creating index [AK_CreditCard_CardNumber] on [Sales].[CreditCard]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'AK_CreditCard_CardNumber' AND object_id = OBJECT_ID(N'[Sales].[CreditCard]'))
CREATE UNIQUE NONCLUSTERED INDEX [AK_CreditCard_CardNumber] ON [Sales].[CreditCard] ([CardNumber])
GO
PRINT N'Creating [Sales].[PersonCreditCard]'
GO
IF OBJECT_ID(N'[Sales].[PersonCreditCard]', 'U') IS NULL
CREATE TABLE [Sales].[PersonCreditCard]
(
[BusinessEntityID] [int] NOT NULL,
[CreditCardID] [int] NOT NULL,
[ModifiedDate] [datetime] NOT NULL CONSTRAINT [DF_PersonCreditCard_ModifiedDate] DEFAULT (getdate())
)
GO
PRINT N'Creating primary key [PK_PersonCreditCard_BusinessEntityID_CreditCardID] on [Sales].[PersonCreditCard]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[Sales].[PK_PersonCreditCard_BusinessEntityID_CreditCardID]', 'PK') AND parent_object_id = OBJECT_ID(N'[Sales].[PersonCreditCard]', 'U'))
ALTER TABLE [Sales].[PersonCreditCard] ADD CONSTRAINT [PK_PersonCreditCard_BusinessEntityID_CreditCardID] PRIMARY KEY CLUSTERED  ([BusinessEntityID], [CreditCardID])
GO
PRINT N'Creating [Person].[PersonPhone]'
GO
IF OBJECT_ID(N'[Person].[PersonPhone]', 'U') IS NULL
CREATE TABLE [Person].[PersonPhone]
(
[BusinessEntityID] [int] NOT NULL,
[PhoneNumber] [dbo].[Phone] NOT NULL,
[PhoneNumberTypeID] [int] NOT NULL,
[ModifiedDate] [datetime] NOT NULL CONSTRAINT [DF_PersonPhone_ModifiedDate] DEFAULT (getdate())
)
GO
PRINT N'Creating primary key [PK_PersonPhone_BusinessEntityID_PhoneNumber_PhoneNumberTypeID] on [Person].[PersonPhone]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[Person].[PK_PersonPhone_BusinessEntityID_PhoneNumber_PhoneNumberTypeID]', 'PK') AND parent_object_id = OBJECT_ID(N'[Person].[PersonPhone]', 'U'))
ALTER TABLE [Person].[PersonPhone] ADD CONSTRAINT [PK_PersonPhone_BusinessEntityID_PhoneNumber_PhoneNumberTypeID] PRIMARY KEY CLUSTERED  ([BusinessEntityID], [PhoneNumber], [PhoneNumberTypeID])
GO
PRINT N'Creating index [IX_PersonPhone_PhoneNumber] on [Person].[PersonPhone]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_PersonPhone_PhoneNumber' AND object_id = OBJECT_ID(N'[Person].[PersonPhone]'))
CREATE NONCLUSTERED INDEX [IX_PersonPhone_PhoneNumber] ON [Person].[PersonPhone] ([PhoneNumber])
GO
PRINT N'Creating [Person].[PhoneNumberType]'
GO
IF OBJECT_ID(N'[Person].[PhoneNumberType]', 'U') IS NULL
CREATE TABLE [Person].[PhoneNumberType]
(
[PhoneNumberTypeID] [int] NOT NULL IDENTITY(1, 1),
[Name] [dbo].[Name] NOT NULL,
[ModifiedDate] [datetime] NOT NULL CONSTRAINT [DF_PhoneNumberType_ModifiedDate] DEFAULT (getdate())
)
GO
PRINT N'Creating primary key [PK_PhoneNumberType_PhoneNumberTypeID] on [Person].[PhoneNumberType]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[Person].[PK_PhoneNumberType_PhoneNumberTypeID]', 'PK') AND parent_object_id = OBJECT_ID(N'[Person].[PhoneNumberType]', 'U'))
ALTER TABLE [Person].[PhoneNumberType] ADD CONSTRAINT [PK_PhoneNumberType_PhoneNumberTypeID] PRIMARY KEY CLUSTERED  ([PhoneNumberTypeID])
GO
PRINT N'Creating [Production].[ProductModel]'
GO
IF OBJECT_ID(N'[Production].[ProductModel]', 'U') IS NULL
CREATE TABLE [Production].[ProductModel]
(
[ProductModelID] [int] NOT NULL IDENTITY(1, 1),
[Name] [dbo].[Name] NOT NULL,
[CatalogDescription] [xml] (CONTENT [Production].[ProductDescriptionSchemaCollection]) NULL,
[Instructions] [xml] (CONTENT [Production].[ManuInstructionsSchemaCollection]) NULL,
[rowguid] [uniqueidentifier] NOT NULL ROWGUIDCOL CONSTRAINT [DF_ProductModel_rowguid] DEFAULT (newid()),
[ModifiedDate] [datetime] NOT NULL CONSTRAINT [DF_ProductModel_ModifiedDate] DEFAULT (getdate())
)
GO
PRINT N'Creating primary key [PK_ProductModel_ProductModelID] on [Production].[ProductModel]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[Production].[PK_ProductModel_ProductModelID]', 'PK') AND parent_object_id = OBJECT_ID(N'[Production].[ProductModel]', 'U'))
ALTER TABLE [Production].[ProductModel] ADD CONSTRAINT [PK_ProductModel_ProductModelID] PRIMARY KEY CLUSTERED  ([ProductModelID])
GO
PRINT N'Creating index [AK_ProductModel_Name] on [Production].[ProductModel]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'AK_ProductModel_Name' AND object_id = OBJECT_ID(N'[Production].[ProductModel]'))
CREATE UNIQUE NONCLUSTERED INDEX [AK_ProductModel_Name] ON [Production].[ProductModel] ([Name])
GO
PRINT N'Creating index [AK_ProductModel_rowguid] on [Production].[ProductModel]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'AK_ProductModel_rowguid' AND object_id = OBJECT_ID(N'[Production].[ProductModel]'))
CREATE UNIQUE NONCLUSTERED INDEX [AK_ProductModel_rowguid] ON [Production].[ProductModel] ([rowguid])
GO
PRINT N'Creating index [PXML_ProductModel_CatalogDescription] on [Production].[ProductModel]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'PXML_ProductModel_CatalogDescription' AND object_id = OBJECT_ID(N'[Production].[ProductModel]'))
CREATE PRIMARY XML INDEX [PXML_ProductModel_CatalogDescription]
ON [Production].[ProductModel] ([CatalogDescription])
GO
PRINT N'Creating index [PXML_ProductModel_Instructions] on [Production].[ProductModel]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'PXML_ProductModel_Instructions' AND object_id = OBJECT_ID(N'[Production].[ProductModel]'))
CREATE PRIMARY XML INDEX [PXML_ProductModel_Instructions]
ON [Production].[ProductModel] ([Instructions])
GO
PRINT N'Creating [Production].[ProductSubcategory]'
GO
IF OBJECT_ID(N'[Production].[ProductSubcategory]', 'U') IS NULL
CREATE TABLE [Production].[ProductSubcategory]
(
[ProductSubcategoryID] [int] NOT NULL IDENTITY(1, 1),
[ProductCategoryID] [int] NOT NULL,
[Name] [dbo].[Name] NOT NULL,
[rowguid] [uniqueidentifier] NOT NULL ROWGUIDCOL CONSTRAINT [DF_ProductSubcategory_rowguid] DEFAULT (newid()),
[ModifiedDate] [datetime] NOT NULL CONSTRAINT [DF_ProductSubcategory_ModifiedDate] DEFAULT (getdate())
)
GO
PRINT N'Creating primary key [PK_ProductSubcategory_ProductSubcategoryID] on [Production].[ProductSubcategory]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[Production].[PK_ProductSubcategory_ProductSubcategoryID]', 'PK') AND parent_object_id = OBJECT_ID(N'[Production].[ProductSubcategory]', 'U'))
ALTER TABLE [Production].[ProductSubcategory] ADD CONSTRAINT [PK_ProductSubcategory_ProductSubcategoryID] PRIMARY KEY CLUSTERED  ([ProductSubcategoryID])
GO
PRINT N'Creating index [AK_ProductSubcategory_Name] on [Production].[ProductSubcategory]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'AK_ProductSubcategory_Name' AND object_id = OBJECT_ID(N'[Production].[ProductSubcategory]'))
CREATE UNIQUE NONCLUSTERED INDEX [AK_ProductSubcategory_Name] ON [Production].[ProductSubcategory] ([Name])
GO
PRINT N'Creating index [AK_ProductSubcategory_rowguid] on [Production].[ProductSubcategory]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'AK_ProductSubcategory_rowguid' AND object_id = OBJECT_ID(N'[Production].[ProductSubcategory]'))
CREATE UNIQUE NONCLUSTERED INDEX [AK_ProductSubcategory_rowguid] ON [Production].[ProductSubcategory] ([rowguid])
GO
PRINT N'Creating [Production].[ProductCostHistory]'
GO
IF OBJECT_ID(N'[Production].[ProductCostHistory]', 'U') IS NULL
CREATE TABLE [Production].[ProductCostHistory]
(
[ProductID] [int] NOT NULL,
[StartDate] [datetime] NOT NULL,
[EndDate] [datetime] NULL,
[StandardCost] [money] NOT NULL,
[ModifiedDate] [datetime] NOT NULL CONSTRAINT [DF_ProductCostHistory_ModifiedDate] DEFAULT (getdate())
)
GO
PRINT N'Creating primary key [PK_ProductCostHistory_ProductID_StartDate] on [Production].[ProductCostHistory]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[Production].[PK_ProductCostHistory_ProductID_StartDate]', 'PK') AND parent_object_id = OBJECT_ID(N'[Production].[ProductCostHistory]', 'U'))
ALTER TABLE [Production].[ProductCostHistory] ADD CONSTRAINT [PK_ProductCostHistory_ProductID_StartDate] PRIMARY KEY CLUSTERED  ([ProductID], [StartDate])
GO
PRINT N'Creating [Production].[ProductDocument]'
GO
IF OBJECT_ID(N'[Production].[ProductDocument]', 'U') IS NULL
CREATE TABLE [Production].[ProductDocument]
(
[ProductID] [int] NOT NULL,
[DocumentNode] [sys].[hierarchyid] NOT NULL,
[ModifiedDate] [datetime] NOT NULL CONSTRAINT [DF_ProductDocument_ModifiedDate] DEFAULT (getdate())
)
GO
PRINT N'Creating primary key [PK_ProductDocument_ProductID_DocumentNode] on [Production].[ProductDocument]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[Production].[PK_ProductDocument_ProductID_DocumentNode]', 'PK') AND parent_object_id = OBJECT_ID(N'[Production].[ProductDocument]', 'U'))
ALTER TABLE [Production].[ProductDocument] ADD CONSTRAINT [PK_ProductDocument_ProductID_DocumentNode] PRIMARY KEY CLUSTERED  ([ProductID], [DocumentNode])
GO
PRINT N'Creating [Production].[Location]'
GO
IF OBJECT_ID(N'[Production].[Location]', 'U') IS NULL
CREATE TABLE [Production].[Location]
(
[LocationID] [smallint] NOT NULL IDENTITY(1, 1),
[Name] [dbo].[Name] NOT NULL,
[CostRate] [smallmoney] NOT NULL CONSTRAINT [DF_Location_CostRate] DEFAULT ((0.00)),
[Availability] [decimal] (8, 2) NOT NULL CONSTRAINT [DF_Location_Availability] DEFAULT ((0.00)),
[ModifiedDate] [datetime] NOT NULL CONSTRAINT [DF_Location_ModifiedDate] DEFAULT (getdate())
)
GO
PRINT N'Creating primary key [PK_Location_LocationID] on [Production].[Location]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[Production].[PK_Location_LocationID]', 'PK') AND parent_object_id = OBJECT_ID(N'[Production].[Location]', 'U'))
ALTER TABLE [Production].[Location] ADD CONSTRAINT [PK_Location_LocationID] PRIMARY KEY CLUSTERED  ([LocationID])
GO
PRINT N'Creating index [AK_Location_Name] on [Production].[Location]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'AK_Location_Name' AND object_id = OBJECT_ID(N'[Production].[Location]'))
CREATE UNIQUE NONCLUSTERED INDEX [AK_Location_Name] ON [Production].[Location] ([Name])
GO
PRINT N'Creating [Production].[ProductInventory]'
GO
IF OBJECT_ID(N'[Production].[ProductInventory]', 'U') IS NULL
CREATE TABLE [Production].[ProductInventory]
(
[ProductID] [int] NOT NULL,
[LocationID] [smallint] NOT NULL,
[Shelf] [nvarchar] (10) NOT NULL,
[Bin] [tinyint] NOT NULL,
[Quantity] [smallint] NOT NULL CONSTRAINT [DF_ProductInventory_Quantity] DEFAULT ((0)),
[rowguid] [uniqueidentifier] NOT NULL ROWGUIDCOL CONSTRAINT [DF_ProductInventory_rowguid] DEFAULT (newid()),
[ModifiedDate] [datetime] NOT NULL CONSTRAINT [DF_ProductInventory_ModifiedDate] DEFAULT (getdate())
)
GO
PRINT N'Creating primary key [PK_ProductInventory_ProductID_LocationID] on [Production].[ProductInventory]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[Production].[PK_ProductInventory_ProductID_LocationID]', 'PK') AND parent_object_id = OBJECT_ID(N'[Production].[ProductInventory]', 'U'))
ALTER TABLE [Production].[ProductInventory] ADD CONSTRAINT [PK_ProductInventory_ProductID_LocationID] PRIMARY KEY CLUSTERED  ([ProductID], [LocationID])
GO
PRINT N'Creating [Production].[ProductListPriceHistory]'
GO
IF OBJECT_ID(N'[Production].[ProductListPriceHistory]', 'U') IS NULL
CREATE TABLE [Production].[ProductListPriceHistory]
(
[ProductID] [int] NOT NULL,
[StartDate] [datetime] NOT NULL,
[EndDate] [datetime] NULL,
[ListPrice] [money] NOT NULL,
[ModifiedDate] [datetime] NOT NULL CONSTRAINT [DF_ProductListPriceHistory_ModifiedDate] DEFAULT (getdate())
)
GO
PRINT N'Creating primary key [PK_ProductListPriceHistory_ProductID_StartDate] on [Production].[ProductListPriceHistory]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[Production].[PK_ProductListPriceHistory_ProductID_StartDate]', 'PK') AND parent_object_id = OBJECT_ID(N'[Production].[ProductListPriceHistory]', 'U'))
ALTER TABLE [Production].[ProductListPriceHistory] ADD CONSTRAINT [PK_ProductListPriceHistory_ProductID_StartDate] PRIMARY KEY CLUSTERED  ([ProductID], [StartDate])
GO
PRINT N'Creating [Production].[Illustration]'
GO
IF OBJECT_ID(N'[Production].[Illustration]', 'U') IS NULL
CREATE TABLE [Production].[Illustration]
(
[IllustrationID] [int] NOT NULL IDENTITY(1, 1),
[Diagram] [xml] NULL,
[ModifiedDate] [datetime] NOT NULL CONSTRAINT [DF_Illustration_ModifiedDate] DEFAULT (getdate())
)
GO
PRINT N'Creating primary key [PK_Illustration_IllustrationID] on [Production].[Illustration]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[Production].[PK_Illustration_IllustrationID]', 'PK') AND parent_object_id = OBJECT_ID(N'[Production].[Illustration]', 'U'))
ALTER TABLE [Production].[Illustration] ADD CONSTRAINT [PK_Illustration_IllustrationID] PRIMARY KEY CLUSTERED  ([IllustrationID])
GO
PRINT N'Creating [Production].[ProductModelIllustration]'
GO
IF OBJECT_ID(N'[Production].[ProductModelIllustration]', 'U') IS NULL
CREATE TABLE [Production].[ProductModelIllustration]
(
[ProductModelID] [int] NOT NULL,
[IllustrationID] [int] NOT NULL,
[ModifiedDate] [datetime] NOT NULL CONSTRAINT [DF_ProductModelIllustration_ModifiedDate] DEFAULT (getdate())
)
GO
PRINT N'Creating primary key [PK_ProductModelIllustration_ProductModelID_IllustrationID] on [Production].[ProductModelIllustration]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[Production].[PK_ProductModelIllustration_ProductModelID_IllustrationID]', 'PK') AND parent_object_id = OBJECT_ID(N'[Production].[ProductModelIllustration]', 'U'))
ALTER TABLE [Production].[ProductModelIllustration] ADD CONSTRAINT [PK_ProductModelIllustration_ProductModelID_IllustrationID] PRIMARY KEY CLUSTERED  ([ProductModelID], [IllustrationID])
GO
PRINT N'Creating [Production].[Culture]'
GO
IF OBJECT_ID(N'[Production].[Culture]', 'U') IS NULL
CREATE TABLE [Production].[Culture]
(
[CultureID] [nchar] (6) NOT NULL,
[Name] [dbo].[Name] NOT NULL,
[ModifiedDate] [datetime] NOT NULL CONSTRAINT [DF_Culture_ModifiedDate] DEFAULT (getdate())
)
GO
PRINT N'Creating primary key [PK_Culture_CultureID] on [Production].[Culture]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[Production].[PK_Culture_CultureID]', 'PK') AND parent_object_id = OBJECT_ID(N'[Production].[Culture]', 'U'))
ALTER TABLE [Production].[Culture] ADD CONSTRAINT [PK_Culture_CultureID] PRIMARY KEY CLUSTERED  ([CultureID])
GO
PRINT N'Creating index [AK_Culture_Name] on [Production].[Culture]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'AK_Culture_Name' AND object_id = OBJECT_ID(N'[Production].[Culture]'))
CREATE UNIQUE NONCLUSTERED INDEX [AK_Culture_Name] ON [Production].[Culture] ([Name])
GO
PRINT N'Creating [Production].[ProductModelProductDescriptionCulture]'
GO
IF OBJECT_ID(N'[Production].[ProductModelProductDescriptionCulture]', 'U') IS NULL
CREATE TABLE [Production].[ProductModelProductDescriptionCulture]
(
[ProductModelID] [int] NOT NULL,
[ProductDescriptionID] [int] NOT NULL,
[CultureID] [nchar] (6) NOT NULL,
[ModifiedDate] [datetime] NOT NULL CONSTRAINT [DF_ProductModelProductDescriptionCulture_ModifiedDate] DEFAULT (getdate())
)
GO
PRINT N'Creating primary key [PK_ProductModelProductDescriptionCulture_ProductModelID_ProductDescriptionID_CultureID] on [Production].[ProductModelProductDescriptionCulture]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[Production].[PK_ProductModelProductDescriptionCulture_ProductModelID_ProductDescriptionID_CultureID]', 'PK') AND parent_object_id = OBJECT_ID(N'[Production].[ProductModelProductDescriptionCulture]', 'U'))
ALTER TABLE [Production].[ProductModelProductDescriptionCulture] ADD CONSTRAINT [PK_ProductModelProductDescriptionCulture_ProductModelID_ProductDescriptionID_CultureID] PRIMARY KEY CLUSTERED  ([ProductModelID], [ProductDescriptionID], [CultureID])
GO
PRINT N'Creating [Production].[ProductDescription]'
GO
IF OBJECT_ID(N'[Production].[ProductDescription]', 'U') IS NULL
CREATE TABLE [Production].[ProductDescription]
(
[ProductDescriptionID] [int] NOT NULL IDENTITY(1, 1),
[Description] [nvarchar] (400) NOT NULL,
[rowguid] [uniqueidentifier] NOT NULL ROWGUIDCOL CONSTRAINT [DF_ProductDescription_rowguid] DEFAULT (newid()),
[ModifiedDate] [datetime] NOT NULL CONSTRAINT [DF_ProductDescription_ModifiedDate] DEFAULT (getdate())
)
GO
PRINT N'Creating primary key [PK_ProductDescription_ProductDescriptionID] on [Production].[ProductDescription]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[Production].[PK_ProductDescription_ProductDescriptionID]', 'PK') AND parent_object_id = OBJECT_ID(N'[Production].[ProductDescription]', 'U'))
ALTER TABLE [Production].[ProductDescription] ADD CONSTRAINT [PK_ProductDescription_ProductDescriptionID] PRIMARY KEY CLUSTERED  ([ProductDescriptionID])
GO
PRINT N'Creating index [AK_ProductDescription_rowguid] on [Production].[ProductDescription]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'AK_ProductDescription_rowguid' AND object_id = OBJECT_ID(N'[Production].[ProductDescription]'))
CREATE UNIQUE NONCLUSTERED INDEX [AK_ProductDescription_rowguid] ON [Production].[ProductDescription] ([rowguid])
GO
PRINT N'Creating [Production].[ProductProductPhoto]'
GO
IF OBJECT_ID(N'[Production].[ProductProductPhoto]', 'U') IS NULL
CREATE TABLE [Production].[ProductProductPhoto]
(
[ProductID] [int] NOT NULL,
[ProductPhotoID] [int] NOT NULL,
[Primary] [dbo].[Flag] NOT NULL CONSTRAINT [DF_ProductProductPhoto_Primary] DEFAULT ((0)),
[ModifiedDate] [datetime] NOT NULL CONSTRAINT [DF_ProductProductPhoto_ModifiedDate] DEFAULT (getdate())
)
GO
PRINT N'Creating primary key [PK_ProductProductPhoto_ProductID_ProductPhotoID] on [Production].[ProductProductPhoto]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[Production].[PK_ProductProductPhoto_ProductID_ProductPhotoID]', 'PK') AND parent_object_id = OBJECT_ID(N'[Production].[ProductProductPhoto]', 'U'))
ALTER TABLE [Production].[ProductProductPhoto] ADD CONSTRAINT [PK_ProductProductPhoto_ProductID_ProductPhotoID] PRIMARY KEY NONCLUSTERED  ([ProductID], [ProductPhotoID])
GO
PRINT N'Creating [Production].[ProductPhoto]'
GO
IF OBJECT_ID(N'[Production].[ProductPhoto]', 'U') IS NULL
CREATE TABLE [Production].[ProductPhoto]
(
[ProductPhotoID] [int] NOT NULL IDENTITY(1, 1),
[ThumbNailPhoto] [varbinary] (max) NULL,
[ThumbnailPhotoFileName] [nvarchar] (50) NULL,
[LargePhoto] [varbinary] (max) NULL,
[LargePhotoFileName] [nvarchar] (50) NULL,
[ModifiedDate] [datetime] NOT NULL CONSTRAINT [DF_ProductPhoto_ModifiedDate] DEFAULT (getdate())
)
GO
PRINT N'Creating primary key [PK_ProductPhoto_ProductPhotoID] on [Production].[ProductPhoto]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[Production].[PK_ProductPhoto_ProductPhotoID]', 'PK') AND parent_object_id = OBJECT_ID(N'[Production].[ProductPhoto]', 'U'))
ALTER TABLE [Production].[ProductPhoto] ADD CONSTRAINT [PK_ProductPhoto_ProductPhotoID] PRIMARY KEY CLUSTERED  ([ProductPhotoID])
GO
PRINT N'Creating [Production].[ProductReview]'
GO
IF OBJECT_ID(N'[Production].[ProductReview]', 'U') IS NULL
CREATE TABLE [Production].[ProductReview]
(
[ProductReviewID] [int] NOT NULL IDENTITY(1, 1),
[ProductID] [int] NOT NULL,
[ReviewerName] [dbo].[Name] NOT NULL,
[ReviewDate] [datetime] NOT NULL CONSTRAINT [DF_ProductReview_ReviewDate] DEFAULT (getdate()),
[EmailAddress] [nvarchar] (50) NOT NULL,
[Rating] [int] NOT NULL,
[Comments] [nvarchar] (3850) NULL,
[ModifiedDate] [datetime] NOT NULL CONSTRAINT [DF_ProductReview_ModifiedDate] DEFAULT (getdate())
)
GO
PRINT N'Creating primary key [PK_ProductReview_ProductReviewID] on [Production].[ProductReview]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[Production].[PK_ProductReview_ProductReviewID]', 'PK') AND parent_object_id = OBJECT_ID(N'[Production].[ProductReview]', 'U'))
ALTER TABLE [Production].[ProductReview] ADD CONSTRAINT [PK_ProductReview_ProductReviewID] PRIMARY KEY CLUSTERED  ([ProductReviewID])
GO
PRINT N'Creating index [IX_ProductReview_ProductID_Name] on [Production].[ProductReview]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ProductReview_ProductID_Name' AND object_id = OBJECT_ID(N'[Production].[ProductReview]'))
CREATE NONCLUSTERED INDEX [IX_ProductReview_ProductID_Name] ON [Production].[ProductReview] ([ProductID], [ReviewerName]) INCLUDE ([Comments])
GO
PRINT N'Adding full text indexing to tables'
GO
IF NOT EXISTS (SELECT 1 FROM sys.fulltext_indexes JOIN sys.fulltext_catalogs ON sys.fulltext_indexes.fulltext_catalog_id = sys.fulltext_catalogs.fulltext_catalog_id WHERE  name = N'AW2014FullTextCatalog' AND object_id = OBJECT_ID(N'[Production].[ProductReview]', 'U'))
CREATE FULLTEXT INDEX ON [Production].[ProductReview] KEY INDEX [PK_ProductReview_ProductReviewID] ON [AW2014FullTextCatalog]
GO
PRINT N'Adding full text indexing to columns'
GO
IF NOT EXISTS (SELECT 1 FROM sys.fulltext_index_columns JOIN sys.columns ON fulltext_index_columns.object_id = sys.columns.object_id AND fulltext_index_columns.column_id = sys.columns.column_id WHERE sys.columns.object_id = OBJECT_ID(N'[Production].[ProductReview]') AND sys.columns.name = N'Comments')
ALTER FULLTEXT INDEX ON [Production].[ProductReview] ADD ([Comments] LANGUAGE 1033)
GO
ALTER FULLTEXT INDEX ON [Production].[ProductReview] ENABLE
GO
PRINT N'Creating [Production].[ProductCategory]'
GO
IF OBJECT_ID(N'[Production].[ProductCategory]', 'U') IS NULL
CREATE TABLE [Production].[ProductCategory]
(
[ProductCategoryID] [int] NOT NULL IDENTITY(1, 1),
[Name] [dbo].[Name] NOT NULL,
[rowguid] [uniqueidentifier] NOT NULL ROWGUIDCOL CONSTRAINT [DF_ProductCategory_rowguid] DEFAULT (newid()),
[ModifiedDate] [datetime] NOT NULL CONSTRAINT [DF_ProductCategory_ModifiedDate] DEFAULT (getdate())
)
GO
PRINT N'Creating primary key [PK_ProductCategory_ProductCategoryID] on [Production].[ProductCategory]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[Production].[PK_ProductCategory_ProductCategoryID]', 'PK') AND parent_object_id = OBJECT_ID(N'[Production].[ProductCategory]', 'U'))
ALTER TABLE [Production].[ProductCategory] ADD CONSTRAINT [PK_ProductCategory_ProductCategoryID] PRIMARY KEY CLUSTERED  ([ProductCategoryID])
GO
PRINT N'Creating index [AK_ProductCategory_Name] on [Production].[ProductCategory]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'AK_ProductCategory_Name' AND object_id = OBJECT_ID(N'[Production].[ProductCategory]'))
CREATE UNIQUE NONCLUSTERED INDEX [AK_ProductCategory_Name] ON [Production].[ProductCategory] ([Name])
GO
PRINT N'Creating index [AK_ProductCategory_rowguid] on [Production].[ProductCategory]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'AK_ProductCategory_rowguid' AND object_id = OBJECT_ID(N'[Production].[ProductCategory]'))
CREATE UNIQUE NONCLUSTERED INDEX [AK_ProductCategory_rowguid] ON [Production].[ProductCategory] ([rowguid])
GO
PRINT N'Creating [Purchasing].[ProductVendor]'
GO
IF OBJECT_ID(N'[Purchasing].[ProductVendor]', 'U') IS NULL
CREATE TABLE [Purchasing].[ProductVendor]
(
[ProductID] [int] NOT NULL,
[BusinessEntityID] [int] NOT NULL,
[AverageLeadTime] [int] NOT NULL,
[StandardPrice] [money] NOT NULL,
[LastReceiptCost] [money] NULL,
[LastReceiptDate] [datetime] NULL,
[MinOrderQty] [int] NOT NULL,
[MaxOrderQty] [int] NOT NULL,
[OnOrderQty] [int] NULL,
[UnitMeasureCode] [nchar] (3) NOT NULL,
[ModifiedDate] [datetime] NOT NULL CONSTRAINT [DF_ProductVendor_ModifiedDate] DEFAULT (getdate())
)
GO
PRINT N'Creating primary key [PK_ProductVendor_ProductID_BusinessEntityID] on [Purchasing].[ProductVendor]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[Purchasing].[PK_ProductVendor_ProductID_BusinessEntityID]', 'PK') AND parent_object_id = OBJECT_ID(N'[Purchasing].[ProductVendor]', 'U'))
ALTER TABLE [Purchasing].[ProductVendor] ADD CONSTRAINT [PK_ProductVendor_ProductID_BusinessEntityID] PRIMARY KEY CLUSTERED  ([ProductID], [BusinessEntityID])
GO
PRINT N'Creating index [IX_ProductVendor_BusinessEntityID] on [Purchasing].[ProductVendor]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ProductVendor_BusinessEntityID' AND object_id = OBJECT_ID(N'[Purchasing].[ProductVendor]'))
CREATE NONCLUSTERED INDEX [IX_ProductVendor_BusinessEntityID] ON [Purchasing].[ProductVendor] ([BusinessEntityID])
GO
PRINT N'Creating index [IX_ProductVendor_UnitMeasureCode] on [Purchasing].[ProductVendor]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ProductVendor_UnitMeasureCode' AND object_id = OBJECT_ID(N'[Purchasing].[ProductVendor]'))
CREATE NONCLUSTERED INDEX [IX_ProductVendor_UnitMeasureCode] ON [Purchasing].[ProductVendor] ([UnitMeasureCode])
GO
PRINT N'Creating [Purchasing].[ShipMethod]'
GO
IF OBJECT_ID(N'[Purchasing].[ShipMethod]', 'U') IS NULL
CREATE TABLE [Purchasing].[ShipMethod]
(
[ShipMethodID] [int] NOT NULL IDENTITY(1, 1),
[Name] [dbo].[Name] NOT NULL,
[ShipBase] [money] NOT NULL CONSTRAINT [DF_ShipMethod_ShipBase] DEFAULT ((0.00)),
[ShipRate] [money] NOT NULL CONSTRAINT [DF_ShipMethod_ShipRate] DEFAULT ((0.00)),
[rowguid] [uniqueidentifier] NOT NULL ROWGUIDCOL CONSTRAINT [DF_ShipMethod_rowguid] DEFAULT (newid()),
[ModifiedDate] [datetime] NOT NULL CONSTRAINT [DF_ShipMethod_ModifiedDate] DEFAULT (getdate())
)
GO
PRINT N'Creating primary key [PK_ShipMethod_ShipMethodID] on [Purchasing].[ShipMethod]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[Purchasing].[PK_ShipMethod_ShipMethodID]', 'PK') AND parent_object_id = OBJECT_ID(N'[Purchasing].[ShipMethod]', 'U'))
ALTER TABLE [Purchasing].[ShipMethod] ADD CONSTRAINT [PK_ShipMethod_ShipMethodID] PRIMARY KEY CLUSTERED  ([ShipMethodID])
GO
PRINT N'Creating index [AK_ShipMethod_Name] on [Purchasing].[ShipMethod]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'AK_ShipMethod_Name' AND object_id = OBJECT_ID(N'[Purchasing].[ShipMethod]'))
CREATE UNIQUE NONCLUSTERED INDEX [AK_ShipMethod_Name] ON [Purchasing].[ShipMethod] ([Name])
GO
PRINT N'Creating index [AK_ShipMethod_rowguid] on [Purchasing].[ShipMethod]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'AK_ShipMethod_rowguid' AND object_id = OBJECT_ID(N'[Purchasing].[ShipMethod]'))
CREATE UNIQUE NONCLUSTERED INDEX [AK_ShipMethod_rowguid] ON [Purchasing].[ShipMethod] ([rowguid])
GO
PRINT N'Creating [Sales].[SpecialOfferProduct]'
GO
IF OBJECT_ID(N'[Sales].[SpecialOfferProduct]', 'U') IS NULL
CREATE TABLE [Sales].[SpecialOfferProduct]
(
[SpecialOfferID] [int] NOT NULL,
[ProductID] [int] NOT NULL,
[rowguid] [uniqueidentifier] NOT NULL ROWGUIDCOL CONSTRAINT [DF_SpecialOfferProduct_rowguid] DEFAULT (newid()),
[ModifiedDate] [datetime] NOT NULL CONSTRAINT [DF_SpecialOfferProduct_ModifiedDate] DEFAULT (getdate())
)
GO
PRINT N'Creating primary key [PK_SpecialOfferProduct_SpecialOfferID_ProductID] on [Sales].[SpecialOfferProduct]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[Sales].[PK_SpecialOfferProduct_SpecialOfferID_ProductID]', 'PK') AND parent_object_id = OBJECT_ID(N'[Sales].[SpecialOfferProduct]', 'U'))
ALTER TABLE [Sales].[SpecialOfferProduct] ADD CONSTRAINT [PK_SpecialOfferProduct_SpecialOfferID_ProductID] PRIMARY KEY CLUSTERED  ([SpecialOfferID], [ProductID])
GO
PRINT N'Creating index [IX_SpecialOfferProduct_ProductID] on [Sales].[SpecialOfferProduct]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_SpecialOfferProduct_ProductID' AND object_id = OBJECT_ID(N'[Sales].[SpecialOfferProduct]'))
CREATE NONCLUSTERED INDEX [IX_SpecialOfferProduct_ProductID] ON [Sales].[SpecialOfferProduct] ([ProductID])
GO
PRINT N'Creating index [AK_SpecialOfferProduct_rowguid] on [Sales].[SpecialOfferProduct]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'AK_SpecialOfferProduct_rowguid' AND object_id = OBJECT_ID(N'[Sales].[SpecialOfferProduct]'))
CREATE UNIQUE NONCLUSTERED INDEX [AK_SpecialOfferProduct_rowguid] ON [Sales].[SpecialOfferProduct] ([rowguid])
GO
PRINT N'Creating [Sales].[SalesOrderHeaderSalesReason]'
GO
IF OBJECT_ID(N'[Sales].[SalesOrderHeaderSalesReason]', 'U') IS NULL
CREATE TABLE [Sales].[SalesOrderHeaderSalesReason]
(
[SalesOrderID] [int] NOT NULL,
[SalesReasonID] [int] NOT NULL,
[ModifiedDate] [datetime] NOT NULL CONSTRAINT [DF_SalesOrderHeaderSalesReason_ModifiedDate] DEFAULT (getdate())
)
GO
PRINT N'Creating primary key [PK_SalesOrderHeaderSalesReason_SalesOrderID_SalesReasonID] on [Sales].[SalesOrderHeaderSalesReason]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[Sales].[PK_SalesOrderHeaderSalesReason_SalesOrderID_SalesReasonID]', 'PK') AND parent_object_id = OBJECT_ID(N'[Sales].[SalesOrderHeaderSalesReason]', 'U'))
ALTER TABLE [Sales].[SalesOrderHeaderSalesReason] ADD CONSTRAINT [PK_SalesOrderHeaderSalesReason_SalesOrderID_SalesReasonID] PRIMARY KEY CLUSTERED  ([SalesOrderID], [SalesReasonID])
GO
PRINT N'Creating [Sales].[SalesReason]'
GO
IF OBJECT_ID(N'[Sales].[SalesReason]', 'U') IS NULL
CREATE TABLE [Sales].[SalesReason]
(
[SalesReasonID] [int] NOT NULL IDENTITY(1, 1),
[Name] [dbo].[Name] NOT NULL,
[ReasonType] [dbo].[Name] NOT NULL,
[ModifiedDate] [datetime] NOT NULL CONSTRAINT [DF_SalesReason_ModifiedDate] DEFAULT (getdate())
)
GO
PRINT N'Creating primary key [PK_SalesReason_SalesReasonID] on [Sales].[SalesReason]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[Sales].[PK_SalesReason_SalesReasonID]', 'PK') AND parent_object_id = OBJECT_ID(N'[Sales].[SalesReason]', 'U'))
ALTER TABLE [Sales].[SalesReason] ADD CONSTRAINT [PK_SalesReason_SalesReasonID] PRIMARY KEY CLUSTERED  ([SalesReasonID])
GO
PRINT N'Creating [Sales].[SalesPersonQuotaHistory]'
GO
IF OBJECT_ID(N'[Sales].[SalesPersonQuotaHistory]', 'U') IS NULL
CREATE TABLE [Sales].[SalesPersonQuotaHistory]
(
[BusinessEntityID] [int] NOT NULL,
[QuotaDate] [datetime] NOT NULL,
[SalesQuota] [money] NOT NULL,
[rowguid] [uniqueidentifier] NOT NULL ROWGUIDCOL CONSTRAINT [DF_SalesPersonQuotaHistory_rowguid] DEFAULT (newid()),
[ModifiedDate] [datetime] NOT NULL CONSTRAINT [DF_SalesPersonQuotaHistory_ModifiedDate] DEFAULT (getdate())
)
GO
PRINT N'Creating primary key [PK_SalesPersonQuotaHistory_BusinessEntityID_QuotaDate] on [Sales].[SalesPersonQuotaHistory]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[Sales].[PK_SalesPersonQuotaHistory_BusinessEntityID_QuotaDate]', 'PK') AND parent_object_id = OBJECT_ID(N'[Sales].[SalesPersonQuotaHistory]', 'U'))
ALTER TABLE [Sales].[SalesPersonQuotaHistory] ADD CONSTRAINT [PK_SalesPersonQuotaHistory_BusinessEntityID_QuotaDate] PRIMARY KEY CLUSTERED  ([BusinessEntityID], [QuotaDate])
GO
PRINT N'Creating index [AK_SalesPersonQuotaHistory_rowguid] on [Sales].[SalesPersonQuotaHistory]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'AK_SalesPersonQuotaHistory_rowguid' AND object_id = OBJECT_ID(N'[Sales].[SalesPersonQuotaHistory]'))
CREATE UNIQUE NONCLUSTERED INDEX [AK_SalesPersonQuotaHistory_rowguid] ON [Sales].[SalesPersonQuotaHistory] ([rowguid])
GO
PRINT N'Creating [Sales].[SalesTaxRate]'
GO
IF OBJECT_ID(N'[Sales].[SalesTaxRate]', 'U') IS NULL
CREATE TABLE [Sales].[SalesTaxRate]
(
[SalesTaxRateID] [int] NOT NULL IDENTITY(1, 1),
[StateProvinceID] [int] NOT NULL,
[TaxType] [tinyint] NOT NULL,
[TaxRate] [smallmoney] NOT NULL CONSTRAINT [DF_SalesTaxRate_TaxRate] DEFAULT ((0.00)),
[Name] [dbo].[Name] NOT NULL,
[rowguid] [uniqueidentifier] NOT NULL ROWGUIDCOL CONSTRAINT [DF_SalesTaxRate_rowguid] DEFAULT (newid()),
[ModifiedDate] [datetime] NOT NULL CONSTRAINT [DF_SalesTaxRate_ModifiedDate] DEFAULT (getdate())
)
GO
PRINT N'Creating primary key [PK_SalesTaxRate_SalesTaxRateID] on [Sales].[SalesTaxRate]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[Sales].[PK_SalesTaxRate_SalesTaxRateID]', 'PK') AND parent_object_id = OBJECT_ID(N'[Sales].[SalesTaxRate]', 'U'))
ALTER TABLE [Sales].[SalesTaxRate] ADD CONSTRAINT [PK_SalesTaxRate_SalesTaxRateID] PRIMARY KEY CLUSTERED  ([SalesTaxRateID])
GO
PRINT N'Creating index [AK_SalesTaxRate_rowguid] on [Sales].[SalesTaxRate]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'AK_SalesTaxRate_rowguid' AND object_id = OBJECT_ID(N'[Sales].[SalesTaxRate]'))
CREATE UNIQUE NONCLUSTERED INDEX [AK_SalesTaxRate_rowguid] ON [Sales].[SalesTaxRate] ([rowguid])
GO
PRINT N'Creating index [AK_SalesTaxRate_StateProvinceID_TaxType] on [Sales].[SalesTaxRate]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'AK_SalesTaxRate_StateProvinceID_TaxType' AND object_id = OBJECT_ID(N'[Sales].[SalesTaxRate]'))
CREATE UNIQUE NONCLUSTERED INDEX [AK_SalesTaxRate_StateProvinceID_TaxType] ON [Sales].[SalesTaxRate] ([StateProvinceID], [TaxType])
GO
PRINT N'Creating [Sales].[SalesTerritoryHistory]'
GO
IF OBJECT_ID(N'[Sales].[SalesTerritoryHistory]', 'U') IS NULL
CREATE TABLE [Sales].[SalesTerritoryHistory]
(
[BusinessEntityID] [int] NOT NULL,
[TerritoryID] [int] NOT NULL,
[StartDate] [datetime] NOT NULL,
[EndDate] [datetime] NULL,
[rowguid] [uniqueidentifier] NOT NULL ROWGUIDCOL CONSTRAINT [DF_SalesTerritoryHistory_rowguid] DEFAULT (newid()),
[ModifiedDate] [datetime] NOT NULL CONSTRAINT [DF_SalesTerritoryHistory_ModifiedDate] DEFAULT (getdate())
)
GO
PRINT N'Creating primary key [PK_SalesTerritoryHistory_BusinessEntityID_StartDate_TerritoryID] on [Sales].[SalesTerritoryHistory]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[Sales].[PK_SalesTerritoryHistory_BusinessEntityID_StartDate_TerritoryID]', 'PK') AND parent_object_id = OBJECT_ID(N'[Sales].[SalesTerritoryHistory]', 'U'))
ALTER TABLE [Sales].[SalesTerritoryHistory] ADD CONSTRAINT [PK_SalesTerritoryHistory_BusinessEntityID_StartDate_TerritoryID] PRIMARY KEY CLUSTERED  ([BusinessEntityID], [StartDate], [TerritoryID])
GO
PRINT N'Creating index [AK_SalesTerritoryHistory_rowguid] on [Sales].[SalesTerritoryHistory]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'AK_SalesTerritoryHistory_rowguid' AND object_id = OBJECT_ID(N'[Sales].[SalesTerritoryHistory]'))
CREATE UNIQUE NONCLUSTERED INDEX [AK_SalesTerritoryHistory_rowguid] ON [Sales].[SalesTerritoryHistory] ([rowguid])
GO
PRINT N'Creating [Sales].[ShoppingCartItem]'
GO
IF OBJECT_ID(N'[Sales].[ShoppingCartItem]', 'U') IS NULL
CREATE TABLE [Sales].[ShoppingCartItem]
(
[ShoppingCartItemID] [int] NOT NULL IDENTITY(1, 1),
[ShoppingCartID] [nvarchar] (50) NOT NULL,
[Quantity] [int] NOT NULL CONSTRAINT [DF_ShoppingCartItem_Quantity] DEFAULT ((1)),
[ProductID] [int] NOT NULL,
[DateCreated] [datetime] NOT NULL CONSTRAINT [DF_ShoppingCartItem_DateCreated] DEFAULT (getdate()),
[ModifiedDate] [datetime] NOT NULL CONSTRAINT [DF_ShoppingCartItem_ModifiedDate] DEFAULT (getdate())
)
GO
PRINT N'Creating primary key [PK_ShoppingCartItem_ShoppingCartItemID] on [Sales].[ShoppingCartItem]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[Sales].[PK_ShoppingCartItem_ShoppingCartItemID]', 'PK') AND parent_object_id = OBJECT_ID(N'[Sales].[ShoppingCartItem]', 'U'))
ALTER TABLE [Sales].[ShoppingCartItem] ADD CONSTRAINT [PK_ShoppingCartItem_ShoppingCartItemID] PRIMARY KEY CLUSTERED  ([ShoppingCartItemID])
GO
PRINT N'Creating index [IX_ShoppingCartItem_ShoppingCartID_ProductID] on [Sales].[ShoppingCartItem]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ShoppingCartItem_ShoppingCartID_ProductID' AND object_id = OBJECT_ID(N'[Sales].[ShoppingCartItem]'))
CREATE NONCLUSTERED INDEX [IX_ShoppingCartItem_ShoppingCartID_ProductID] ON [Sales].[ShoppingCartItem] ([ShoppingCartID], [ProductID])
GO
PRINT N'Creating [Sales].[SpecialOffer]'
GO
IF OBJECT_ID(N'[Sales].[SpecialOffer]', 'U') IS NULL
CREATE TABLE [Sales].[SpecialOffer]
(
[SpecialOfferID] [int] NOT NULL IDENTITY(1, 1),
[Description] [nvarchar] (255) NOT NULL,
[DiscountPct] [smallmoney] NOT NULL CONSTRAINT [DF_SpecialOffer_DiscountPct] DEFAULT ((0.00)),
[Type] [nvarchar] (50) NOT NULL,
[Category] [nvarchar] (50) NOT NULL,
[StartDate] [datetime] NOT NULL,
[EndDate] [datetime] NOT NULL,
[MinQty] [int] NOT NULL CONSTRAINT [DF_SpecialOffer_MinQty] DEFAULT ((0)),
[MaxQty] [int] NULL,
[rowguid] [uniqueidentifier] NOT NULL ROWGUIDCOL CONSTRAINT [DF_SpecialOffer_rowguid] DEFAULT (newid()),
[ModifiedDate] [datetime] NOT NULL CONSTRAINT [DF_SpecialOffer_ModifiedDate] DEFAULT (getdate())
)
GO
PRINT N'Creating primary key [PK_SpecialOffer_SpecialOfferID] on [Sales].[SpecialOffer]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[Sales].[PK_SpecialOffer_SpecialOfferID]', 'PK') AND parent_object_id = OBJECT_ID(N'[Sales].[SpecialOffer]', 'U'))
ALTER TABLE [Sales].[SpecialOffer] ADD CONSTRAINT [PK_SpecialOffer_SpecialOfferID] PRIMARY KEY CLUSTERED  ([SpecialOfferID])
GO
PRINT N'Creating index [AK_SpecialOffer_rowguid] on [Sales].[SpecialOffer]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'AK_SpecialOffer_rowguid' AND object_id = OBJECT_ID(N'[Sales].[SpecialOffer]'))
CREATE UNIQUE NONCLUSTERED INDEX [AK_SpecialOffer_rowguid] ON [Sales].[SpecialOffer] ([rowguid])
GO
PRINT N'Creating [Production].[ScrapReason]'
GO
IF OBJECT_ID(N'[Production].[ScrapReason]', 'U') IS NULL
CREATE TABLE [Production].[ScrapReason]
(
[ScrapReasonID] [smallint] NOT NULL IDENTITY(1, 1),
[Name] [dbo].[Name] NOT NULL,
[ModifiedDate] [datetime] NOT NULL CONSTRAINT [DF_ScrapReason_ModifiedDate] DEFAULT (getdate())
)
GO
PRINT N'Creating primary key [PK_ScrapReason_ScrapReasonID] on [Production].[ScrapReason]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[Production].[PK_ScrapReason_ScrapReasonID]', 'PK') AND parent_object_id = OBJECT_ID(N'[Production].[ScrapReason]', 'U'))
ALTER TABLE [Production].[ScrapReason] ADD CONSTRAINT [PK_ScrapReason_ScrapReasonID] PRIMARY KEY CLUSTERED  ([ScrapReasonID])
GO
PRINT N'Creating index [AK_ScrapReason_Name] on [Production].[ScrapReason]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'AK_ScrapReason_Name' AND object_id = OBJECT_ID(N'[Production].[ScrapReason]'))
CREATE UNIQUE NONCLUSTERED INDEX [AK_ScrapReason_Name] ON [Production].[ScrapReason] ([Name])
GO
PRINT N'Creating [Production].[WorkOrderRouting]'
GO
IF OBJECT_ID(N'[Production].[WorkOrderRouting]', 'U') IS NULL
CREATE TABLE [Production].[WorkOrderRouting]
(
[WorkOrderID] [int] NOT NULL,
[ProductID] [int] NOT NULL,
[OperationSequence] [smallint] NOT NULL,
[LocationID] [smallint] NOT NULL,
[ScheduledStartDate] [datetime] NOT NULL,
[ScheduledEndDate] [datetime] NOT NULL,
[ActualStartDate] [datetime] NULL,
[ActualEndDate] [datetime] NULL,
[ActualResourceHrs] [decimal] (9, 4) NULL,
[PlannedCost] [money] NOT NULL,
[ActualCost] [money] NULL,
[ModifiedDate] [datetime] NOT NULL CONSTRAINT [DF_WorkOrderRouting_ModifiedDate] DEFAULT (getdate())
)
GO
PRINT N'Creating primary key [PK_WorkOrderRouting_WorkOrderID_ProductID_OperationSequence] on [Production].[WorkOrderRouting]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[Production].[PK_WorkOrderRouting_WorkOrderID_ProductID_OperationSequence]', 'PK') AND parent_object_id = OBJECT_ID(N'[Production].[WorkOrderRouting]', 'U'))
ALTER TABLE [Production].[WorkOrderRouting] ADD CONSTRAINT [PK_WorkOrderRouting_WorkOrderID_ProductID_OperationSequence] PRIMARY KEY CLUSTERED  ([WorkOrderID], [ProductID], [OperationSequence])
GO
PRINT N'Creating index [IX_WorkOrderRouting_ProductID] on [Production].[WorkOrderRouting]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_WorkOrderRouting_ProductID' AND object_id = OBJECT_ID(N'[Production].[WorkOrderRouting]'))
CREATE NONCLUSTERED INDEX [IX_WorkOrderRouting_ProductID] ON [Production].[WorkOrderRouting] ([ProductID])
GO
PRINT N'Creating [Sales].[SalesOrderHeader_inmem]'
GO
IF OBJECT_ID(N'[Sales].[SalesOrderHeader_inmem]', 'U') IS NULL
CREATE TABLE [Sales].[SalesOrderHeader_inmem]
(
[SalesOrderID] [int] NOT NULL IDENTITY(1, 1),
[RevisionNumber] [tinyint] NOT NULL CONSTRAINT [IMDF_SalesOrderHeader_RevisionNumber] DEFAULT ((0)),
[OrderDate] [datetime2] NOT NULL,
[DueDate] [datetime2] NOT NULL,
[ShipDate] [datetime2] NULL,
[Status] [tinyint] NOT NULL CONSTRAINT [IMDF_SalesOrderHeader_Status] DEFAULT ((1)),
[OnlineOrderFlag] [bit] NOT NULL CONSTRAINT [IMDF_SalesOrderHeader_OnlineOrderFlag] DEFAULT ((1)),
[PurchaseOrderNumber] [nvarchar] (25) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[AccountNumber] [nvarchar] (15) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[CustomerID] [int] NOT NULL,
[SalesPersonID] [int] NOT NULL CONSTRAINT [IMDF_SalesOrderHeader_SalesPersonID] DEFAULT ((-1)),
[TerritoryID] [int] NULL,
[BillToAddressID] [int] NOT NULL,
[ShipToAddressID] [int] NOT NULL,
[ShipMethodID] [int] NOT NULL,
[CreditCardID] [int] NULL,
[CreditCardApprovalCode] [varchar] (15) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[CurrencyRateID] [int] NULL,
[SubTotal] [money] NOT NULL CONSTRAINT [IMDF_SalesOrderHeader_SubTotal] DEFAULT ((0.00)),
[TaxAmt] [money] NOT NULL CONSTRAINT [IMDF_SalesOrderHeader_TaxAmt] DEFAULT ((0.00)),
[Freight] [money] NOT NULL CONSTRAINT [IMDF_SalesOrderHeader_Freight] DEFAULT ((0.00)),
[Comment] [nvarchar] (128) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[ModifiedDate] [datetime2] NOT NULL,
CONSTRAINT [PK__SalesOrd__B14003C3270C320B] PRIMARY KEY NONCLUSTERED HASH  ([SalesOrderID]) WITH (BUCKET_COUNT=16777216),
INDEX [IX_CustomerID] NONCLUSTERED HASH ([CustomerID]) WITH (BUCKET_COUNT=1048576),
INDEX [IX_SalesPersonID] NONCLUSTERED HASH ([SalesPersonID]) WITH (BUCKET_COUNT=1048576)
)
WITH
(
MEMORY_OPTIMIZED = ON
)
GO
PRINT N'Creating [Sales].[SalesOrderDetail_inmem]'
GO
IF OBJECT_ID(N'[Sales].[SalesOrderDetail_inmem]', 'U') IS NULL
CREATE TABLE [Sales].[SalesOrderDetail_inmem]
(
[SalesOrderID] [int] NOT NULL,
[SalesOrderDetailID] [bigint] NOT NULL IDENTITY(1, 1),
[CarrierTrackingNumber] [nvarchar] (25) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[OrderQty] [smallint] NOT NULL,
[ProductID] [int] NOT NULL,
[SpecialOfferID] [int] NOT NULL,
[UnitPrice] [money] NOT NULL,
[UnitPriceDiscount] [money] NOT NULL CONSTRAINT [IMDF_SalesOrderDetail_UnitPriceDiscount] DEFAULT ((0.0)),
[ModifiedDate] [datetime2] NOT NULL,
CONSTRAINT [imPK_SalesOrderDetail_SalesOrderID_SalesOrderDetailID] PRIMARY KEY NONCLUSTERED HASH  ([SalesOrderID], [SalesOrderDetailID]) WITH (BUCKET_COUNT=67108864),
INDEX [IX_ProductID] NONCLUSTERED HASH ([ProductID]) WITH (BUCKET_COUNT=1048576),
INDEX [IX_SalesOrderID] NONCLUSTERED HASH ([SalesOrderID]) WITH (BUCKET_COUNT=16777216)
)
WITH
(
MEMORY_OPTIMIZED = ON
)
GO
PRINT N'Creating [Sales].[SpecialOfferProduct_inmem]'
GO
IF OBJECT_ID(N'[Sales].[SpecialOfferProduct_inmem]', 'U') IS NULL
CREATE TABLE [Sales].[SpecialOfferProduct_inmem]
(
[SpecialOfferID] [int] NOT NULL,
[ProductID] [int] NOT NULL,
[ModifiedDate] [datetime2] NOT NULL CONSTRAINT [IMDF_SpecialOfferProduct_ModifiedDate] DEFAULT (sysdatetime()),
CONSTRAINT [IMPK_SpecialOfferProduct_SpecialOfferID_ProductID] PRIMARY KEY NONCLUSTERED  ([SpecialOfferID], [ProductID]),
INDEX [ix_ProductID] NONCLUSTERED ([ProductID])
)
WITH
(
MEMORY_OPTIMIZED = ON
)
GO
PRINT N'Creating [Production].[Product_inmem]'
GO
IF OBJECT_ID(N'[Production].[Product_inmem]', 'U') IS NULL
CREATE TABLE [Production].[Product_inmem]
(
[ProductID] [int] NOT NULL IDENTITY(1, 1),
[Name] [nvarchar] (50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
[ProductNumber] [nvarchar] (25) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
[MakeFlag] [bit] NOT NULL CONSTRAINT [IMDF_Product_MakeFlag] DEFAULT ((1)),
[FinishedGoodsFlag] [bit] NOT NULL CONSTRAINT [IMDF_Product_FinishedGoodsFlag] DEFAULT ((1)),
[Color] [nvarchar] (15) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[SafetyStockLevel] [smallint] NOT NULL,
[ReorderPoint] [smallint] NOT NULL,
[StandardCost] [money] NOT NULL,
[ListPrice] [money] NOT NULL,
[Size] [nvarchar] (5) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[SizeUnitMeasureCode] [nchar] (3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[WeightUnitMeasureCode] [nchar] (3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[Weight] [decimal] (8, 2) NULL,
[DaysToManufacture] [int] NOT NULL,
[ProductLine] [nchar] (2) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[Class] [nchar] (2) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[Style] [nchar] (2) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[ProductSubcategoryID] [int] NULL,
[ProductModelID] [int] NULL,
[SellStartDate] [datetime2] NOT NULL,
[SellEndDate] [datetime2] NULL,
[DiscontinuedDate] [datetime2] NULL,
[ModifiedDate] [datetime2] NOT NULL CONSTRAINT [IMDF_Product_ModifiedDate] DEFAULT (sysdatetime()),
CONSTRAINT [IMPK_Product_ProductID] PRIMARY KEY NONCLUSTERED HASH  ([ProductID]) WITH (BUCKET_COUNT=1048576),
INDEX [IX_Name] NONCLUSTERED ([Name]),
INDEX [IX_ProductNumber] NONCLUSTERED ([ProductNumber])
)
WITH
(
MEMORY_OPTIMIZED = ON
)
GO
PRINT N'Creating [Sales].[SpecialOffer_inmem]'
GO
IF OBJECT_ID(N'[Sales].[SpecialOffer_inmem]', 'U') IS NULL
CREATE TABLE [Sales].[SpecialOffer_inmem]
(
[SpecialOfferID] [int] NOT NULL IDENTITY(1, 1),
[Description] [nvarchar] (255) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
[DiscountPct] [smallmoney] NOT NULL CONSTRAINT [IMDF_SpecialOffer_DiscountPct] DEFAULT ((0.00)),
[Type] [nvarchar] (50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
[Category] [nvarchar] (50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
[StartDate] [datetime2] NOT NULL,
[EndDate] [datetime2] NOT NULL,
[MinQty] [int] NOT NULL CONSTRAINT [IMDF_SpecialOffer_MinQty] DEFAULT ((0)),
[MaxQty] [int] NULL,
[ModifiedDate] [datetime2] NOT NULL CONSTRAINT [IMDF_SpecialOffer_ModifiedDate] DEFAULT (sysdatetime()),
CONSTRAINT [IMPK_SpecialOffer_SpecialOfferID] PRIMARY KEY NONCLUSTERED HASH  ([SpecialOfferID]) WITH (BUCKET_COUNT=1048576)
)
WITH
(
MEMORY_OPTIMIZED = ON
)
GO
PRINT N'Creating [Sales].[SalesOrderHeader_ondisk]'
GO
IF OBJECT_ID(N'[Sales].[SalesOrderHeader_ondisk]', 'U') IS NULL
CREATE TABLE [Sales].[SalesOrderHeader_ondisk]
(
[SalesOrderID] [int] NOT NULL IDENTITY(1, 1),
[RevisionNumber] [tinyint] NOT NULL CONSTRAINT [ODDF_SalesOrderHeader_RevisionNumber] DEFAULT ((0)),
[OrderDate] [datetime2] NOT NULL,
[DueDate] [datetime2] NOT NULL,
[ShipDate] [datetime2] NULL,
[Status] [tinyint] NOT NULL CONSTRAINT [ODDF_SalesOrderHeader_Status] DEFAULT ((1)),
[OnlineOrderFlag] [bit] NOT NULL CONSTRAINT [ODDF_SalesOrderHeader_OnlineOrderFlag] DEFAULT ((1)),
[PurchaseOrderNumber] [nvarchar] (25) NULL,
[AccountNumber] [nvarchar] (15) NULL,
[CustomerID] [int] NOT NULL,
[SalesPersonID] [int] NOT NULL CONSTRAINT [ODDF_SalesOrderHeader_SalesPersonID] DEFAULT ((-1)),
[TerritoryID] [int] NULL,
[BillToAddressID] [int] NOT NULL,
[ShipToAddressID] [int] NOT NULL,
[ShipMethodID] [int] NOT NULL,
[CreditCardID] [int] NULL,
[CreditCardApprovalCode] [varchar] (15) NULL,
[CurrencyRateID] [int] NULL,
[SubTotal] [money] NOT NULL CONSTRAINT [ODDF_SalesOrderHeader_SubTotal] DEFAULT ((0.00)),
[TaxAmt] [money] NOT NULL CONSTRAINT [ODDF_SalesOrderHeader_TaxAmt] DEFAULT ((0.00)),
[Freight] [money] NOT NULL CONSTRAINT [ODDF_SalesOrderHeader_Freight] DEFAULT ((0.00)),
[Comment] [nvarchar] (128) NULL,
[ModifiedDate] [datetime2] NOT NULL
)
GO
PRINT N'Creating primary key [PK__SalesOrd__B14003C2B181FB70] on [Sales].[SalesOrderHeader_ondisk]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[Sales].[PK__SalesOrd__B14003C2B181FB70]', 'PK') AND parent_object_id = OBJECT_ID(N'[Sales].[SalesOrderHeader_ondisk]', 'U'))
ALTER TABLE [Sales].[SalesOrderHeader_ondisk] ADD CONSTRAINT [PK__SalesOrd__B14003C2B181FB70] PRIMARY KEY CLUSTERED  ([SalesOrderID])
GO
PRINT N'Creating index [IX_CustomerID] on [Sales].[SalesOrderHeader_ondisk]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_CustomerID' AND object_id = OBJECT_ID(N'[Sales].[SalesOrderHeader_ondisk]'))
CREATE NONCLUSTERED INDEX [IX_CustomerID] ON [Sales].[SalesOrderHeader_ondisk] ([CustomerID])
GO
PRINT N'Creating index [IX_SalesPersonID] on [Sales].[SalesOrderHeader_ondisk]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_SalesPersonID' AND object_id = OBJECT_ID(N'[Sales].[SalesOrderHeader_ondisk]'))
CREATE NONCLUSTERED INDEX [IX_SalesPersonID] ON [Sales].[SalesOrderHeader_ondisk] ([SalesPersonID])
GO
PRINT N'Creating [Sales].[SalesOrderDetail_ondisk]'
GO
IF OBJECT_ID(N'[Sales].[SalesOrderDetail_ondisk]', 'U') IS NULL
CREATE TABLE [Sales].[SalesOrderDetail_ondisk]
(
[SalesOrderID] [int] NOT NULL,
[SalesOrderDetailID] [bigint] NOT NULL IDENTITY(1, 1),
[CarrierTrackingNumber] [nvarchar] (25) NULL,
[OrderQty] [smallint] NOT NULL,
[ProductID] [int] NOT NULL,
[SpecialOfferID] [int] NOT NULL,
[UnitPrice] [money] NOT NULL,
[UnitPriceDiscount] [money] NOT NULL CONSTRAINT [ODDF_SalesOrderDetail_UnitPriceDiscount] DEFAULT ((0.0)),
[ModifiedDate] [datetime2] NOT NULL
)
GO
PRINT N'Creating primary key [ODPK_SalesOrderDetail_SalesOrderID_SalesOrderDetailID] on [Sales].[SalesOrderDetail_ondisk]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[Sales].[ODPK_SalesOrderDetail_SalesOrderID_SalesOrderDetailID]', 'PK') AND parent_object_id = OBJECT_ID(N'[Sales].[SalesOrderDetail_ondisk]', 'U'))
ALTER TABLE [Sales].[SalesOrderDetail_ondisk] ADD CONSTRAINT [ODPK_SalesOrderDetail_SalesOrderID_SalesOrderDetailID] PRIMARY KEY CLUSTERED  ([SalesOrderID], [SalesOrderDetailID])
GO
PRINT N'Creating index [IX_ProductID] on [Sales].[SalesOrderDetail_ondisk]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ProductID' AND object_id = OBJECT_ID(N'[Sales].[SalesOrderDetail_ondisk]'))
CREATE NONCLUSTERED INDEX [IX_ProductID] ON [Sales].[SalesOrderDetail_ondisk] ([ProductID])
GO
PRINT N'Creating [Sales].[SpecialOfferProduct_ondisk]'
GO
IF OBJECT_ID(N'[Sales].[SpecialOfferProduct_ondisk]', 'U') IS NULL
CREATE TABLE [Sales].[SpecialOfferProduct_ondisk]
(
[SpecialOfferID] [int] NOT NULL,
[ProductID] [int] NOT NULL,
[ModifiedDate] [datetime2] NOT NULL CONSTRAINT [ODDF_SpecialOfferProduct_ModifiedDate] DEFAULT (sysdatetime())
)
GO
PRINT N'Creating primary key [ODPK_SpecialOfferProduct_SpecialOfferID_ProductID] on [Sales].[SpecialOfferProduct_ondisk]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[Sales].[ODPK_SpecialOfferProduct_SpecialOfferID_ProductID]', 'PK') AND parent_object_id = OBJECT_ID(N'[Sales].[SpecialOfferProduct_ondisk]', 'U'))
ALTER TABLE [Sales].[SpecialOfferProduct_ondisk] ADD CONSTRAINT [ODPK_SpecialOfferProduct_SpecialOfferID_ProductID] PRIMARY KEY NONCLUSTERED  ([SpecialOfferID], [ProductID])
GO
PRINT N'Creating index [ix_ProductID] on [Sales].[SpecialOfferProduct_ondisk]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'ix_ProductID' AND object_id = OBJECT_ID(N'[Sales].[SpecialOfferProduct_ondisk]'))
CREATE NONCLUSTERED INDEX [ix_ProductID] ON [Sales].[SpecialOfferProduct_ondisk] ([ProductID])
GO
PRINT N'Creating [Production].[Product_ondisk]'
GO
IF OBJECT_ID(N'[Production].[Product_ondisk]', 'U') IS NULL
CREATE TABLE [Production].[Product_ondisk]
(
[ProductID] [int] NOT NULL IDENTITY(1, 1),
[Name] [nvarchar] (50) NOT NULL,
[ProductNumber] [nvarchar] (25) NOT NULL,
[MakeFlag] [bit] NOT NULL CONSTRAINT [ODDF_Product_MakeFlag] DEFAULT ((1)),
[FinishedGoodsFlag] [bit] NOT NULL CONSTRAINT [ODDF_Product_FinishedGoodsFlag] DEFAULT ((1)),
[Color] [nvarchar] (15) NULL,
[SafetyStockLevel] [smallint] NOT NULL,
[ReorderPoint] [smallint] NOT NULL,
[StandardCost] [money] NOT NULL,
[ListPrice] [money] NOT NULL,
[Size] [nvarchar] (5) NULL,
[SizeUnitMeasureCode] [nchar] (3) NULL,
[WeightUnitMeasureCode] [nchar] (3) NULL,
[Weight] [decimal] (8, 2) NULL,
[DaysToManufacture] [int] NOT NULL,
[ProductLine] [nchar] (2) NULL,
[Class] [nchar] (2) NULL,
[Style] [nchar] (2) NULL,
[ProductSubcategoryID] [int] NULL,
[ProductModelID] [int] NULL,
[SellStartDate] [datetime2] NOT NULL,
[SellEndDate] [datetime2] NULL,
[DiscontinuedDate] [datetime2] NULL,
[ModifiedDate] [datetime2] NOT NULL CONSTRAINT [ODDF_Product_ModifiedDate] DEFAULT (sysdatetime())
)
GO
PRINT N'Creating primary key [ODPK_Product_ProductID] on [Production].[Product_ondisk]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[Production].[ODPK_Product_ProductID]', 'PK') AND parent_object_id = OBJECT_ID(N'[Production].[Product_ondisk]', 'U'))
ALTER TABLE [Production].[Product_ondisk] ADD CONSTRAINT [ODPK_Product_ProductID] PRIMARY KEY CLUSTERED  ([ProductID])
GO
PRINT N'Creating index [IX_Name] on [Production].[Product_ondisk]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Name' AND object_id = OBJECT_ID(N'[Production].[Product_ondisk]'))
CREATE NONCLUSTERED INDEX [IX_Name] ON [Production].[Product_ondisk] ([Name])
GO
PRINT N'Creating index [IX_ProductNumber] on [Production].[Product_ondisk]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ProductNumber' AND object_id = OBJECT_ID(N'[Production].[Product_ondisk]'))
CREATE NONCLUSTERED INDEX [IX_ProductNumber] ON [Production].[Product_ondisk] ([ProductNumber])
GO
PRINT N'Creating [Sales].[SpecialOffer_ondisk]'
GO
IF OBJECT_ID(N'[Sales].[SpecialOffer_ondisk]', 'U') IS NULL
CREATE TABLE [Sales].[SpecialOffer_ondisk]
(
[SpecialOfferID] [int] NOT NULL IDENTITY(1, 1),
[Description] [nvarchar] (255) NOT NULL,
[DiscountPct] [smallmoney] NOT NULL CONSTRAINT [ODDF_SpecialOffer_DiscountPct] DEFAULT ((0.00)),
[Type] [nvarchar] (50) NOT NULL,
[Category] [nvarchar] (50) NOT NULL,
[StartDate] [datetime2] NOT NULL,
[EndDate] [datetime2] NOT NULL,
[MinQty] [int] NOT NULL CONSTRAINT [ODDF_SpecialOffer_MinQty] DEFAULT ((0)),
[MaxQty] [int] NULL,
[ModifiedDate] [datetime2] NOT NULL CONSTRAINT [ODDF_SpecialOffer_ModifiedDate] DEFAULT (sysdatetime())
)
GO
PRINT N'Creating primary key [ODPK_SpecialOffer_SpecialOfferID] on [Sales].[SpecialOffer_ondisk]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[Sales].[ODPK_SpecialOffer_SpecialOfferID]', 'PK') AND parent_object_id = OBJECT_ID(N'[Sales].[SpecialOffer_ondisk]', 'U'))
ALTER TABLE [Sales].[SpecialOffer_ondisk] ADD CONSTRAINT [ODPK_SpecialOffer_SpecialOfferID] PRIMARY KEY CLUSTERED  ([SpecialOfferID])
GO
PRINT N'Creating [Person].[Person_json]'
GO
IF OBJECT_ID(N'[Person].[Person_json]', 'U') IS NULL
CREATE TABLE [Person].[Person_json]
(
[PersonID] [int] NOT NULL IDENTITY(1, 1) NOT FOR REPLICATION,
[PersonType] [nchar] (2) NOT NULL,
[NameStyle] [dbo].[NameStyle] NOT NULL CONSTRAINT [DF__Person_js__NameS__5D60DB10] DEFAULT ((0)),
[Title] [nvarchar] (8) NULL,
[FirstName] [dbo].[Name] NOT NULL,
[MiddleName] [dbo].[Name] NULL,
[LastName] [dbo].[Name] NOT NULL,
[Suffix] [nvarchar] (10) NULL,
[EmailPromotion] [int] NOT NULL CONSTRAINT [DF__Person_js__Email__5E54FF49] DEFAULT ((0)),
[AdditionalContactInfo] [nvarchar] (max) NULL,
[Demographics] [nvarchar] (max) NULL,
[rowguid] [uniqueidentifier] NOT NULL ROWGUIDCOL CONSTRAINT [DF__Person_js__rowgu__61316BF4] DEFAULT (newid()),
[ModifiedDate] [datetime] NOT NULL CONSTRAINT [DF__Person_js__Modif__6225902D] DEFAULT (getdate()),
[PhoneNumbers] [nvarchar] (max) NULL,
[EmailAddresses] [nvarchar] (max) NULL
)
GO
PRINT N'Creating primary key [PK_Person_json_PersonID] on [Person].[Person_json]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[Person].[PK_Person_json_PersonID]', 'PK') AND parent_object_id = OBJECT_ID(N'[Person].[Person_json]', 'U'))
ALTER TABLE [Person].[Person_json] ADD CONSTRAINT [PK_Person_json_PersonID] PRIMARY KEY CLUSTERED  ([PersonID])
GO
PRINT N'Creating [Sales].[SalesOrder_json]'
GO
IF OBJECT_ID(N'[Sales].[SalesOrder_json]', 'U') IS NULL
CREATE TABLE [Sales].[SalesOrder_json]
(
[SalesOrderID] [int] NOT NULL,
[RevisionNumber] [tinyint] NOT NULL CONSTRAINT [DF_SalesOrder_RevisionNumber] DEFAULT ((0)),
[OrderDate] [datetime] NOT NULL CONSTRAINT [DF_SalesOrder_OrderDate] DEFAULT (getdate()),
[DueDate] [datetime] NOT NULL,
[ShipDate] [datetime] NULL,
[Status] [tinyint] NOT NULL CONSTRAINT [DF_SalesOrder_Status] DEFAULT ((1)),
[OnlineOrderFlag] [dbo].[Flag] NOT NULL CONSTRAINT [DF_SalesOrder_OnlineOrderFlag] DEFAULT ((1)),
[SalesOrderNumber] AS (isnull(N'SO'+CONVERT([nvarchar](23),[SalesOrderID],(0)),N'*** ERROR ***')),
[PurchaseOrderNumber] [dbo].[OrderNumber] NULL,
[AccountNumber] [dbo].[AccountNumber] NULL,
[CustomerID] [int] NOT NULL,
[SalesPersonID] [int] NULL,
[TerritoryID] [int] NULL,
[BillToAddressID] [int] NULL,
[ShipToAddressID] [int] NULL,
[ShipMethodID] [int] NULL,
[CreditCardID] [int] NULL,
[CreditCardApprovalCode] [varchar] (15) NULL,
[CurrencyRateID] [int] NULL,
[SubTotal] [money] NOT NULL CONSTRAINT [DF_SalesOrder_SubTotal] DEFAULT ((0.00)),
[TaxAmt] [money] NOT NULL CONSTRAINT [DF_SalesOrder_TaxAmt] DEFAULT ((0.00)),
[Freight] [money] NOT NULL CONSTRAINT [DF_SalesOrder_Freight] DEFAULT ((0.00)),
[TotalDue] AS (isnull(([SubTotal]+[TaxAmt])+[Freight],(0))),
[Comment] [nvarchar] (128) NULL,
[rowguid] [uniqueidentifier] NOT NULL ROWGUIDCOL CONSTRAINT [DF_SalesOrder_rowguid] DEFAULT (newid()),
[ModifiedDate] [datetime] NOT NULL CONSTRAINT [DF_SalesOrder_ModifiedDate] DEFAULT (getdate()),
[SalesReasons] [nvarchar] (max) NULL,
[OrderItems] [nvarchar] (max) NULL,
[Info] [nvarchar] (max) NULL
)
GO
PRINT N'Creating primary key [PK_SalesOrder__json_SalesOrderID] on [Sales].[SalesOrder_json]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[Sales].[PK_SalesOrder__json_SalesOrderID]', 'PK') AND parent_object_id = OBJECT_ID(N'[Sales].[SalesOrder_json]', 'U'))
ALTER TABLE [Sales].[SalesOrder_json] ADD CONSTRAINT [PK_SalesOrder__json_SalesOrderID] PRIMARY KEY CLUSTERED  ([SalesOrderID])
GO
PRINT N'Creating [Sales].[vSalesOrderHeader_extended_inmem]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[Sales].[vSalesOrderHeader_extended_inmem]') AND (type = 'IF' OR type = 'FN' OR type = 'TF'))
EXEC sp_executesql N'CREATE FUNCTION [Sales].[vSalesOrderHeader_extended_inmem] ()
RETURNS TABLE
WITH SCHEMABINDING, NATIVE_COMPILATION
	RETURN SELECT SalesOrderID, 
		RevisionNumber, 
		OrderDate, 
		DueDate, 
		ShipDate, 
		Status, 
		OnlineOrderFlag, 
		PurchaseOrderNumber, 
		AccountNumber, 
		CustomerID, 
		SalesPersonID, 
		TerritoryID, 
		BillToAddressID, 
		ShipToAddressID,                          
		ShipMethodID, 
		CreditCardID, 
		CreditCardApprovalCode, 
		CurrencyRateID, 
		SubTotal, 
		Freight, 
		TaxAmt, 
		Comment, 
		ModifiedDate, 
		ISNULL(N''SO'' + CONVERT([nvarchar](23), SalesOrderID), N''*** ERROR ***'') AS SalesOrderNumber, 
		ISNULL(SubTotal + TaxAmt + Freight, 0) AS TotalDue
	FROM Sales.SalesOrderHeader_inmem
'
GO
PRINT N'Creating [Sales].[vSalesOrderDetail_extended_inmem]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[Sales].[vSalesOrderDetail_extended_inmem]') AND (type = 'IF' OR type = 'FN' OR type = 'TF'))
EXEC sp_executesql N'CREATE FUNCTION [Sales].[vSalesOrderDetail_extended_inmem] ()
RETURNS TABLE
WITH SCHEMABINDING, NATIVE_COMPILATION
	RETURN SELECT SalesOrderID, 
		SalesOrderDetailID, 
		CarrierTrackingNumber, 
		OrderQty, 
		ProductID, 
		SpecialOfferID, 
		UnitPrice, 
		UnitPriceDiscount, 
		ModifiedDate, 
		ISNULL(UnitPrice * (1.0 - UnitPriceDiscount) * OrderQty, 0.0) AS LineTotal
	FROM Sales.SalesOrderDetail_inmem
'
GO
PRINT N'Creating [Person].[vAdditionalContactInfo]'
GO
IF OBJECT_ID(N'[Person].[vAdditionalContactInfo]', 'V') IS NULL
EXEC sp_executesql N'
CREATE VIEW [Person].[vAdditionalContactInfo] 
AS 
SELECT 
    [BusinessEntityID] 
    ,[FirstName]
    ,[MiddleName]
    ,[LastName]
    ,[ContactInfo].ref.value(N''declare namespace ci="http://schemas.microsoft.com/sqlserver/2004/07/adventure-works/ContactInfo"; 
        declare namespace act="http://schemas.microsoft.com/sqlserver/2004/07/adventure-works/ContactTypes"; 
        (act:telephoneNumber)[1]/act:number'', ''nvarchar(50)'') AS [TelephoneNumber] 
    ,LTRIM(RTRIM([ContactInfo].ref.value(N''declare namespace ci="http://schemas.microsoft.com/sqlserver/2004/07/adventure-works/ContactInfo"; 
        declare namespace act="http://schemas.microsoft.com/sqlserver/2004/07/adventure-works/ContactTypes"; 
        (act:telephoneNumber/act:SpecialInstructions/text())[1]'', ''nvarchar(max)''))) AS [TelephoneSpecialInstructions] 
    ,[ContactInfo].ref.value(N''declare namespace ci="http://schemas.microsoft.com/sqlserver/2004/07/adventure-works/ContactInfo"; 
        declare namespace act="http://schemas.microsoft.com/sqlserver/2004/07/adventure-works/ContactTypes";
        (act:homePostalAddress/act:Street)[1]'', ''nvarchar(50)'') AS [Street] 
    ,[ContactInfo].ref.value(N''declare namespace ci="http://schemas.microsoft.com/sqlserver/2004/07/adventure-works/ContactInfo"; 
        declare namespace act="http://schemas.microsoft.com/sqlserver/2004/07/adventure-works/ContactTypes"; 
        (act:homePostalAddress/act:City)[1]'', ''nvarchar(50)'') AS [City] 
    ,[ContactInfo].ref.value(N''declare namespace ci="http://schemas.microsoft.com/sqlserver/2004/07/adventure-works/ContactInfo"; 
        declare namespace act="http://schemas.microsoft.com/sqlserver/2004/07/adventure-works/ContactTypes"; 
        (act:homePostalAddress/act:StateProvince)[1]'', ''nvarchar(50)'') AS [StateProvince] 
    ,[ContactInfo].ref.value(N''declare namespace ci="http://schemas.microsoft.com/sqlserver/2004/07/adventure-works/ContactInfo"; 
        declare namespace act="http://schemas.microsoft.com/sqlserver/2004/07/adventure-works/ContactTypes"; 
        (act:homePostalAddress/act:PostalCode)[1]'', ''nvarchar(50)'') AS [PostalCode] 
    ,[ContactInfo].ref.value(N''declare namespace ci="http://schemas.microsoft.com/sqlserver/2004/07/adventure-works/ContactInfo"; 
        declare namespace act="http://schemas.microsoft.com/sqlserver/2004/07/adventure-works/ContactTypes"; 
        (act:homePostalAddress/act:CountryRegion)[1]'', ''nvarchar(50)'') AS [CountryRegion] 
    ,[ContactInfo].ref.value(N''declare namespace ci="http://schemas.microsoft.com/sqlserver/2004/07/adventure-works/ContactInfo"; 
        declare namespace act="http://schemas.microsoft.com/sqlserver/2004/07/adventure-works/ContactTypes"; 
        (act:homePostalAddress/act:SpecialInstructions/text())[1]'', ''nvarchar(max)'') AS [HomeAddressSpecialInstructions] 
    ,[ContactInfo].ref.value(N''declare namespace ci="http://schemas.microsoft.com/sqlserver/2004/07/adventure-works/ContactInfo"; 
        declare namespace act="http://schemas.microsoft.com/sqlserver/2004/07/adventure-works/ContactTypes"; 
        (act:eMail/act:eMailAddress)[1]'', ''nvarchar(128)'') AS [EMailAddress] 
    ,LTRIM(RTRIM([ContactInfo].ref.value(N''declare namespace ci="http://schemas.microsoft.com/sqlserver/2004/07/adventure-works/ContactInfo"; 
        declare namespace act="http://schemas.microsoft.com/sqlserver/2004/07/adventure-works/ContactTypes"; 
        (act:eMail/act:SpecialInstructions/text())[1]'', ''nvarchar(max)''))) AS [EMailSpecialInstructions] 
    ,[ContactInfo].ref.value(N''declare namespace ci="http://schemas.microsoft.com/sqlserver/2004/07/adventure-works/ContactInfo"; 
        declare namespace act="http://schemas.microsoft.com/sqlserver/2004/07/adventure-works/ContactTypes"; 
        (act:eMail/act:SpecialInstructions/act:telephoneNumber/act:number)[1]'', ''nvarchar(50)'') AS [EMailTelephoneNumber] 
    ,[rowguid] 
    ,[ModifiedDate]
FROM [Person].[Person]
OUTER APPLY [AdditionalContactInfo].nodes(
    ''declare namespace ci="http://schemas.microsoft.com/sqlserver/2004/07/adventure-works/ContactInfo"; 
    /ci:AdditionalContactInfo'') AS ContactInfo(ref) 
WHERE [AdditionalContactInfo] IS NOT NULL;
'
GO
PRINT N'Creating [HumanResources].[vEmployee]'
GO
IF OBJECT_ID(N'[HumanResources].[vEmployee]', 'V') IS NULL
EXEC sp_executesql N'
CREATE VIEW [HumanResources].[vEmployee] 
AS 
SELECT 
    e.[BusinessEntityID]
    ,p.[Title]
    ,p.[FirstName]
    ,p.[MiddleName]
    ,p.[LastName]
    ,p.[Suffix]
    ,e.[JobTitle]  
    ,pp.[PhoneNumber]
    ,pnt.[Name] AS [PhoneNumberType]
    ,ea.[EmailAddress]
    ,p.[EmailPromotion]
    ,a.[AddressLine1]
    ,a.[AddressLine2]
    ,a.[City]
    ,sp.[Name] AS [StateProvinceName] 
    ,a.[PostalCode]
    ,cr.[Name] AS [CountryRegionName] 
    ,p.[AdditionalContactInfo]
FROM [HumanResources].[Employee] e
	INNER JOIN [Person].[Person] p
	ON p.[BusinessEntityID] = e.[BusinessEntityID]
    INNER JOIN [Person].[BusinessEntityAddress] bea 
    ON bea.[BusinessEntityID] = e.[BusinessEntityID] 
    INNER JOIN [Person].[Address] a 
    ON a.[AddressID] = bea.[AddressID]
    INNER JOIN [Person].[StateProvince] sp 
    ON sp.[StateProvinceID] = a.[StateProvinceID]
    INNER JOIN [Person].[CountryRegion] cr 
    ON cr.[CountryRegionCode] = sp.[CountryRegionCode]
    LEFT OUTER JOIN [Person].[PersonPhone] pp
    ON pp.BusinessEntityID = p.[BusinessEntityID]
    LEFT OUTER JOIN [Person].[PhoneNumberType] pnt
    ON pp.[PhoneNumberTypeID] = pnt.[PhoneNumberTypeID]
    LEFT OUTER JOIN [Person].[EmailAddress] ea
    ON p.[BusinessEntityID] = ea.[BusinessEntityID];
'
GO
PRINT N'Creating [Production].[TransactionHistoryArchive]'
GO
IF OBJECT_ID(N'[Production].[TransactionHistoryArchive]', 'U') IS NULL
CREATE TABLE [Production].[TransactionHistoryArchive]
(
[TransactionID] [int] NOT NULL,
[ProductID] [int] NOT NULL,
[ReferenceOrderID] [int] NOT NULL,
[ReferenceOrderLineID] [int] NOT NULL CONSTRAINT [DF_TransactionHistoryArchive_ReferenceOrderLineID] DEFAULT ((0)),
[TransactionDate] [datetime] NOT NULL CONSTRAINT [DF_TransactionHistoryArchive_TransactionDate] DEFAULT (getdate()),
[TransactionType] [nchar] (1) NOT NULL,
[Quantity] [int] NOT NULL,
[ActualCost] [money] NOT NULL,
[ModifiedDate] [datetime] NOT NULL CONSTRAINT [DF_TransactionHistoryArchive_ModifiedDate] DEFAULT (getdate())
)
GO
PRINT N'Creating primary key [PK_TransactionHistoryArchive_TransactionID] on [Production].[TransactionHistoryArchive]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[Production].[PK_TransactionHistoryArchive_TransactionID]', 'PK') AND parent_object_id = OBJECT_ID(N'[Production].[TransactionHistoryArchive]', 'U'))
ALTER TABLE [Production].[TransactionHistoryArchive] ADD CONSTRAINT [PK_TransactionHistoryArchive_TransactionID] PRIMARY KEY CLUSTERED  ([TransactionID])
GO
PRINT N'Creating index [IX_TransactionHistoryArchive_ProductID] on [Production].[TransactionHistoryArchive]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_TransactionHistoryArchive_ProductID' AND object_id = OBJECT_ID(N'[Production].[TransactionHistoryArchive]'))
CREATE NONCLUSTERED INDEX [IX_TransactionHistoryArchive_ProductID] ON [Production].[TransactionHistoryArchive] ([ProductID])
GO
PRINT N'Creating index [IX_TransactionHistoryArchive_ReferenceOrderID_ReferenceOrderLineID] on [Production].[TransactionHistoryArchive]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_TransactionHistoryArchive_ReferenceOrderID_ReferenceOrderLineID' AND object_id = OBJECT_ID(N'[Production].[TransactionHistoryArchive]'))
CREATE NONCLUSTERED INDEX [IX_TransactionHistoryArchive_ReferenceOrderID_ReferenceOrderLineID] ON [Production].[TransactionHistoryArchive] ([ReferenceOrderID], [ReferenceOrderLineID])
GO
PRINT N'Creating [HumanResources].[vEmployeeDepartment]'
GO
IF OBJECT_ID(N'[HumanResources].[vEmployeeDepartment]', 'V') IS NULL
EXEC sp_executesql N'
CREATE VIEW [HumanResources].[vEmployeeDepartment] 
AS 
SELECT 
    e.[BusinessEntityID] 
    ,p.[Title] 
    ,p.[FirstName] 
    ,p.[MiddleName] 
    ,p.[LastName] 
    ,p.[Suffix] 
    ,e.[JobTitle]
    ,d.[Name] AS [Department] 
    ,d.[GroupName] 
    ,edh.[StartDate] 
FROM [HumanResources].[Employee] e
	INNER JOIN [Person].[Person] p
	ON p.[BusinessEntityID] = e.[BusinessEntityID]
    INNER JOIN [HumanResources].[EmployeeDepartmentHistory] edh 
    ON e.[BusinessEntityID] = edh.[BusinessEntityID] 
    INNER JOIN [HumanResources].[Department] d 
    ON edh.[DepartmentID] = d.[DepartmentID] 
WHERE edh.EndDate IS NULL
'
GO
PRINT N'Creating [HumanResources].[vEmployeeDepartmentHistory]'
GO
IF OBJECT_ID(N'[HumanResources].[vEmployeeDepartmentHistory]', 'V') IS NULL
EXEC sp_executesql N'
CREATE VIEW [HumanResources].[vEmployeeDepartmentHistory] 
AS 
SELECT 
    e.[BusinessEntityID] 
    ,p.[Title] 
    ,p.[FirstName] 
    ,p.[MiddleName] 
    ,p.[LastName] 
    ,p.[Suffix] 
    ,s.[Name] AS [Shift]
    ,d.[Name] AS [Department] 
    ,d.[GroupName] 
    ,edh.[StartDate] 
    ,edh.[EndDate]
FROM [HumanResources].[Employee] e
	INNER JOIN [Person].[Person] p
	ON p.[BusinessEntityID] = e.[BusinessEntityID]
    INNER JOIN [HumanResources].[EmployeeDepartmentHistory] edh 
    ON e.[BusinessEntityID] = edh.[BusinessEntityID] 
    INNER JOIN [HumanResources].[Department] d 
    ON edh.[DepartmentID] = d.[DepartmentID] 
    INNER JOIN [HumanResources].[Shift] s
    ON s.[ShiftID] = edh.[ShiftID];
'
GO
PRINT N'Creating [Sales].[vIndividualCustomer]'
GO
IF OBJECT_ID(N'[Sales].[vIndividualCustomer]', 'V') IS NULL
EXEC sp_executesql N'
CREATE VIEW [Sales].[vIndividualCustomer] 
AS 
SELECT 
    p.[BusinessEntityID]
    ,p.[Title]
    ,p.[FirstName]
    ,p.[MiddleName]
    ,p.[LastName]
    ,p.[Suffix]
    ,pp.[PhoneNumber]
	,pnt.[Name] AS [PhoneNumberType]
    ,ea.[EmailAddress]
    ,p.[EmailPromotion]
    ,at.[Name] AS [AddressType]
    ,a.[AddressLine1]
    ,a.[AddressLine2]
    ,a.[City]
    ,[StateProvinceName] = sp.[Name]
    ,a.[PostalCode]
    ,[CountryRegionName] = cr.[Name]
    ,p.[Demographics]
FROM [Person].[Person] p
    INNER JOIN [Person].[BusinessEntityAddress] bea 
    ON bea.[BusinessEntityID] = p.[BusinessEntityID] 
    INNER JOIN [Person].[Address] a 
    ON a.[AddressID] = bea.[AddressID]
    INNER JOIN [Person].[StateProvince] sp 
    ON sp.[StateProvinceID] = a.[StateProvinceID]
    INNER JOIN [Person].[CountryRegion] cr 
    ON cr.[CountryRegionCode] = sp.[CountryRegionCode]
    INNER JOIN [Person].[AddressType] at 
    ON at.[AddressTypeID] = bea.[AddressTypeID]
	INNER JOIN [Sales].[Customer] c
	ON c.[PersonID] = p.[BusinessEntityID]
	LEFT OUTER JOIN [Person].[EmailAddress] ea
	ON ea.[BusinessEntityID] = p.[BusinessEntityID]
	LEFT OUTER JOIN [Person].[PersonPhone] pp
	ON pp.[BusinessEntityID] = p.[BusinessEntityID]
	LEFT OUTER JOIN [Person].[PhoneNumberType] pnt
	ON pnt.[PhoneNumberTypeID] = pp.[PhoneNumberTypeID]
WHERE c.StoreID IS NULL;
'
GO
PRINT N'Creating [Sales].[vPersonDemographics]'
GO
IF OBJECT_ID(N'[Sales].[vPersonDemographics]', 'V') IS NULL
EXEC sp_executesql N'
CREATE VIEW [Sales].[vPersonDemographics] 
AS 
SELECT 
    p.[BusinessEntityID] 
    ,[IndividualSurvey].[ref].[value](N''declare default element namespace "http://schemas.microsoft.com/sqlserver/2004/07/adventure-works/IndividualSurvey"; 
        TotalPurchaseYTD[1]'', ''money'') AS [TotalPurchaseYTD] 
    ,CONVERT(datetime, REPLACE([IndividualSurvey].[ref].[value](N''declare default element namespace "http://schemas.microsoft.com/sqlserver/2004/07/adventure-works/IndividualSurvey"; 
        DateFirstPurchase[1]'', ''nvarchar(20)'') ,''Z'', ''''), 101) AS [DateFirstPurchase] 
    ,CONVERT(datetime, REPLACE([IndividualSurvey].[ref].[value](N''declare default element namespace "http://schemas.microsoft.com/sqlserver/2004/07/adventure-works/IndividualSurvey"; 
        BirthDate[1]'', ''nvarchar(20)'') ,''Z'', ''''), 101) AS [BirthDate] 
    ,[IndividualSurvey].[ref].[value](N''declare default element namespace "http://schemas.microsoft.com/sqlserver/2004/07/adventure-works/IndividualSurvey"; 
        MaritalStatus[1]'', ''nvarchar(1)'') AS [MaritalStatus] 
    ,[IndividualSurvey].[ref].[value](N''declare default element namespace "http://schemas.microsoft.com/sqlserver/2004/07/adventure-works/IndividualSurvey"; 
        YearlyIncome[1]'', ''nvarchar(30)'') AS [YearlyIncome] 
    ,[IndividualSurvey].[ref].[value](N''declare default element namespace "http://schemas.microsoft.com/sqlserver/2004/07/adventure-works/IndividualSurvey"; 
        Gender[1]'', ''nvarchar(1)'') AS [Gender] 
    ,[IndividualSurvey].[ref].[value](N''declare default element namespace "http://schemas.microsoft.com/sqlserver/2004/07/adventure-works/IndividualSurvey"; 
        TotalChildren[1]'', ''integer'') AS [TotalChildren] 
    ,[IndividualSurvey].[ref].[value](N''declare default element namespace "http://schemas.microsoft.com/sqlserver/2004/07/adventure-works/IndividualSurvey"; 
        NumberChildrenAtHome[1]'', ''integer'') AS [NumberChildrenAtHome] 
    ,[IndividualSurvey].[ref].[value](N''declare default element namespace "http://schemas.microsoft.com/sqlserver/2004/07/adventure-works/IndividualSurvey"; 
        Education[1]'', ''nvarchar(30)'') AS [Education] 
    ,[IndividualSurvey].[ref].[value](N''declare default element namespace "http://schemas.microsoft.com/sqlserver/2004/07/adventure-works/IndividualSurvey"; 
        Occupation[1]'', ''nvarchar(30)'') AS [Occupation] 
    ,[IndividualSurvey].[ref].[value](N''declare default element namespace "http://schemas.microsoft.com/sqlserver/2004/07/adventure-works/IndividualSurvey"; 
        HomeOwnerFlag[1]'', ''bit'') AS [HomeOwnerFlag] 
    ,[IndividualSurvey].[ref].[value](N''declare default element namespace "http://schemas.microsoft.com/sqlserver/2004/07/adventure-works/IndividualSurvey"; 
        NumberCarsOwned[1]'', ''integer'') AS [NumberCarsOwned] 
FROM [Person].[Person] p 
CROSS APPLY p.[Demographics].nodes(N''declare default element namespace "http://schemas.microsoft.com/sqlserver/2004/07/adventure-works/IndividualSurvey"; 
    /IndividualSurvey'') AS [IndividualSurvey](ref) 
WHERE [Demographics] IS NOT NULL;
'
GO
PRINT N'Creating [HumanResources].[vJobCandidate]'
GO
IF OBJECT_ID(N'[HumanResources].[vJobCandidate]', 'V') IS NULL
EXEC sp_executesql N'
CREATE VIEW [HumanResources].[vJobCandidate] 
AS 
SELECT 
    jc.[JobCandidateID] 
    ,jc.[BusinessEntityID] 
    ,[Resume].ref.value(N''declare default element namespace "http://schemas.microsoft.com/sqlserver/2004/07/adventure-works/Resume"; 
        (/Resume/Name/Name.Prefix)[1]'', ''nvarchar(30)'') AS [Name.Prefix] 
    ,[Resume].ref.value(N''declare default element namespace "http://schemas.microsoft.com/sqlserver/2004/07/adventure-works/Resume";
        (/Resume/Name/Name.First)[1]'', ''nvarchar(30)'') AS [Name.First] 
    ,[Resume].ref.value(N''declare default element namespace "http://schemas.microsoft.com/sqlserver/2004/07/adventure-works/Resume"; 
        (/Resume/Name/Name.Middle)[1]'', ''nvarchar(30)'') AS [Name.Middle] 
    ,[Resume].ref.value(N''declare default element namespace "http://schemas.microsoft.com/sqlserver/2004/07/adventure-works/Resume"; 
        (/Resume/Name/Name.Last)[1]'', ''nvarchar(30)'') AS [Name.Last] 
    ,[Resume].ref.value(N''declare default element namespace "http://schemas.microsoft.com/sqlserver/2004/07/adventure-works/Resume"; 
        (/Resume/Name/Name.Suffix)[1]'', ''nvarchar(30)'') AS [Name.Suffix] 
    ,[Resume].ref.value(N''declare default element namespace "http://schemas.microsoft.com/sqlserver/2004/07/adventure-works/Resume"; 
        (/Resume/Skills)[1]'', ''nvarchar(max)'') AS [Skills] 
    ,[Resume].ref.value(N''declare default element namespace "http://schemas.microsoft.com/sqlserver/2004/07/adventure-works/Resume"; 
        (Address/Addr.Type)[1]'', ''nvarchar(30)'') AS [Addr.Type]
    ,[Resume].ref.value(N''declare default element namespace "http://schemas.microsoft.com/sqlserver/2004/07/adventure-works/Resume"; 
        (Address/Addr.Location/Location/Loc.CountryRegion)[1]'', ''nvarchar(100)'') AS [Addr.Loc.CountryRegion]
    ,[Resume].ref.value(N''declare default element namespace "http://schemas.microsoft.com/sqlserver/2004/07/adventure-works/Resume"; 
        (Address/Addr.Location/Location/Loc.State)[1]'', ''nvarchar(100)'') AS [Addr.Loc.State]
    ,[Resume].ref.value(N''declare default element namespace "http://schemas.microsoft.com/sqlserver/2004/07/adventure-works/Resume"; 
        (Address/Addr.Location/Location/Loc.City)[1]'', ''nvarchar(100)'') AS [Addr.Loc.City]
    ,[Resume].ref.value(N''declare default element namespace "http://schemas.microsoft.com/sqlserver/2004/07/adventure-works/Resume"; 
        (Address/Addr.PostalCode)[1]'', ''nvarchar(20)'') AS [Addr.PostalCode]
    ,[Resume].ref.value(N''declare default element namespace "http://schemas.microsoft.com/sqlserver/2004/07/adventure-works/Resume"; 
        (/Resume/EMail)[1]'', ''nvarchar(max)'') AS [EMail] 
    ,[Resume].ref.value(N''declare default element namespace "http://schemas.microsoft.com/sqlserver/2004/07/adventure-works/Resume"; 
        (/Resume/WebSite)[1]'', ''nvarchar(max)'') AS [WebSite] 
    ,jc.[ModifiedDate] 
FROM [HumanResources].[JobCandidate] jc 
CROSS APPLY jc.[Resume].nodes(N''declare default element namespace "http://schemas.microsoft.com/sqlserver/2004/07/adventure-works/Resume"; 
    /Resume'') AS Resume(ref);
'
GO
PRINT N'Creating [HumanResources].[vJobCandidateEmployment]'
GO
IF OBJECT_ID(N'[HumanResources].[vJobCandidateEmployment]', 'V') IS NULL
EXEC sp_executesql N'
CREATE VIEW [HumanResources].[vJobCandidateEmployment] 
AS 
SELECT 
    jc.[JobCandidateID] 
    ,CONVERT(datetime, REPLACE([Employment].ref.value(N''declare default element namespace "http://schemas.microsoft.com/sqlserver/2004/07/adventure-works/Resume"; 
        (Emp.StartDate)[1]'', ''nvarchar(20)'') ,''Z'', ''''), 101) AS [Emp.StartDate] 
    ,CONVERT(datetime, REPLACE([Employment].ref.value(N''declare default element namespace "http://schemas.microsoft.com/sqlserver/2004/07/adventure-works/Resume"; 
        (Emp.EndDate)[1]'', ''nvarchar(20)'') ,''Z'', ''''), 101) AS [Emp.EndDate] 
    ,[Employment].ref.value(N''declare default element namespace "http://schemas.microsoft.com/sqlserver/2004/07/adventure-works/Resume"; 
        (Emp.OrgName)[1]'', ''nvarchar(100)'') AS [Emp.OrgName]
    ,[Employment].ref.value(N''declare default element namespace "http://schemas.microsoft.com/sqlserver/2004/07/adventure-works/Resume"; 
        (Emp.JobTitle)[1]'', ''nvarchar(100)'') AS [Emp.JobTitle]
    ,[Employment].ref.value(N''declare default element namespace "http://schemas.microsoft.com/sqlserver/2004/07/adventure-works/Resume"; 
        (Emp.Responsibility)[1]'', ''nvarchar(max)'') AS [Emp.Responsibility]
    ,[Employment].ref.value(N''declare default element namespace "http://schemas.microsoft.com/sqlserver/2004/07/adventure-works/Resume"; 
        (Emp.FunctionCategory)[1]'', ''nvarchar(max)'') AS [Emp.FunctionCategory]
    ,[Employment].ref.value(N''declare default element namespace "http://schemas.microsoft.com/sqlserver/2004/07/adventure-works/Resume"; 
        (Emp.IndustryCategory)[1]'', ''nvarchar(max)'') AS [Emp.IndustryCategory]
    ,[Employment].ref.value(N''declare default element namespace "http://schemas.microsoft.com/sqlserver/2004/07/adventure-works/Resume"; 
        (Emp.Location/Location/Loc.CountryRegion)[1]'', ''nvarchar(max)'') AS [Emp.Loc.CountryRegion]
    ,[Employment].ref.value(N''declare default element namespace "http://schemas.microsoft.com/sqlserver/2004/07/adventure-works/Resume"; 
        (Emp.Location/Location/Loc.State)[1]'', ''nvarchar(max)'') AS [Emp.Loc.State]
    ,[Employment].ref.value(N''declare default element namespace "http://schemas.microsoft.com/sqlserver/2004/07/adventure-works/Resume"; 
        (Emp.Location/Location/Loc.City)[1]'', ''nvarchar(max)'') AS [Emp.Loc.City]
FROM [HumanResources].[JobCandidate] jc 
CROSS APPLY jc.[Resume].nodes(N''declare default element namespace "http://schemas.microsoft.com/sqlserver/2004/07/adventure-works/Resume"; 
    /Resume/Employment'') AS Employment(ref);
'
GO
PRINT N'Creating [HumanResources].[vJobCandidateEducation]'
GO
IF OBJECT_ID(N'[HumanResources].[vJobCandidateEducation]', 'V') IS NULL
EXEC sp_executesql N'
CREATE VIEW [HumanResources].[vJobCandidateEducation] 
AS 
SELECT 
    jc.[JobCandidateID] 
    ,[Education].ref.value(N''declare default element namespace "http://schemas.microsoft.com/sqlserver/2004/07/adventure-works/Resume"; 
        (Edu.Level)[1]'', ''nvarchar(max)'') AS [Edu.Level]
    ,CONVERT(datetime, REPLACE([Education].ref.value(N''declare default element namespace "http://schemas.microsoft.com/sqlserver/2004/07/adventure-works/Resume"; 
        (Edu.StartDate)[1]'', ''nvarchar(20)'') ,''Z'', ''''), 101) AS [Edu.StartDate] 
    ,CONVERT(datetime, REPLACE([Education].ref.value(N''declare default element namespace "http://schemas.microsoft.com/sqlserver/2004/07/adventure-works/Resume"; 
        (Edu.EndDate)[1]'', ''nvarchar(20)'') ,''Z'', ''''), 101) AS [Edu.EndDate] 
    ,[Education].ref.value(N''declare default element namespace "http://schemas.microsoft.com/sqlserver/2004/07/adventure-works/Resume"; 
        (Edu.Degree)[1]'', ''nvarchar(50)'') AS [Edu.Degree]
    ,[Education].ref.value(N''declare default element namespace "http://schemas.microsoft.com/sqlserver/2004/07/adventure-works/Resume"; 
        (Edu.Major)[1]'', ''nvarchar(50)'') AS [Edu.Major]
    ,[Education].ref.value(N''declare default element namespace "http://schemas.microsoft.com/sqlserver/2004/07/adventure-works/Resume"; 
        (Edu.Minor)[1]'', ''nvarchar(50)'') AS [Edu.Minor]
    ,[Education].ref.value(N''declare default element namespace "http://schemas.microsoft.com/sqlserver/2004/07/adventure-works/Resume"; 
        (Edu.GPA)[1]'', ''nvarchar(5)'') AS [Edu.GPA]
    ,[Education].ref.value(N''declare default element namespace "http://schemas.microsoft.com/sqlserver/2004/07/adventure-works/Resume"; 
        (Edu.GPAScale)[1]'', ''nvarchar(5)'') AS [Edu.GPAScale]
    ,[Education].ref.value(N''declare default element namespace "http://schemas.microsoft.com/sqlserver/2004/07/adventure-works/Resume"; 
        (Edu.School)[1]'', ''nvarchar(100)'') AS [Edu.School]
    ,[Education].ref.value(N''declare default element namespace "http://schemas.microsoft.com/sqlserver/2004/07/adventure-works/Resume"; 
        (Edu.Location/Location/Loc.CountryRegion)[1]'', ''nvarchar(100)'') AS [Edu.Loc.CountryRegion]
    ,[Education].ref.value(N''declare default element namespace "http://schemas.microsoft.com/sqlserver/2004/07/adventure-works/Resume"; 
        (Edu.Location/Location/Loc.State)[1]'', ''nvarchar(100)'') AS [Edu.Loc.State]
    ,[Education].ref.value(N''declare default element namespace "http://schemas.microsoft.com/sqlserver/2004/07/adventure-works/Resume"; 
        (Edu.Location/Location/Loc.City)[1]'', ''nvarchar(100)'') AS [Edu.Loc.City]
FROM [HumanResources].[JobCandidate] jc 
CROSS APPLY jc.[Resume].nodes(N''declare default element namespace "http://schemas.microsoft.com/sqlserver/2004/07/adventure-works/Resume"; 
    /Resume/Education'') AS [Education](ref);
'
GO
PRINT N'Creating [Production].[vProductAndDescription]'
GO
IF OBJECT_ID(N'[Production].[vProductAndDescription]', 'V') IS NULL
EXEC sp_executesql N'
CREATE VIEW [Production].[vProductAndDescription] 
WITH SCHEMABINDING 
AS 
-- View (indexed or standard) to display products and product descriptions by language.
SELECT 
    p.[ProductID] 
    ,p.[Name] 
    ,pm.[Name] AS [ProductModel] 
    ,pmx.[CultureID] 
    ,pd.[Description] 
FROM [Production].[Product] p 
    INNER JOIN [Production].[ProductModel] pm 
    ON p.[ProductModelID] = pm.[ProductModelID] 
    INNER JOIN [Production].[ProductModelProductDescriptionCulture] pmx 
    ON pm.[ProductModelID] = pmx.[ProductModelID] 
    INNER JOIN [Production].[ProductDescription] pd 
    ON pmx.[ProductDescriptionID] = pd.[ProductDescriptionID];
'
GO
PRINT N'Creating index [IX_vProductAndDescription] on [Production].[vProductAndDescription]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_vProductAndDescription' AND object_id = OBJECT_ID(N'[Production].[vProductAndDescription]'))
CREATE UNIQUE CLUSTERED INDEX [IX_vProductAndDescription] ON [Production].[vProductAndDescription] ([CultureID], [ProductID])
GO
PRINT N'Creating [Production].[vProductModelCatalogDescription]'
GO
IF OBJECT_ID(N'[Production].[vProductModelCatalogDescription]', 'V') IS NULL
EXEC sp_executesql N'
CREATE VIEW [Production].[vProductModelCatalogDescription] 
AS 
SELECT 
    [ProductModelID] 
    ,[Name] 
    ,[CatalogDescription].value(N''declare namespace p1="http://schemas.microsoft.com/sqlserver/2004/07/adventure-works/ProductModelDescription"; 
        declare namespace html="http://www.w3.org/1999/xhtml"; 
        (/p1:ProductDescription/p1:Summary/html:p)[1]'', ''nvarchar(max)'') AS [Summary] 
    ,[CatalogDescription].value(N''declare namespace p1="http://schemas.microsoft.com/sqlserver/2004/07/adventure-works/ProductModelDescription"; 
        (/p1:ProductDescription/p1:Manufacturer/p1:Name)[1]'', ''nvarchar(max)'') AS [Manufacturer] 
    ,[CatalogDescription].value(N''declare namespace p1="http://schemas.microsoft.com/sqlserver/2004/07/adventure-works/ProductModelDescription"; 
        (/p1:ProductDescription/p1:Manufacturer/p1:Copyright)[1]'', ''nvarchar(30)'') AS [Copyright] 
    ,[CatalogDescription].value(N''declare namespace p1="http://schemas.microsoft.com/sqlserver/2004/07/adventure-works/ProductModelDescription"; 
        (/p1:ProductDescription/p1:Manufacturer/p1:ProductURL)[1]'', ''nvarchar(256)'') AS [ProductURL] 
    ,[CatalogDescription].value(N''declare namespace p1="http://schemas.microsoft.com/sqlserver/2004/07/adventure-works/ProductModelDescription"; 
        declare namespace wm="http://schemas.microsoft.com/sqlserver/2004/07/adventure-works/ProductModelWarrAndMain"; 
        (/p1:ProductDescription/p1:Features/wm:Warranty/wm:WarrantyPeriod)[1]'', ''nvarchar(256)'') AS [WarrantyPeriod] 
    ,[CatalogDescription].value(N''declare namespace p1="http://schemas.microsoft.com/sqlserver/2004/07/adventure-works/ProductModelDescription"; 
        declare namespace wm="http://schemas.microsoft.com/sqlserver/2004/07/adventure-works/ProductModelWarrAndMain"; 
        (/p1:ProductDescription/p1:Features/wm:Warranty/wm:Description)[1]'', ''nvarchar(256)'') AS [WarrantyDescription] 
    ,[CatalogDescription].value(N''declare namespace p1="http://schemas.microsoft.com/sqlserver/2004/07/adventure-works/ProductModelDescription"; 
        declare namespace wm="http://schemas.microsoft.com/sqlserver/2004/07/adventure-works/ProductModelWarrAndMain"; 
        (/p1:ProductDescription/p1:Features/wm:Maintenance/wm:NoOfYears)[1]'', ''nvarchar(256)'') AS [NoOfYears] 
    ,[CatalogDescription].value(N''declare namespace p1="http://schemas.microsoft.com/sqlserver/2004/07/adventure-works/ProductModelDescription"; 
        declare namespace wm="http://schemas.microsoft.com/sqlserver/2004/07/adventure-works/ProductModelWarrAndMain"; 
        (/p1:ProductDescription/p1:Features/wm:Maintenance/wm:Description)[1]'', ''nvarchar(256)'') AS [MaintenanceDescription] 
    ,[CatalogDescription].value(N''declare namespace p1="http://schemas.microsoft.com/sqlserver/2004/07/adventure-works/ProductModelDescription"; 
        declare namespace wf="http://www.adventure-works.com/schemas/OtherFeatures"; 
        (/p1:ProductDescription/p1:Features/wf:wheel)[1]'', ''nvarchar(256)'') AS [Wheel] 
    ,[CatalogDescription].value(N''declare namespace p1="http://schemas.microsoft.com/sqlserver/2004/07/adventure-works/ProductModelDescription"; 
        declare namespace wf="http://www.adventure-works.com/schemas/OtherFeatures"; 
        (/p1:ProductDescription/p1:Features/wf:saddle)[1]'', ''nvarchar(256)'') AS [Saddle] 
    ,[CatalogDescription].value(N''declare namespace p1="http://schemas.microsoft.com/sqlserver/2004/07/adventure-works/ProductModelDescription"; 
        declare namespace wf="http://www.adventure-works.com/schemas/OtherFeatures"; 
        (/p1:ProductDescription/p1:Features/wf:pedal)[1]'', ''nvarchar(256)'') AS [Pedal] 
    ,[CatalogDescription].value(N''declare namespace p1="http://schemas.microsoft.com/sqlserver/2004/07/adventure-works/ProductModelDescription"; 
        declare namespace wf="http://www.adventure-works.com/schemas/OtherFeatures"; 
        (/p1:ProductDescription/p1:Features/wf:BikeFrame)[1]'', ''nvarchar(max)'') AS [BikeFrame] 
    ,[CatalogDescription].value(N''declare namespace p1="http://schemas.microsoft.com/sqlserver/2004/07/adventure-works/ProductModelDescription"; 
        declare namespace wf="http://www.adventure-works.com/schemas/OtherFeatures"; 
        (/p1:ProductDescription/p1:Features/wf:crankset)[1]'', ''nvarchar(256)'') AS [Crankset] 
    ,[CatalogDescription].value(N''declare namespace p1="http://schemas.microsoft.com/sqlserver/2004/07/adventure-works/ProductModelDescription"; 
        (/p1:ProductDescription/p1:Picture/p1:Angle)[1]'', ''nvarchar(256)'') AS [PictureAngle] 
    ,[CatalogDescription].value(N''declare namespace p1="http://schemas.microsoft.com/sqlserver/2004/07/adventure-works/ProductModelDescription"; 
        (/p1:ProductDescription/p1:Picture/p1:Size)[1]'', ''nvarchar(256)'') AS [PictureSize] 
    ,[CatalogDescription].value(N''declare namespace p1="http://schemas.microsoft.com/sqlserver/2004/07/adventure-works/ProductModelDescription"; 
        (/p1:ProductDescription/p1:Picture/p1:ProductPhotoID)[1]'', ''nvarchar(256)'') AS [ProductPhotoID] 
    ,[CatalogDescription].value(N''declare namespace p1="http://schemas.microsoft.com/sqlserver/2004/07/adventure-works/ProductModelDescription"; 
        (/p1:ProductDescription/p1:Specifications/Material)[1]'', ''nvarchar(256)'') AS [Material] 
    ,[CatalogDescription].value(N''declare namespace p1="http://schemas.microsoft.com/sqlserver/2004/07/adventure-works/ProductModelDescription"; 
        (/p1:ProductDescription/p1:Specifications/Color)[1]'', ''nvarchar(256)'') AS [Color] 
    ,[CatalogDescription].value(N''declare namespace p1="http://schemas.microsoft.com/sqlserver/2004/07/adventure-works/ProductModelDescription"; 
        (/p1:ProductDescription/p1:Specifications/ProductLine)[1]'', ''nvarchar(256)'') AS [ProductLine] 
    ,[CatalogDescription].value(N''declare namespace p1="http://schemas.microsoft.com/sqlserver/2004/07/adventure-works/ProductModelDescription"; 
        (/p1:ProductDescription/p1:Specifications/Style)[1]'', ''nvarchar(256)'') AS [Style] 
    ,[CatalogDescription].value(N''declare namespace p1="http://schemas.microsoft.com/sqlserver/2004/07/adventure-works/ProductModelDescription"; 
        (/p1:ProductDescription/p1:Specifications/RiderExperience)[1]'', ''nvarchar(1024)'') AS [RiderExperience] 
    ,[rowguid] 
    ,[ModifiedDate]
FROM [Production].[ProductModel] 
WHERE [CatalogDescription] IS NOT NULL;
'
GO
PRINT N'Creating [Production].[vProductModelInstructions]'
GO
IF OBJECT_ID(N'[Production].[vProductModelInstructions]', 'V') IS NULL
EXEC sp_executesql N'
CREATE VIEW [Production].[vProductModelInstructions] 
AS 
SELECT 
    [ProductModelID] 
    ,[Name] 
    ,[Instructions].value(N''declare default element namespace "http://schemas.microsoft.com/sqlserver/2004/07/adventure-works/ProductModelManuInstructions"; 
        (/root/text())[1]'', ''nvarchar(max)'') AS [Instructions] 
    ,[MfgInstructions].ref.value(''@LocationID[1]'', ''int'') AS [LocationID] 
    ,[MfgInstructions].ref.value(''@SetupHours[1]'', ''decimal(9, 4)'') AS [SetupHours] 
    ,[MfgInstructions].ref.value(''@MachineHours[1]'', ''decimal(9, 4)'') AS [MachineHours] 
    ,[MfgInstructions].ref.value(''@LaborHours[1]'', ''decimal(9, 4)'') AS [LaborHours] 
    ,[MfgInstructions].ref.value(''@LotSize[1]'', ''int'') AS [LotSize] 
    ,[Steps].ref.value(''string(.)[1]'', ''nvarchar(1024)'') AS [Step] 
    ,[rowguid] 
    ,[ModifiedDate]
FROM [Production].[ProductModel] 
CROSS APPLY [Instructions].nodes(N''declare default element namespace "http://schemas.microsoft.com/sqlserver/2004/07/adventure-works/ProductModelManuInstructions"; 
    /root/Location'') MfgInstructions(ref)
CROSS APPLY [MfgInstructions].ref.nodes(''declare default element namespace "http://schemas.microsoft.com/sqlserver/2004/07/adventure-works/ProductModelManuInstructions"; 
    step'') Steps(ref);
'
GO
PRINT N'Creating [Sales].[vSalesPerson]'
GO
IF OBJECT_ID(N'[Sales].[vSalesPerson]', 'V') IS NULL
EXEC sp_executesql N'
CREATE VIEW [Sales].[vSalesPerson] 
AS 
SELECT 
    s.[BusinessEntityID]
    ,p.[Title]
    ,p.[FirstName]
    ,p.[MiddleName]
    ,p.[LastName]
    ,p.[Suffix]
    ,e.[JobTitle]
    ,pp.[PhoneNumber]
	,pnt.[Name] AS [PhoneNumberType]
    ,ea.[EmailAddress]
    ,p.[EmailPromotion]
    ,a.[AddressLine1]
    ,a.[AddressLine2]
    ,a.[City]
    ,[StateProvinceName] = sp.[Name]
    ,a.[PostalCode]
    ,[CountryRegionName] = cr.[Name]
    ,[TerritoryName] = st.[Name]
    ,[TerritoryGroup] = st.[Group]
    ,s.[SalesQuota]
    ,s.[SalesYTD]
    ,s.[SalesLastYear]
FROM [Sales].[SalesPerson] s
    INNER JOIN [HumanResources].[Employee] e 
    ON e.[BusinessEntityID] = s.[BusinessEntityID]
	INNER JOIN [Person].[Person] p
	ON p.[BusinessEntityID] = s.[BusinessEntityID]
    INNER JOIN [Person].[BusinessEntityAddress] bea 
    ON bea.[BusinessEntityID] = s.[BusinessEntityID] 
    INNER JOIN [Person].[Address] a 
    ON a.[AddressID] = bea.[AddressID]
    INNER JOIN [Person].[StateProvince] sp 
    ON sp.[StateProvinceID] = a.[StateProvinceID]
    INNER JOIN [Person].[CountryRegion] cr 
    ON cr.[CountryRegionCode] = sp.[CountryRegionCode]
    LEFT OUTER JOIN [Sales].[SalesTerritory] st 
    ON st.[TerritoryID] = s.[TerritoryID]
	LEFT OUTER JOIN [Person].[EmailAddress] ea
	ON ea.[BusinessEntityID] = p.[BusinessEntityID]
	LEFT OUTER JOIN [Person].[PersonPhone] pp
	ON pp.[BusinessEntityID] = p.[BusinessEntityID]
	LEFT OUTER JOIN [Person].[PhoneNumberType] pnt
	ON pnt.[PhoneNumberTypeID] = pp.[PhoneNumberTypeID];
'
GO
PRINT N'Creating [Sales].[vSalesPersonSalesByFiscalYears]'
GO
IF OBJECT_ID(N'[Sales].[vSalesPersonSalesByFiscalYears]', 'V') IS NULL
EXEC sp_executesql N'
CREATE VIEW [Sales].[vSalesPersonSalesByFiscalYears] 
AS 
SELECT 
    pvt.[SalesPersonID]
    ,pvt.[FullName]
    ,pvt.[JobTitle]
    ,pvt.[SalesTerritory]
    ,pvt.[2002]
    ,pvt.[2003]
    ,pvt.[2004] 
FROM (SELECT 
        soh.[SalesPersonID]
        ,p.[FirstName] + '' '' + COALESCE(p.[MiddleName], '''') + '' '' + p.[LastName] AS [FullName]
        ,e.[JobTitle]
        ,st.[Name] AS [SalesTerritory]
        ,soh.[SubTotal]
        ,YEAR(DATEADD(m, 6, soh.[OrderDate])) AS [FiscalYear] 
    FROM [Sales].[SalesPerson] sp 
        INNER JOIN [Sales].[SalesOrderHeader] soh 
        ON sp.[BusinessEntityID] = soh.[SalesPersonID]
        INNER JOIN [Sales].[SalesTerritory] st 
        ON sp.[TerritoryID] = st.[TerritoryID] 
        INNER JOIN [HumanResources].[Employee] e 
        ON soh.[SalesPersonID] = e.[BusinessEntityID] 
		INNER JOIN [Person].[Person] p
		ON p.[BusinessEntityID] = sp.[BusinessEntityID]
	 ) AS soh 
PIVOT 
(
    SUM([SubTotal]) 
    FOR [FiscalYear] 
    IN ([2002], [2003], [2004])
) AS pvt;
'
GO
PRINT N'Creating [Person].[vStateProvinceCountryRegion]'
GO
IF OBJECT_ID(N'[Person].[vStateProvinceCountryRegion]', 'V') IS NULL
EXEC sp_executesql N'
CREATE VIEW [Person].[vStateProvinceCountryRegion] 
WITH SCHEMABINDING 
AS 
SELECT 
    sp.[StateProvinceID] 
    ,sp.[StateProvinceCode] 
    ,sp.[IsOnlyStateProvinceFlag] 
    ,sp.[Name] AS [StateProvinceName] 
    ,sp.[TerritoryID] 
    ,cr.[CountryRegionCode] 
    ,cr.[Name] AS [CountryRegionName]
FROM [Person].[StateProvince] sp 
    INNER JOIN [Person].[CountryRegion] cr 
    ON sp.[CountryRegionCode] = cr.[CountryRegionCode];
'
GO
PRINT N'Creating index [IX_vStateProvinceCountryRegion] on [Person].[vStateProvinceCountryRegion]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_vStateProvinceCountryRegion' AND object_id = OBJECT_ID(N'[Person].[vStateProvinceCountryRegion]'))
CREATE UNIQUE CLUSTERED INDEX [IX_vStateProvinceCountryRegion] ON [Person].[vStateProvinceCountryRegion] ([StateProvinceID], [CountryRegionCode])
GO
PRINT N'Creating [Sales].[vStoreWithDemographics]'
GO
IF OBJECT_ID(N'[Sales].[vStoreWithDemographics]', 'V') IS NULL
EXEC sp_executesql N'
CREATE VIEW [Sales].[vStoreWithDemographics] AS 
SELECT 
    s.[BusinessEntityID] 
    ,s.[Name] 
    ,s.[Demographics].value(''declare default element namespace "http://schemas.microsoft.com/sqlserver/2004/07/adventure-works/StoreSurvey"; 
        (/StoreSurvey/AnnualSales)[1]'', ''money'') AS [AnnualSales] 
    ,s.[Demographics].value(''declare default element namespace "http://schemas.microsoft.com/sqlserver/2004/07/adventure-works/StoreSurvey"; 
        (/StoreSurvey/AnnualRevenue)[1]'', ''money'') AS [AnnualRevenue] 
    ,s.[Demographics].value(''declare default element namespace "http://schemas.microsoft.com/sqlserver/2004/07/adventure-works/StoreSurvey"; 
        (/StoreSurvey/BankName)[1]'', ''nvarchar(50)'') AS [BankName] 
    ,s.[Demographics].value(''declare default element namespace "http://schemas.microsoft.com/sqlserver/2004/07/adventure-works/StoreSurvey"; 
        (/StoreSurvey/BusinessType)[1]'', ''nvarchar(5)'') AS [BusinessType] 
    ,s.[Demographics].value(''declare default element namespace "http://schemas.microsoft.com/sqlserver/2004/07/adventure-works/StoreSurvey"; 
        (/StoreSurvey/YearOpened)[1]'', ''integer'') AS [YearOpened] 
    ,s.[Demographics].value(''declare default element namespace "http://schemas.microsoft.com/sqlserver/2004/07/adventure-works/StoreSurvey"; 
        (/StoreSurvey/Specialty)[1]'', ''nvarchar(50)'') AS [Specialty] 
    ,s.[Demographics].value(''declare default element namespace "http://schemas.microsoft.com/sqlserver/2004/07/adventure-works/StoreSurvey"; 
        (/StoreSurvey/SquareFeet)[1]'', ''integer'') AS [SquareFeet] 
    ,s.[Demographics].value(''declare default element namespace "http://schemas.microsoft.com/sqlserver/2004/07/adventure-works/StoreSurvey"; 
        (/StoreSurvey/Brands)[1]'', ''nvarchar(30)'') AS [Brands] 
    ,s.[Demographics].value(''declare default element namespace "http://schemas.microsoft.com/sqlserver/2004/07/adventure-works/StoreSurvey"; 
        (/StoreSurvey/Internet)[1]'', ''nvarchar(30)'') AS [Internet] 
    ,s.[Demographics].value(''declare default element namespace "http://schemas.microsoft.com/sqlserver/2004/07/adventure-works/StoreSurvey"; 
        (/StoreSurvey/NumberEmployees)[1]'', ''integer'') AS [NumberEmployees] 
FROM [Sales].[Store] s;
'
GO
PRINT N'Creating [Sales].[vStoreWithContacts]'
GO
IF OBJECT_ID(N'[Sales].[vStoreWithContacts]', 'V') IS NULL
EXEC sp_executesql N'
CREATE VIEW [Sales].[vStoreWithContacts] AS 
SELECT 
    s.[BusinessEntityID] 
    ,s.[Name] 
    ,ct.[Name] AS [ContactType] 
    ,p.[Title] 
    ,p.[FirstName] 
    ,p.[MiddleName] 
    ,p.[LastName] 
    ,p.[Suffix] 
    ,pp.[PhoneNumber] 
	,pnt.[Name] AS [PhoneNumberType]
    ,ea.[EmailAddress] 
    ,p.[EmailPromotion] 
FROM [Sales].[Store] s
    INNER JOIN [Person].[BusinessEntityContact] bec 
    ON bec.[BusinessEntityID] = s.[BusinessEntityID]
	INNER JOIN [Person].[ContactType] ct
	ON ct.[ContactTypeID] = bec.[ContactTypeID]
	INNER JOIN [Person].[Person] p
	ON p.[BusinessEntityID] = bec.[PersonID]
	LEFT OUTER JOIN [Person].[EmailAddress] ea
	ON ea.[BusinessEntityID] = p.[BusinessEntityID]
	LEFT OUTER JOIN [Person].[PersonPhone] pp
	ON pp.[BusinessEntityID] = p.[BusinessEntityID]
	LEFT OUTER JOIN [Person].[PhoneNumberType] pnt
	ON pnt.[PhoneNumberTypeID] = pp.[PhoneNumberTypeID];
'
GO
PRINT N'Creating [Sales].[vStoreWithAddresses]'
GO
IF OBJECT_ID(N'[Sales].[vStoreWithAddresses]', 'V') IS NULL
EXEC sp_executesql N'
CREATE VIEW [Sales].[vStoreWithAddresses] AS 
SELECT 
    s.[BusinessEntityID] 
    ,s.[Name] 
    ,at.[Name] AS [AddressType]
    ,a.[AddressLine1] 
    ,a.[AddressLine2] 
    ,a.[City] 
    ,sp.[Name] AS [StateProvinceName] 
    ,a.[PostalCode] 
    ,cr.[Name] AS [CountryRegionName] 
FROM [Sales].[Store] s
    INNER JOIN [Person].[BusinessEntityAddress] bea 
    ON bea.[BusinessEntityID] = s.[BusinessEntityID] 
    INNER JOIN [Person].[Address] a 
    ON a.[AddressID] = bea.[AddressID]
    INNER JOIN [Person].[StateProvince] sp 
    ON sp.[StateProvinceID] = a.[StateProvinceID]
    INNER JOIN [Person].[CountryRegion] cr 
    ON cr.[CountryRegionCode] = sp.[CountryRegionCode]
    INNER JOIN [Person].[AddressType] at 
    ON at.[AddressTypeID] = bea.[AddressTypeID];
'
GO
PRINT N'Creating [Purchasing].[vVendorWithContacts]'
GO
IF OBJECT_ID(N'[Purchasing].[vVendorWithContacts]', 'V') IS NULL
EXEC sp_executesql N'
CREATE VIEW [Purchasing].[vVendorWithContacts] AS 
SELECT 
    v.[BusinessEntityID]
    ,v.[Name]
    ,ct.[Name] AS [ContactType] 
    ,p.[Title] 
    ,p.[FirstName] 
    ,p.[MiddleName] 
    ,p.[LastName] 
    ,p.[Suffix] 
    ,pp.[PhoneNumber] 
	,pnt.[Name] AS [PhoneNumberType]
    ,ea.[EmailAddress] 
    ,p.[EmailPromotion] 
FROM [Purchasing].[Vendor] v
    INNER JOIN [Person].[BusinessEntityContact] bec 
    ON bec.[BusinessEntityID] = v.[BusinessEntityID]
	INNER JOIN [Person].ContactType ct
	ON ct.[ContactTypeID] = bec.[ContactTypeID]
	INNER JOIN [Person].[Person] p
	ON p.[BusinessEntityID] = bec.[PersonID]
	LEFT OUTER JOIN [Person].[EmailAddress] ea
	ON ea.[BusinessEntityID] = p.[BusinessEntityID]
	LEFT OUTER JOIN [Person].[PersonPhone] pp
	ON pp.[BusinessEntityID] = p.[BusinessEntityID]
	LEFT OUTER JOIN [Person].[PhoneNumberType] pnt
	ON pnt.[PhoneNumberTypeID] = pp.[PhoneNumberTypeID];
'
GO
PRINT N'Creating [Purchasing].[vVendorWithAddresses]'
GO
IF OBJECT_ID(N'[Purchasing].[vVendorWithAddresses]', 'V') IS NULL
EXEC sp_executesql N'
CREATE VIEW [Purchasing].[vVendorWithAddresses] AS 
SELECT 
    v.[BusinessEntityID]
    ,v.[Name]
    ,at.[Name] AS [AddressType]
    ,a.[AddressLine1] 
    ,a.[AddressLine2] 
    ,a.[City] 
    ,sp.[Name] AS [StateProvinceName] 
    ,a.[PostalCode] 
    ,cr.[Name] AS [CountryRegionName] 
FROM [Purchasing].[Vendor] v
    INNER JOIN [Person].[BusinessEntityAddress] bea 
    ON bea.[BusinessEntityID] = v.[BusinessEntityID] 
    INNER JOIN [Person].[Address] a 
    ON a.[AddressID] = bea.[AddressID]
    INNER JOIN [Person].[StateProvince] sp 
    ON sp.[StateProvinceID] = a.[StateProvinceID]
    INNER JOIN [Person].[CountryRegion] cr 
    ON cr.[CountryRegionCode] = sp.[CountryRegionCode]
    INNER JOIN [Person].[AddressType] at 
    ON at.[AddressTypeID] = bea.[AddressTypeID];
'
GO
PRINT N'Creating [dbo].[ufnGetContactInformation]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ufnGetContactInformation]') AND (type = 'IF' OR type = 'FN' OR type = 'TF'))
EXEC sp_executesql N'
CREATE FUNCTION [dbo].[ufnGetContactInformation](@PersonID int)
RETURNS @retContactInformation TABLE 
(
    -- Columns returned by the function
    [PersonID] int NOT NULL, 
    [FirstName] [nvarchar](50) NULL, 
    [LastName] [nvarchar](50) NULL, 
	[JobTitle] [nvarchar](50) NULL,
    [BusinessEntityType] [nvarchar](50) NULL
)
AS 
-- Returns the first name, last name, job title and business entity type for the specified contact.
-- Since a contact can serve multiple roles, more than one row may be returned.
BEGIN
	IF @PersonID IS NOT NULL 
		BEGIN
		IF EXISTS(SELECT * FROM [HumanResources].[Employee] e 
					WHERE e.[BusinessEntityID] = @PersonID) 
			INSERT INTO @retContactInformation
				SELECT @PersonID, p.FirstName, p.LastName, e.[JobTitle], ''Employee''
				FROM [HumanResources].[Employee] AS e
					INNER JOIN [Person].[Person] p
					ON p.[BusinessEntityID] = e.[BusinessEntityID]
				WHERE e.[BusinessEntityID] = @PersonID;

		IF EXISTS(SELECT * FROM [Purchasing].[Vendor] AS v
					INNER JOIN [Person].[BusinessEntityContact] bec 
					ON bec.[BusinessEntityID] = v.[BusinessEntityID]
					WHERE bec.[PersonID] = @PersonID)
			INSERT INTO @retContactInformation
				SELECT @PersonID, p.FirstName, p.LastName, ct.[Name], ''Vendor Contact'' 
				FROM [Purchasing].[Vendor] AS v
					INNER JOIN [Person].[BusinessEntityContact] bec 
					ON bec.[BusinessEntityID] = v.[BusinessEntityID]
					INNER JOIN [Person].ContactType ct
					ON ct.[ContactTypeID] = bec.[ContactTypeID]
					INNER JOIN [Person].[Person] p
					ON p.[BusinessEntityID] = bec.[PersonID]
				WHERE bec.[PersonID] = @PersonID;
		
		IF EXISTS(SELECT * FROM [Sales].[Store] AS s
					INNER JOIN [Person].[BusinessEntityContact] bec 
					ON bec.[BusinessEntityID] = s.[BusinessEntityID]
					WHERE bec.[PersonID] = @PersonID)
			INSERT INTO @retContactInformation
				SELECT @PersonID, p.FirstName, p.LastName, ct.[Name], ''Store Contact'' 
				FROM [Sales].[Store] AS s
					INNER JOIN [Person].[BusinessEntityContact] bec 
					ON bec.[BusinessEntityID] = s.[BusinessEntityID]
					INNER JOIN [Person].ContactType ct
					ON ct.[ContactTypeID] = bec.[ContactTypeID]
					INNER JOIN [Person].[Person] p
					ON p.[BusinessEntityID] = bec.[PersonID]
				WHERE bec.[PersonID] = @PersonID;

		IF EXISTS(SELECT * FROM [Person].[Person] AS p
					INNER JOIN [Sales].[Customer] AS c
					ON c.[PersonID] = p.[BusinessEntityID]
					WHERE p.[BusinessEntityID] = @PersonID AND c.[StoreID] IS NULL) 
			INSERT INTO @retContactInformation
				SELECT @PersonID, p.FirstName, p.LastName, NULL, ''Consumer'' 
				FROM [Person].[Person] AS p
					INNER JOIN [Sales].[Customer] AS c
					ON c.[PersonID] = p.[BusinessEntityID]
					WHERE p.[BusinessEntityID] = @PersonID AND c.[StoreID] IS NULL; 
		END

	RETURN;
END;
'
GO
PRINT N'Creating [dbo].[ufnGetProductDealerPrice]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ufnGetProductDealerPrice]') AND (type = 'IF' OR type = 'FN' OR type = 'TF'))
EXEC sp_executesql N'


CREATE FUNCTION [dbo].[ufnGetProductDealerPrice](@ProductID [int], @OrderDate [datetime])
RETURNS [money] 
AS 
-- Returns the dealer price for the product on a specific date.
BEGIN
    DECLARE @DealerPrice money;
    DECLARE @DealerDiscount money;

    SET @DealerDiscount = 0.60  -- 60% of list price

    SELECT @DealerPrice = plph.[ListPrice] * @DealerDiscount 
    FROM [Production].[Product] p 
        INNER JOIN [Production].[ProductListPriceHistory] plph 
        ON p.[ProductID] = plph.[ProductID] 
            AND p.[ProductID] = @ProductID 
            AND @OrderDate BETWEEN plph.[StartDate] AND COALESCE(plph.[EndDate], CONVERT(datetime, ''99991231'', 112)); -- Make sure we get all the prices!

    RETURN @DealerPrice;
END;
'
GO
PRINT N'Creating [dbo].[ufnGetProductListPrice]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ufnGetProductListPrice]') AND (type = 'IF' OR type = 'FN' OR type = 'TF'))
EXEC sp_executesql N'
CREATE FUNCTION [dbo].[ufnGetProductListPrice](@ProductID [int], @OrderDate [datetime])
RETURNS [money] 
AS 
BEGIN
    DECLARE @ListPrice money;

    SELECT @ListPrice = plph.[ListPrice] 
    FROM [Production].[Product] p 
        INNER JOIN [Production].[ProductListPriceHistory] plph 
        ON p.[ProductID] = plph.[ProductID] 
            AND p.[ProductID] = @ProductID 
            AND @OrderDate BETWEEN plph.[StartDate] AND COALESCE(plph.[EndDate], CONVERT(datetime, ''99991231'', 112)); -- Make sure we get all the prices!

    RETURN @ListPrice;
END;
'
GO
PRINT N'Creating [dbo].[ufnGetProductStandardCost]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ufnGetProductStandardCost]') AND (type = 'IF' OR type = 'FN' OR type = 'TF'))
EXEC sp_executesql N'
CREATE FUNCTION [dbo].[ufnGetProductStandardCost](@ProductID [int], @OrderDate [datetime])
RETURNS [money] 
AS 
-- Returns the standard cost for the product on a specific date.
BEGIN
    DECLARE @StandardCost money;

    SELECT @StandardCost = pch.[StandardCost] 
    FROM [Production].[Product] p 
        INNER JOIN [Production].[ProductCostHistory] pch 
        ON p.[ProductID] = pch.[ProductID] 
            AND p.[ProductID] = @ProductID 
            AND @OrderDate BETWEEN pch.[StartDate] AND COALESCE(pch.[EndDate], CONVERT(datetime, ''99991231'', 112)); -- Make sure we get all the prices!

    RETURN @StandardCost;
END;
'
GO
PRINT N'Creating [dbo].[ufnGetStock]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ufnGetStock]') AND (type = 'IF' OR type = 'FN' OR type = 'TF'))
EXEC sp_executesql N'
CREATE FUNCTION [dbo].[ufnGetStock](@ProductID [int])
RETURNS [int] 
AS 
-- Returns the stock level for the product. This function is used internally only
BEGIN
    DECLARE @ret int;
    
    SELECT @ret = SUM(p.[Quantity]) 
    FROM [Production].[ProductInventory] p 
    WHERE p.[ProductID] = @ProductID 
        AND p.[LocationID] = ''6''; -- Only look at inventory in the misc storage
    
    IF (@ret IS NULL) 
        SET @ret = 0
    
    RETURN @ret
END;
'
GO
PRINT N'Creating [Sales].[usp_InsertSalesOrder_ondisk]'
GO
IF OBJECT_ID(N'[Sales].[usp_InsertSalesOrder_ondisk]', 'P') IS NULL
EXEC sp_executesql N'CREATE PROCEDURE [Sales].[usp_InsertSalesOrder_ondisk]
	@SalesOrderID int OUTPUT,
	@DueDate [datetime2](7) ,
	@CustomerID [int] ,
	@BillToAddressID [int] ,
	@ShipToAddressID [int] ,
	@ShipMethodID [int] ,
	@SalesOrderDetails Sales.SalesOrderDetailType_ondisk READONLY,
	@Status [tinyint]  = 1,
	@OnlineOrderFlag [bit] = 1,
	@PurchaseOrderNumber [nvarchar](25) = NULL,
	@AccountNumber [nvarchar](15) = NULL,
	@SalesPersonID [int] = -1,
	@TerritoryID [int] = NULL,
	@CreditCardID [int] = NULL,
	@CreditCardApprovalCode [varchar](15) = NULL,
	@CurrencyRateID [int] = NULL,
	@Comment nvarchar(128) = NULL
AS
BEGIN 
	BEGIN TRAN
	
		DECLARE @OrderDate datetime2 = sysdatetime()

		DECLARE @SubTotal money = 0

		SELECT @SubTotal = ISNULL(SUM(p.ListPrice * (1 - ISNULL(so.DiscountPct, 0))),0)
		FROM @SalesOrderDetails od 
			JOIN Production.Product_ondisk p on od.ProductID=p.ProductID
			LEFT JOIN Sales.SpecialOffer_ondisk so on od.SpecialOfferID=so.SpecialOfferID

		INSERT INTO Sales.SalesOrderHeader_ondisk
		(	DueDate,
			Status,
			OnlineOrderFlag,
			PurchaseOrderNumber,
			AccountNumber,
			CustomerID,
			SalesPersonID,
			TerritoryID,
			BillToAddressID,
			ShipToAddressID,
			ShipMethodID,
			CreditCardID,
			CreditCardApprovalCode,
			CurrencyRateID,
			Comment,
			OrderDate,
			SubTotal,
			ModifiedDate)
		VALUES
		(	
			@DueDate,
			@Status,
			@OnlineOrderFlag,
			@PurchaseOrderNumber,
			@AccountNumber,
			@CustomerID,
			@SalesPersonID,
			@TerritoryID,
			@BillToAddressID,
			@ShipToAddressID,
			@ShipMethodID,
			@CreditCardID,
			@CreditCardApprovalCode,
			@CurrencyRateID,
			@Comment,
			@OrderDate,
			@SubTotal,
			@OrderDate
		)

		SET @SalesOrderID = SCOPE_IDENTITY()

		INSERT INTO Sales.SalesOrderDetail_ondisk
		(
			SalesOrderID,
			OrderQty,
			ProductID,
			SpecialOfferID,
			UnitPrice,
			UnitPriceDiscount,
			ModifiedDate
		)
		SELECT 
			@SalesOrderID,
			od.OrderQty,
			od.ProductID,
			od.SpecialOfferID,
			p.ListPrice,
			ISNULL(p.ListPrice * so.DiscountPct, 0),
			@OrderDate
		FROM @SalesOrderDetails od 
			JOIN Production.Product_ondisk p on od.ProductID=p.ProductID
			LEFT JOIN Sales.SpecialOffer_ondisk so on od.SpecialOfferID=so.SpecialOfferID

	COMMIT
END
'
GO
PRINT N'Creating [dbo].[uspGetBillOfMaterials]'
GO
IF OBJECT_ID(N'[dbo].[uspGetBillOfMaterials]', 'P') IS NULL
EXEC sp_executesql N'
CREATE PROCEDURE [dbo].[uspGetBillOfMaterials]
    @StartProductID [int],
    @CheckDate [datetime]
AS
BEGIN
    SET NOCOUNT ON;

    -- Use recursive query to generate a multi-level Bill of Material (i.e. all level 1 
    -- components of a level 0 assembly, all level 2 components of a level 1 assembly)
    -- The CheckDate eliminates any components that are no longer used in the product on this date.
    WITH [BOM_cte]([ProductAssemblyID], [ComponentID], [ComponentDesc], [PerAssemblyQty], [StandardCost], [ListPrice], [BOMLevel], [RecursionLevel]) -- CTE name and columns
    AS (
        SELECT b.[ProductAssemblyID], b.[ComponentID], p.[Name], b.[PerAssemblyQty], p.[StandardCost], p.[ListPrice], b.[BOMLevel], 0 -- Get the initial list of components for the bike assembly
        FROM [Production].[BillOfMaterials] b
            INNER JOIN [Production].[Product] p 
            ON b.[ComponentID] = p.[ProductID] 
        WHERE b.[ProductAssemblyID] = @StartProductID 
            AND @CheckDate >= b.[StartDate] 
            AND @CheckDate <= ISNULL(b.[EndDate], @CheckDate)
        UNION ALL
        SELECT b.[ProductAssemblyID], b.[ComponentID], p.[Name], b.[PerAssemblyQty], p.[StandardCost], p.[ListPrice], b.[BOMLevel], [RecursionLevel] + 1 -- Join recursive member to anchor
        FROM [BOM_cte] cte
            INNER JOIN [Production].[BillOfMaterials] b 
            ON b.[ProductAssemblyID] = cte.[ComponentID]
            INNER JOIN [Production].[Product] p 
            ON b.[ComponentID] = p.[ProductID] 
        WHERE @CheckDate >= b.[StartDate] 
            AND @CheckDate <= ISNULL(b.[EndDate], @CheckDate)
        )
    -- Outer select from the CTE
    SELECT b.[ProductAssemblyID], b.[ComponentID], b.[ComponentDesc], SUM(b.[PerAssemblyQty]) AS [TotalQuantity] , b.[StandardCost], b.[ListPrice], b.[BOMLevel], b.[RecursionLevel]
    FROM [BOM_cte] b
    GROUP BY b.[ComponentID], b.[ComponentDesc], b.[ProductAssemblyID], b.[BOMLevel], b.[RecursionLevel], b.[StandardCost], b.[ListPrice]
    ORDER BY b.[BOMLevel], b.[ProductAssemblyID], b.[ComponentID]
    OPTION (MAXRECURSION 25) 
END;
'
GO
PRINT N'Creating [Sales].[usp_UpdateSalesOrderShipInfo_native]'
GO
IF OBJECT_ID(N'[Sales].[usp_UpdateSalesOrderShipInfo_native]', 'P') IS NULL
EXEC sp_executesql N'CREATE PROCEDURE [Sales].[usp_UpdateSalesOrderShipInfo_native]
	@SalesOrderID int , 
	@ShipDate datetime2,
	@Comment nvarchar(128),
	@Status tinyint,
	@TaxRate smallmoney,
	@Freight money,
	@CarrierTrackingNumber nvarchar(25)
WITH NATIVE_COMPILATION, SCHEMABINDING
AS
BEGIN ATOMIC WITH
  (TRANSACTION ISOLATION LEVEL = SNAPSHOT,
   LANGUAGE = N''us_english'')

	DECLARE @now datetime2 = SYSDATETIME()

	UPDATE Sales.SalesOrderDetail_inmem 
	SET CarrierTrackingNumber = @CarrierTrackingNumber, ModifiedDate = @now
	WHERE SalesOrderID = @SalesOrderID

	UPDATE Sales.SalesOrderHeader_inmem
	SET RevisionNumber = RevisionNumber + 1,
		ShipDate = @ShipDate,
		Status = @Status,
		TaxAmt = SubTotal * @TaxRate,
		Freight = @Freight,
		ModifiedDate = @now
	WHERE SalesOrderID = @SalesOrderID

END
'
GO
PRINT N'Creating [dbo].[uspGetEmployeeManagers]'
GO
IF OBJECT_ID(N'[dbo].[uspGetEmployeeManagers]', 'P') IS NULL
EXEC sp_executesql N'
CREATE PROCEDURE [dbo].[uspGetEmployeeManagers]
    @BusinessEntityID [int]
AS
BEGIN
    SET NOCOUNT ON;

    -- Use recursive query to list out all Employees required for a particular Manager
    WITH [EMP_cte]([BusinessEntityID], [OrganizationNode], [FirstName], [LastName], [JobTitle], [RecursionLevel]) -- CTE name and columns
    AS (
        SELECT e.[BusinessEntityID], e.[OrganizationNode], p.[FirstName], p.[LastName], e.[JobTitle], 0 -- Get the initial Employee
        FROM [HumanResources].[Employee] e 
			INNER JOIN [Person].[Person] as p
			ON p.[BusinessEntityID] = e.[BusinessEntityID]
        WHERE e.[BusinessEntityID] = @BusinessEntityID
        UNION ALL
        SELECT e.[BusinessEntityID], e.[OrganizationNode], p.[FirstName], p.[LastName], e.[JobTitle], [RecursionLevel] + 1 -- Join recursive member to anchor
        FROM [HumanResources].[Employee] e 
            INNER JOIN [EMP_cte]
            ON e.[OrganizationNode] = [EMP_cte].[OrganizationNode].GetAncestor(1)
            INNER JOIN [Person].[Person] p 
            ON p.[BusinessEntityID] = e.[BusinessEntityID]
    )
    -- Join back to Employee to return the manager name 
    SELECT [EMP_cte].[RecursionLevel], [EMP_cte].[BusinessEntityID], [EMP_cte].[FirstName], [EMP_cte].[LastName], 
        [EMP_cte].[OrganizationNode].ToString() AS [OrganizationNode], p.[FirstName] AS ''ManagerFirstName'', p.[LastName] AS ''ManagerLastName''  -- Outer select from the CTE
    FROM [EMP_cte] 
        INNER JOIN [HumanResources].[Employee] e 
        ON [EMP_cte].[OrganizationNode].GetAncestor(1) = e.[OrganizationNode]
        INNER JOIN [Person].[Person] p 
        ON p.[BusinessEntityID] = e.[BusinessEntityID]
    ORDER BY [RecursionLevel], [EMP_cte].[OrganizationNode].ToString()
    OPTION (MAXRECURSION 25) 
END;
'
GO
PRINT N'Creating [Sales].[usp_UpdateSalesOrderShipInfo_inmem]'
GO
IF OBJECT_ID(N'[Sales].[usp_UpdateSalesOrderShipInfo_inmem]', 'P') IS NULL
EXEC sp_executesql N'-- wrapper stored procedure that contains retry logic to deal with update conflicts
-- alternatively, the client can perform retries in case of conflicts

-- for simplicity, we assume all items in the order are shipped in the same package, and thus have the same carrier tracking number
CREATE PROCEDURE [Sales].[usp_UpdateSalesOrderShipInfo_inmem]
	@SalesOrderID int , 
	@ShipDate datetime2 = NULL,
	@Comment nvarchar(128) = NULL,
	@Status tinyint,
	@TaxRate smallmoney = 0,
	@Freight money,
	@CarrierTrackingNumber nvarchar(25)
AS
BEGIN

  DECLARE @retry INT = 10
  SET @ShipDate = ISNULL(@ShipDate, SYSDATETIME())

  WHILE (@retry > 0)
  BEGIN
    BEGIN TRY

      EXEC Sales.usp_UpdateSalesOrderShipInfo_native
		@SalesOrderID = @SalesOrderID, 
		@ShipDate = @ShipDate,
		@Comment = @Comment,
		@Status = @Status,
		@TaxRate = @TaxRate,
		@Freight = @Freight,
		@CarrierTrackingNumber = @CarrierTrackingNumber


      SET @retry = 0
    END TRY
    BEGIN CATCH
      SET @retry -= 1
  
      IF (@retry > 0 AND error_number() in (41302, 41305, 41325, 41301))
      BEGIN

        IF XACT_STATE() <> 0 
          ROLLBACK TRANSACTION

      END
      ELSE
      BEGIN
        ;THROW
      END
    END CATCH
  END
END
'
GO
PRINT N'Creating [dbo].[uspGetManagerEmployees]'
GO
IF OBJECT_ID(N'[dbo].[uspGetManagerEmployees]', 'P') IS NULL
EXEC sp_executesql N'
CREATE PROCEDURE [dbo].[uspGetManagerEmployees]
    @BusinessEntityID [int]
AS
BEGIN
    SET NOCOUNT ON;

    -- Use recursive query to list out all Employees required for a particular Manager
    WITH [EMP_cte]([BusinessEntityID], [OrganizationNode], [FirstName], [LastName], [RecursionLevel]) -- CTE name and columns
    AS (
        SELECT e.[BusinessEntityID], e.[OrganizationNode], p.[FirstName], p.[LastName], 0 -- Get the initial list of Employees for Manager n
        FROM [HumanResources].[Employee] e 
			INNER JOIN [Person].[Person] p 
			ON p.[BusinessEntityID] = e.[BusinessEntityID]
        WHERE e.[BusinessEntityID] = @BusinessEntityID
        UNION ALL
        SELECT e.[BusinessEntityID], e.[OrganizationNode], p.[FirstName], p.[LastName], [RecursionLevel] + 1 -- Join recursive member to anchor
        FROM [HumanResources].[Employee] e 
            INNER JOIN [EMP_cte]
            ON e.[OrganizationNode].GetAncestor(1) = [EMP_cte].[OrganizationNode]
			INNER JOIN [Person].[Person] p 
			ON p.[BusinessEntityID] = e.[BusinessEntityID]
        )
    -- Join back to Employee to return the manager name 
    SELECT [EMP_cte].[RecursionLevel], [EMP_cte].[OrganizationNode].ToString() as [OrganizationNode], p.[FirstName] AS ''ManagerFirstName'', p.[LastName] AS ''ManagerLastName'',
        [EMP_cte].[BusinessEntityID], [EMP_cte].[FirstName], [EMP_cte].[LastName] -- Outer select from the CTE
    FROM [EMP_cte] 
        INNER JOIN [HumanResources].[Employee] e 
        ON [EMP_cte].[OrganizationNode].GetAncestor(1) = e.[OrganizationNode]
			INNER JOIN [Person].[Person] p 
			ON p.[BusinessEntityID] = e.[BusinessEntityID]
    ORDER BY [RecursionLevel], [EMP_cte].[OrganizationNode].ToString()
    OPTION (MAXRECURSION 25) 
END;
'
GO
PRINT N'Creating [Sales].[usp_UpdateSalesOrderShipInfo_ondisk]'
GO
IF OBJECT_ID(N'[Sales].[usp_UpdateSalesOrderShipInfo_ondisk]', 'P') IS NULL
EXEC sp_executesql N'-- for simplicity, we assume all items in the order are shipped in the same package, and thus have the same carrier tracking number
CREATE PROCEDURE [Sales].[usp_UpdateSalesOrderShipInfo_ondisk]
	@SalesOrderID int , 
	@ShipDate datetime2 = NULL,
	@Comment nvarchar(128) = NULL,
	@Status tinyint,
	@TaxRate smallmoney = 0,
	@Freight money,
	@CarrierTrackingNumber nvarchar(25)
AS
BEGIN
  SET @ShipDate = ISNULL(@ShipDate, SYSDATETIME())

  BEGIN TRAN
	DECLARE @now datetime2 = SYSDATETIME()

	UPDATE Sales.SalesOrderDetail_ondisk 
	SET CarrierTrackingNumber = @CarrierTrackingNumber, ModifiedDate = @now
	WHERE SalesOrderID = @SalesOrderID

	UPDATE Sales.SalesOrderHeader_ondisk
	SET RevisionNumber = RevisionNumber + 1,
		ShipDate = @ShipDate,
		Status = @Status,
		TaxAmt = SubTotal * @TaxRate,
		Freight = @Freight,
		ModifiedDate = @now
	WHERE SalesOrderID = @SalesOrderID
  COMMIT

END
'
GO
PRINT N'Creating [dbo].[uspGetWhereUsedProductID]'
GO
IF OBJECT_ID(N'[dbo].[uspGetWhereUsedProductID]', 'P') IS NULL
EXEC sp_executesql N'
CREATE PROCEDURE [dbo].[uspGetWhereUsedProductID]
    @StartProductID [int],
    @CheckDate [datetime]
AS
BEGIN
    SET NOCOUNT ON;

    --Use recursive query to generate a multi-level Bill of Material (i.e. all level 1 components of a level 0 assembly, all level 2 components of a level 1 assembly)
    WITH [BOM_cte]([ProductAssemblyID], [ComponentID], [ComponentDesc], [PerAssemblyQty], [StandardCost], [ListPrice], [BOMLevel], [RecursionLevel]) -- CTE name and columns
    AS (
        SELECT b.[ProductAssemblyID], b.[ComponentID], p.[Name], b.[PerAssemblyQty], p.[StandardCost], p.[ListPrice], b.[BOMLevel], 0 -- Get the initial list of components for the bike assembly
        FROM [Production].[BillOfMaterials] b
            INNER JOIN [Production].[Product] p 
            ON b.[ProductAssemblyID] = p.[ProductID] 
        WHERE b.[ComponentID] = @StartProductID 
            AND @CheckDate >= b.[StartDate] 
            AND @CheckDate <= ISNULL(b.[EndDate], @CheckDate)
        UNION ALL
        SELECT b.[ProductAssemblyID], b.[ComponentID], p.[Name], b.[PerAssemblyQty], p.[StandardCost], p.[ListPrice], b.[BOMLevel], [RecursionLevel] + 1 -- Join recursive member to anchor
        FROM [BOM_cte] cte
            INNER JOIN [Production].[BillOfMaterials] b 
            ON cte.[ProductAssemblyID] = b.[ComponentID]
            INNER JOIN [Production].[Product] p 
            ON b.[ProductAssemblyID] = p.[ProductID] 
        WHERE @CheckDate >= b.[StartDate] 
            AND @CheckDate <= ISNULL(b.[EndDate], @CheckDate)
        )
    -- Outer select from the CTE
    SELECT b.[ProductAssemblyID], b.[ComponentID], b.[ComponentDesc], SUM(b.[PerAssemblyQty]) AS [TotalQuantity] , b.[StandardCost], b.[ListPrice], b.[BOMLevel], b.[RecursionLevel]
    FROM [BOM_cte] b
    GROUP BY b.[ComponentID], b.[ComponentDesc], b.[ProductAssemblyID], b.[BOMLevel], b.[RecursionLevel], b.[StandardCost], b.[ListPrice]
    ORDER BY b.[BOMLevel], b.[ProductAssemblyID], b.[ComponentID]
    OPTION (MAXRECURSION 25) 
END;
'
GO
PRINT N'Creating [HumanResources].[uspUpdateEmployeeHireInfo]'
GO
IF OBJECT_ID(N'[HumanResources].[uspUpdateEmployeeHireInfo]', 'P') IS NULL
EXEC sp_executesql N'
CREATE PROCEDURE [HumanResources].[uspUpdateEmployeeHireInfo]
    @BusinessEntityID [int], 
    @JobTitle [nvarchar](50), 
    @HireDate [datetime], 
    @RateChangeDate [datetime], 
    @Rate [money], 
    @PayFrequency [tinyint], 
    @CurrentFlag [dbo].[Flag] 
WITH EXECUTE AS CALLER
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        UPDATE [HumanResources].[Employee] 
        SET [JobTitle] = @JobTitle 
            ,[HireDate] = @HireDate 
            ,[CurrentFlag] = @CurrentFlag 
        WHERE [BusinessEntityID] = @BusinessEntityID;

        INSERT INTO [HumanResources].[EmployeePayHistory] 
            ([BusinessEntityID]
            ,[RateChangeDate]
            ,[Rate]
            ,[PayFrequency]) 
        VALUES (@BusinessEntityID, @RateChangeDate, @Rate, @PayFrequency);

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        -- Rollback any active or uncommittable transactions before
        -- inserting information in the ErrorLog
        IF @@TRANCOUNT > 0
        BEGIN
            ROLLBACK TRANSACTION;
        END

        EXECUTE [dbo].[uspLogError];
    END CATCH;
END;
'
GO
PRINT N'Creating [HumanResources].[uspUpdateEmployeeLogin]'
GO
IF OBJECT_ID(N'[HumanResources].[uspUpdateEmployeeLogin]', 'P') IS NULL
EXEC sp_executesql N'
CREATE PROCEDURE [HumanResources].[uspUpdateEmployeeLogin]
    @BusinessEntityID [int], 
    @OrganizationNode [hierarchyid],
    @LoginID [nvarchar](256),
    @JobTitle [nvarchar](50),
    @HireDate [datetime],
    @CurrentFlag [dbo].[Flag]
WITH EXECUTE AS CALLER
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        UPDATE [HumanResources].[Employee] 
        SET [OrganizationNode] = @OrganizationNode 
            ,[LoginID] = @LoginID 
            ,[JobTitle] = @JobTitle 
            ,[HireDate] = @HireDate 
            ,[CurrentFlag] = @CurrentFlag 
        WHERE [BusinessEntityID] = @BusinessEntityID;
    END TRY
    BEGIN CATCH
        EXECUTE [dbo].[uspLogError];
    END CATCH;
END;
'
GO
PRINT N'Creating [HumanResources].[uspUpdateEmployeePersonalInfo]'
GO
IF OBJECT_ID(N'[HumanResources].[uspUpdateEmployeePersonalInfo]', 'P') IS NULL
EXEC sp_executesql N'
CREATE PROCEDURE [HumanResources].[uspUpdateEmployeePersonalInfo]
    @BusinessEntityID [int], 
    @NationalIDNumber [nvarchar](15), 
    @BirthDate [datetime], 
    @MaritalStatus [nchar](1), 
    @Gender [nchar](1)
WITH EXECUTE AS CALLER
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        UPDATE [HumanResources].[Employee] 
        SET [NationalIDNumber] = @NationalIDNumber 
            ,[BirthDate] = @BirthDate 
            ,[MaritalStatus] = @MaritalStatus 
            ,[Gender] = @Gender 
        WHERE [BusinessEntityID] = @BusinessEntityID;
    END TRY
    BEGIN CATCH
        EXECUTE [dbo].[uspLogError];
    END CATCH;
END;
'
GO
PRINT N'Creating [dbo].[uspSearchCandidateResumes]'
GO
IF OBJECT_ID(N'[dbo].[uspSearchCandidateResumes]', 'P') IS NULL
EXEC sp_executesql N'
--A stored procedure which demonstrates integrated full text search

CREATE PROCEDURE [dbo].[uspSearchCandidateResumes]
    @searchString [nvarchar](1000),   
    @useInflectional [bit]=0,
    @useThesaurus [bit]=0,
    @language[int]=0


WITH EXECUTE AS CALLER
AS
BEGIN
    SET NOCOUNT ON;

      DECLARE @string nvarchar(1050)
      --setting the lcid to the default instance LCID if needed
      IF @language = NULL OR @language = 0 
      BEGIN 
            SELECT @language =CONVERT(int, serverproperty(''lcid''))  
      END
      

            --FREETEXTTABLE case as inflectional and Thesaurus were required
      IF @useThesaurus = 1 AND @useInflectional = 1  
        BEGIN
                  SELECT FT_TBL.[JobCandidateID], KEY_TBL.[RANK] FROM [HumanResources].[JobCandidate] AS FT_TBL 
                        INNER JOIN FREETEXTTABLE([HumanResources].[JobCandidate],*, @searchString,LANGUAGE @language) AS KEY_TBL
                   ON  FT_TBL.[JobCandidateID] =KEY_TBL.[KEY]
            END

      ELSE IF @useThesaurus = 1
            BEGIN
                  SELECT @string =''FORMSOF(THESAURUS,"''+@searchString +''"''+'')''      
                  SELECT FT_TBL.[JobCandidateID], KEY_TBL.[RANK] FROM [HumanResources].[JobCandidate] AS FT_TBL 
                        INNER JOIN CONTAINSTABLE([HumanResources].[JobCandidate],*, @string,LANGUAGE @language) AS KEY_TBL
                   ON  FT_TBL.[JobCandidateID] =KEY_TBL.[KEY]
        END

      ELSE IF @useInflectional = 1
            BEGIN
                  SELECT @string =''FORMSOF(INFLECTIONAL,"''+@searchString +''"''+'')''
                  SELECT FT_TBL.[JobCandidateID], KEY_TBL.[RANK] FROM [HumanResources].[JobCandidate] AS FT_TBL 
                        INNER JOIN CONTAINSTABLE([HumanResources].[JobCandidate],*, @string,LANGUAGE @language) AS KEY_TBL
                   ON  FT_TBL.[JobCandidateID] =KEY_TBL.[KEY]
        END
  
      ELSE --base case, plain CONTAINSTABLE
            BEGIN
                  SELECT @string=''"''+@searchString +''"''
                  SELECT FT_TBL.[JobCandidateID],KEY_TBL.[RANK] FROM [HumanResources].[JobCandidate] AS FT_TBL 
                        INNER JOIN CONTAINSTABLE([HumanResources].[JobCandidate],*,@string,LANGUAGE @language) AS KEY_TBL
                   ON  FT_TBL.[JobCandidateID] =KEY_TBL.[KEY]
            END

END;
'
GO
PRINT N'Creating [Demo].[DemoSalesOrderHeaderSeed]'
GO
IF OBJECT_ID(N'[Demo].[DemoSalesOrderHeaderSeed]', 'U') IS NULL
CREATE TABLE [Demo].[DemoSalesOrderHeaderSeed]
(
[DueDate] [datetime2] NOT NULL,
[CustomerID] [int] NOT NULL,
[SalesPersonID] [int] NOT NULL,
[BillToAddressID] [int] NOT NULL,
[ShipToAddressID] [int] NOT NULL,
[ShipMethodID] [int] NOT NULL,
[LocalID] [int] NOT NULL IDENTITY(1, 1),
CONSTRAINT [PK__DemoSale__499359DA31897820] PRIMARY KEY NONCLUSTERED  ([LocalID])
)
WITH
(
MEMORY_OPTIMIZED = ON
)
GO
PRINT N'Creating [Demo].[DemoSalesOrderDetailSeed]'
GO
IF OBJECT_ID(N'[Demo].[DemoSalesOrderDetailSeed]', 'U') IS NULL
CREATE TABLE [Demo].[DemoSalesOrderDetailSeed]
(
[OrderQty] [smallint] NOT NULL,
[ProductID] [int] NOT NULL,
[SpecialOfferID] [int] NOT NULL,
[OrderID] [int] NOT NULL,
[LocalID] [int] NOT NULL IDENTITY(1, 1),
CONSTRAINT [PK__DemoSale__499359DA37DC4424] PRIMARY KEY NONCLUSTERED  ([LocalID]),
INDEX [IX_OrderID] NONCLUSTERED HASH ([OrderID]) WITH (BUCKET_COUNT=1048576)
)
WITH
(
MEMORY_OPTIMIZED = ON
)
GO
PRINT N'Creating [Demo].[usp_DemoInitSeed]'
GO
IF OBJECT_ID(N'[Demo].[usp_DemoInitSeed]', 'P') IS NULL
EXEC sp_executesql N'CREATE PROCEDURE [Demo].[usp_DemoInitSeed] @items_per_order int = 5
AS
BEGIN
	DECLARE @ProductID int, @SpecialOfferID int,
		@i int = 1
	DECLARE @seed_order_count int = (SELECT COUNT(*)/@items_per_order FROM Sales.SpecialOfferProduct_inmem)

	DECLARE seed_cursor CURSOR FOR 
		SELECT 
			ProductID,
			SpecialOfferID 
		FROM Sales.SpecialOfferProduct_inmem

	OPEN seed_cursor

	FETCH NEXT FROM seed_cursor 
	INTO @ProductID, @SpecialOfferID

	BEGIN TRAN

		DELETE FROM Demo.DemoSalesOrderHeaderSeed

		INSERT INTO Demo.DemoSalesOrderHeaderSeed
		(
			DueDate,
			CustomerID,
			SalesPersonID,
			BillToAddressID,
			ShipToAddressID,
			ShipMethodID
		)
		SELECT
			dateadd(d, (rand(BillToAddressID*CustomerID)*10)+1,cast(sysdatetime() as date)),
			CustomerID,
			SalesPersonID,
			BillToAddressID,
			ShipToAddressID,
			ShipMethodID
		FROM Sales.SalesOrderHeader_inmem


		WHILE @@FETCH_STATUS = 0
		BEGIN
			INSERT Demo.DemoSalesOrderDetailSeed
			SELECT 
				@i % 6 + 1,
				@ProductID,
				@SpecialOfferID,
				@i % (@seed_order_count+1)

			SET @i += 1

			FETCH NEXT FROM seed_cursor 
			INTO @ProductID, @SpecialOfferID
		END

		CLOSE seed_cursor
		DEALLOCATE seed_cursor
	COMMIT

	UPDATE STATISTICS Demo.DemoSalesOrderDetailSeed
	WITH FULLSCAN, NORECOMPUTE
END
'
GO
PRINT N'Creating [Demo].[usp_DemoReset]'
GO
IF OBJECT_ID(N'[Demo].[usp_DemoReset]', 'P') IS NULL
EXEC sp_executesql N'CREATE PROCEDURE [Demo].[usp_DemoReset]
AS
BEGIN
	truncate table Sales.SalesOrderDetail_ondisk
	delete from Sales.SalesOrderDetail_inmem
	delete from Sales.SalesOrderHeader_ondisk
	delete from Sales.SalesOrderHeader_inmem
	
	CHECKPOINT

	SET IDENTITY_INSERT Sales.SalesOrderHeader_inmem ON
	INSERT INTO Sales.SalesOrderHeader_inmem
		([SalesOrderID],
		[RevisionNumber],
		[OrderDate],
		[DueDate],
		[ShipDate],
		[Status],
		[OnlineOrderFlag],
		[PurchaseOrderNumber],
		[AccountNumber],
		[CustomerID],
		[SalesPersonID],
		[TerritoryID],
		[BillToAddressID],
		[ShipToAddressID],
		[ShipMethodID],
		[CreditCardID],
		[CreditCardApprovalCode],
		[CurrencyRateID],
		[SubTotal],
		[TaxAmt],
		[Freight],
		[Comment],
		[ModifiedDate])
	SELECT
		[SalesOrderID],
		[RevisionNumber],
		[OrderDate],
		[DueDate],
		[ShipDate],
		[Status],
		[OnlineOrderFlag],
		[PurchaseOrderNumber],
		[AccountNumber],
		[CustomerID],
		ISNULL([SalesPersonID],-1),
		[TerritoryID],
		[BillToAddressID],
		[ShipToAddressID],
		[ShipMethodID],
		[CreditCardID],
		[CreditCardApprovalCode],
		[CurrencyRateID],
		[SubTotal],
		[TaxAmt],
		[Freight],
		[Comment],
		[ModifiedDate]
	FROM Sales.SalesOrderHeader
	SET IDENTITY_INSERT Sales.SalesOrderHeader_inmem OFF


	SET IDENTITY_INSERT Sales.SalesOrderHeader_ondisk ON
	INSERT INTO Sales.SalesOrderHeader_ondisk
		([SalesOrderID],
		[RevisionNumber],
		[OrderDate],
		[DueDate],
		[ShipDate],
		[Status],
		[OnlineOrderFlag],
		[PurchaseOrderNumber],
		[AccountNumber],
		[CustomerID],
		[SalesPersonID],
		[TerritoryID],
		[BillToAddressID],
		[ShipToAddressID],
		[ShipMethodID],
		[CreditCardID],
		[CreditCardApprovalCode],
		[CurrencyRateID],
		[SubTotal],
		[TaxAmt],
		[Freight],
		[Comment],
		[ModifiedDate])
	SELECT *
	FROM Sales.SalesOrderHeader_inmem
	SET IDENTITY_INSERT Sales.SalesOrderHeader_ondisk OFF


	SET IDENTITY_INSERT Sales.SalesOrderDetail_inmem ON
	INSERT INTO Sales.SalesOrderDetail_inmem
		([SalesOrderID],
		[SalesOrderDetailID],
		[CarrierTrackingNumber],
		[OrderQty],
		[ProductID],
		[SpecialOfferID],
		[UnitPrice],
		[UnitPriceDiscount],
		[ModifiedDate])
	SELECT
		[SalesOrderID],
		[SalesOrderDetailID],
		[CarrierTrackingNumber],
		[OrderQty],
		[ProductID],
		[SpecialOfferID],
		[UnitPrice],
		[UnitPriceDiscount],
		[ModifiedDate]
	FROM Sales.SalesOrderDetail
	SET IDENTITY_INSERT Sales.SalesOrderDetail_inmem OFF


	SET IDENTITY_INSERT Sales.SalesOrderDetail_ondisk ON
	INSERT INTO Sales.SalesOrderDetail_ondisk
		([SalesOrderID],
		[SalesOrderDetailID],
		[CarrierTrackingNumber],
		[OrderQty],
		[ProductID],
		[SpecialOfferID],
		[UnitPrice],
		[UnitPriceDiscount],
		[ModifiedDate])
	SELECT *
	FROM Sales.SalesOrderDetail_inmem
	SET IDENTITY_INSERT Sales.SalesOrderDetail_ondisk OFF

	CHECKPOINT
END
'
GO
PRINT N'Creating [Security].[customerAccessPredicate]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[Security].[customerAccessPredicate]') AND (type = 'IF' OR type = 'FN' OR type = 'TF'))
EXEC sp_executesql N'
CREATE FUNCTION [Security].[customerAccessPredicate](@TerritoryID int)
	RETURNS TABLE
	WITH SCHEMABINDING
AS
	RETURN SELECT 1 AS accessResult
	FROM HumanResources.Employee e 
	INNER JOIN Sales.SalesPerson sp ON sp.BusinessEntityID = e.BusinessEntityID
	WHERE
		( RIGHT(e.LoginID, LEN(e.LoginID) - LEN(''adventure-works\'')) = USER_NAME() AND sp.TerritoryID = @TerritoryID ) 
		OR IS_MEMBER(''SalesManagers'') = 1
		OR IS_MEMBER(''db_owner'') = 1
'
GO
PRINT N'Creating [Sales].[usp_InsertSalesOrder_inmem]'
GO
IF OBJECT_ID(N'[Sales].[usp_InsertSalesOrder_inmem]', 'P') IS NULL
EXEC sp_executesql N'CREATE PROCEDURE [Sales].[usp_InsertSalesOrder_inmem]
	@SalesOrderID int OUTPUT,
	@DueDate [datetime2](7) NOT NULL,
	@CustomerID [int] NOT NULL,
	@BillToAddressID [int] NOT NULL,
	@ShipToAddressID [int] NOT NULL,
	@ShipMethodID [int] NOT NULL,
	@SalesOrderDetails Sales.SalesOrderDetailType_inmem READONLY,
	@Status [tinyint] NOT NULL = 1,
	@OnlineOrderFlag [bit] NOT NULL = 1,
	@PurchaseOrderNumber [nvarchar](25) = NULL,
	@AccountNumber [nvarchar](15) = NULL,
	@SalesPersonID [int] NOT NULL = -1,
	@TerritoryID [int] = NULL,
	@CreditCardID [int] = NULL,
	@CreditCardApprovalCode [varchar](15) = NULL,
	@CurrencyRateID [int] = NULL,
	@Comment nvarchar(128) = NULL
WITH NATIVE_COMPILATION, SCHEMABINDING
AS
BEGIN ATOMIC WITH
  (TRANSACTION ISOLATION LEVEL = SNAPSHOT,
   LANGUAGE = N''us_english'')

	DECLARE @OrderDate datetime2 NOT NULL = SYSDATETIME()

	DECLARE @SubTotal money NOT NULL = 0

	SELECT @SubTotal = ISNULL(SUM(p.ListPrice * (1 - ISNULL(so.DiscountPct, 0))),0)
	FROM @SalesOrderDetails od 
		JOIN Production.Product_inmem p on od.ProductID=p.ProductID
		LEFT JOIN Sales.SpecialOffer_inmem so on od.SpecialOfferID=so.SpecialOfferID

	INSERT INTO Sales.SalesOrderHeader_inmem
	(	DueDate,
		Status,
		OnlineOrderFlag,
		PurchaseOrderNumber,
		AccountNumber,
		CustomerID,
		SalesPersonID,
		TerritoryID,
		BillToAddressID,
		ShipToAddressID,
		ShipMethodID,
		CreditCardID,
		CreditCardApprovalCode,
		CurrencyRateID,
		Comment,
		OrderDate,
		SubTotal,
		ModifiedDate)
	VALUES
	(	
		@DueDate,
		@Status,
		@OnlineOrderFlag,
		@PurchaseOrderNumber,
		@AccountNumber,
		@CustomerID,
		@SalesPersonID,
		@TerritoryID,
		@BillToAddressID,
		@ShipToAddressID,
		@ShipMethodID,
		@CreditCardID,
		@CreditCardApprovalCode,
		@CurrencyRateID,
		@Comment,
		@OrderDate,
		@SubTotal,
		@OrderDate
	)

    SET @SalesOrderID = SCOPE_IDENTITY()

	INSERT INTO Sales.SalesOrderDetail_inmem
	(
		SalesOrderID,
		OrderQty,
		ProductID,
		SpecialOfferID,
		UnitPrice,
		UnitPriceDiscount,
		ModifiedDate
	)
    SELECT 
		@SalesOrderID,
		od.OrderQty,
		od.ProductID,
		od.SpecialOfferID,
		p.ListPrice,
		ISNULL(p.ListPrice * so.DiscountPct, 0),
		@OrderDate
	FROM @SalesOrderDetails od 
		JOIN Production.Product_inmem p on od.ProductID=p.ProductID
		LEFT JOIN Sales.SpecialOffer_inmem so on od.SpecialOfferID=so.SpecialOfferID

END'
GO
PRINT N'Creating [Sales].[CustomerPII]'
GO
IF OBJECT_ID(N'[Sales].[CustomerPII]', 'U') IS NULL
CREATE TABLE [Sales].[CustomerPII]
(
[CustomerID] [int] NOT NULL,
[FirstName] [dbo].[Name] NOT NULL,
[LastName] [dbo].[Name] NOT NULL,
[SSN] [nvarchar] (11) NULL,
[CreditCardNumber] [nvarchar] (25) NULL,
[EmailAddress] [nvarchar] (50) MASKED WITH (FUNCTION = 'email()') NULL,
[PhoneNumber] [nvarchar] (25) MASKED WITH (FUNCTION = 'default()') NULL,
[TerritoryID] [int] NULL
)
GO
PRINT N'Creating [Security].[customerPolicy]'
GO
CREATE SECURITY POLICY [Security].[customerPolicy]
ADD FILTER PREDICATE [Security].[customerAccessPredicate]([TerritoryID])
ON [Sales].[CustomerPII],
ADD BLOCK PREDICATE [Security].[customerAccessPredicate]([TerritoryID])
ON [Sales].[CustomerPII] 
WITH (STATE = ON)
GO
PRINT N'Creating [Person].[Person_Temporal]'
GO
IF OBJECT_ID(N'[Person].[Person_Temporal_History]', 'U') IS NULL
CREATE TABLE [Person].[Person_Temporal_History]
(
[BusinessEntityID] [int] NOT NULL,
[PersonType] [nchar] (2) NOT NULL,
[NameStyle] [dbo].[NameStyle] NOT NULL,
[Title] [nvarchar] (8) NULL,
[FirstName] [dbo].[Name] NOT NULL,
[MiddleName] [dbo].[Name] NULL,
[LastName] [dbo].[Name] NOT NULL,
[Suffix] [nvarchar] (10) NULL,
[EmailPromotion] [int] NOT NULL,
[ValidFrom] [datetime2] NOT NULL,
[ValidTo] [datetime2] NOT NULL
)
GO
PRINT N'Creating index [ix_Person_Temporal_History] on [Person].[Person_Temporal_History]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'ix_Person_Temporal_History' AND object_id = OBJECT_ID(N'[Person].[Person_Temporal_History]'))
CREATE CLUSTERED INDEX [ix_Person_Temporal_History] ON [Person].[Person_Temporal_History] ([BusinessEntityID], [ValidFrom], [ValidTo])
GO
IF OBJECT_ID(N'[Person].[Person_Temporal]', 'U') IS NULL
CREATE TABLE [Person].[Person_Temporal]
(
[BusinessEntityID] [int] NOT NULL,
[PersonType] [nchar] (2) NOT NULL,
[NameStyle] [dbo].[NameStyle] NOT NULL,
[Title] [nvarchar] (8) NULL,
[FirstName] [dbo].[Name] NOT NULL,
[MiddleName] [dbo].[Name] NULL,
[LastName] [dbo].[Name] NOT NULL,
[Suffix] [nvarchar] (10) NULL,
[EmailPromotion] [int] NOT NULL,
[ValidFrom] [datetime2] GENERATED ALWAYS AS ROW START HIDDEN NOT NULL,
[ValidTo] [datetime2] GENERATED ALWAYS AS ROW END HIDDEN NOT NULL,
PERIOD FOR SYSTEM_TIME (ValidFrom, ValidTo),
CONSTRAINT [PK_Person_Temporal_BusinessEntityID] PRIMARY KEY CLUSTERED  ([BusinessEntityID])
)
WITH
(
SYSTEM_VERSIONING = ON (HISTORY_TABLE = [Person].[Person_Temporal_History])
)
GO
PRINT N'Creating [HumanResources].[Employee_Temporal]'
GO
IF OBJECT_ID(N'[HumanResources].[Employee_Temporal_History]', 'U') IS NULL
CREATE TABLE [HumanResources].[Employee_Temporal_History]
(
[BusinessEntityID] [int] NOT NULL,
[NationalIDNumber] [nvarchar] (15) NOT NULL,
[LoginID] [nvarchar] (256) NOT NULL,
[OrganizationNode] [sys].[hierarchyid] NULL,
[OrganizationLevel] [smallint] NULL,
[JobTitle] [nvarchar] (50) NOT NULL,
[BirthDate] [date] NOT NULL,
[MaritalStatus] [nchar] (1) NOT NULL,
[Gender] [nchar] (1) NOT NULL,
[HireDate] [date] NOT NULL,
[VacationHours] [smallint] NOT NULL,
[SickLeaveHours] [smallint] NOT NULL,
[ValidFrom] [datetime2] NOT NULL,
[ValidTo] [datetime2] NOT NULL
)
GO
PRINT N'Creating index [ix_Employee_Temporal_History] on [HumanResources].[Employee_Temporal_History]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'ix_Employee_Temporal_History' AND object_id = OBJECT_ID(N'[HumanResources].[Employee_Temporal_History]'))
CREATE CLUSTERED INDEX [ix_Employee_Temporal_History] ON [HumanResources].[Employee_Temporal_History] ([BusinessEntityID], [ValidFrom], [ValidTo])
GO
IF OBJECT_ID(N'[HumanResources].[Employee_Temporal]', 'U') IS NULL
CREATE TABLE [HumanResources].[Employee_Temporal]
(
[BusinessEntityID] [int] NOT NULL,
[NationalIDNumber] [nvarchar] (15) NOT NULL,
[LoginID] [nvarchar] (256) NOT NULL,
[OrganizationNode] [sys].[hierarchyid] NULL,
[OrganizationLevel] AS ([OrganizationNode].[GetLevel]()),
[JobTitle] [nvarchar] (50) NOT NULL,
[BirthDate] [date] NOT NULL,
[MaritalStatus] [nchar] (1) NOT NULL,
[Gender] [nchar] (1) NOT NULL,
[HireDate] [date] NOT NULL,
[VacationHours] [smallint] NOT NULL,
[SickLeaveHours] [smallint] NOT NULL,
[ValidFrom] [datetime2] GENERATED ALWAYS AS ROW START HIDDEN NOT NULL,
[ValidTo] [datetime2] GENERATED ALWAYS AS ROW END HIDDEN NOT NULL,
PERIOD FOR SYSTEM_TIME (ValidFrom, ValidTo),
CONSTRAINT [PK_Employee_History_BusinessEntityID] PRIMARY KEY CLUSTERED  ([BusinessEntityID])
)
WITH
(
SYSTEM_VERSIONING = ON (HISTORY_TABLE = [HumanResources].[Employee_Temporal_History])
)
GO
PRINT N'Creating [HumanResources].[vEmployeePersonTemporalInfo]'
GO
IF OBJECT_ID(N'[HumanResources].[vEmployeePersonTemporalInfo]', 'V') IS NULL
EXEC sp_executesql N'
/*
	View that joins [Person].[Person_Temporal] [HumanResources].[Employee_Temporal]
	This view can be later used in temporal querying which is extremely flexible and convenient
	given that participating tables are temporal and can be changed independently
*/
CREATE VIEW [HumanResources].[vEmployeePersonTemporalInfo]
AS
SELECT P.BusinessEntityID, P.Title, P. FirstName, P.LastName, P.MiddleName
, E.JobTitle, E.MaritalStatus, E.Gender, E.VacationHours, E.SickLeaveHours
FROM [Person].Person_Temporal P
JOIN  [HumanResources].[Employee_Temporal] E
ON P.[BusinessEntityID] = E.[BusinessEntityID]

'
GO
PRINT N'Creating [Person].[sp_UpdatePerson_Temporal]'
GO
IF OBJECT_ID(N'[Person].[sp_UpdatePerson_Temporal]', 'P') IS NULL
EXEC sp_executesql N'
/*
	Stored procedure for updating columns of Person_Temporal
	If all parameters except @BusinessEntityID are NULL no update is performed 
	For NON NULL columns NULL values are ignored (i.e. existing values is applied)
*/
CREATE PROCEDURE [Person].[sp_UpdatePerson_Temporal]
@BusinessEntityID INT,
@PersonType nchar(2) = NULL,
@Title nvarchar(8) = NULL,
@FirstName nvarchar(50) = NULL,
@MiddleName nvarchar(50) = NULL,
@LastName nvarchar(50) = NULL,
@Suffix nvarchar(10) = NULL,
@EmailPromotion smallint = NULL

AS

IF @PersonType IS NOT NULL OR @Title IS NOT NULL OR @FirstName IS NOT NULL OR @MiddleName IS NOT NULL
OR @LastName IS NOT NULL OR @Suffix IS NOT NULL OR @EmailPromotion IS NOT NULL 

	UPDATE Person.Person_Temporal
	SET PersonType = ISNULL (@PersonType, PersonType),
	Title = @Title,
	FirstName = ISNULL (@FirstName, FirstName),
	MiddleName = ISNULL (@MiddleName, MiddleName),
	LastName = ISNULL (@LastName, LastName),
	Suffix = @Suffix,
	EmailPromotion = ISNULL(@EmailPromotion, EmailPromotion)
	WHERE BusinessEntityID = @BusinessEntityID;
	
'
GO
PRINT N'Creating [Person].[sp_DeletePerson_Temporal]'
GO
IF OBJECT_ID(N'[Person].[sp_DeletePerson_Temporal]', 'P') IS NULL
EXEC sp_executesql N'
/*
	Stored procedure that deletes row in [Person].[Person_Temporal]
	and corresponding row in [HumanResources].[Employee_Temporal]
*/
CREATE PROCEDURE [Person].[sp_DeletePerson_Temporal]
@BusinessEntityID INT
AS

DELETE FROM [HumanResources].[Employee_Temporal] WHERE [BusinessEntityID] = @BusinessEntityID;
DELETE FROM [Person].[Person_Temporal] WHERE [BusinessEntityID] = @BusinessEntityID;

'
GO
PRINT N'Creating [HumanResources].[sp_UpdateEmployee_Temporal]'
GO
IF OBJECT_ID(N'[HumanResources].[sp_UpdateEmployee_Temporal]', 'P') IS NULL
EXEC sp_executesql N'
/*
	Stored procedure for updating columns of [HumanResources].[Employee_Temporal]
	If all parameters except @BusinessEntityID are NULL no update is performed 
	For NON NULL columns NULL values are ignored (i.e. existing values is applied)
*/
CREATE PROCEDURE [HumanResources].[sp_UpdateEmployee_Temporal]
 @BusinessEntityID INT
,@LoginID nvarchar(256) = NULL   
,@JobTitle nvarchar(50) = NULL
,@MaritalStatus nchar(1) = NULL
,@Gender nchar(1) = NULL
,@VacationHours smallint = 0
,@SickLeaveHours smallint = 0

AS
IF @LoginID IS NOT NULL OR @JobTitle IS NOT NULL OR @MaritalStatus IS NOT NULL 
OR @Gender IS NOT NULL OR @VacationHours IS NOT NULL OR @SickLeaveHours IS NOT NULL 

	UPDATE [HumanResources].[Employee_Temporal]
	SET  [LoginID] = ISNULL (@LoginID, LoginID),
	JobTitle = ISNULL (@JobTitle, JobTitle),
	MaritalStatus = ISNULL (@MaritalStatus, MaritalStatus),
	Gender = ISNULL (@Gender, Gender),
	VacationHours = ISNULL (@VacationHours, VacationHours),
	SickLeaveHours = ISNULL (@SickLeaveHours, SickLeaveHours)	
	WHERE BusinessEntityID = @BusinessEntityID;	
	
'
GO
PRINT N'Creating [HumanResources].[sp_GetEmployee_Person_Info_AsOf]'
GO
IF OBJECT_ID(N'[HumanResources].[sp_GetEmployee_Person_Info_AsOf]', 'P') IS NULL
EXEC sp_executesql N'
/*
	Stored procedure used for querying Employee and Person data AS OF
	If @AsOf parameter is NULL, current data is queried
	otherwise both current and historical data is queried
*/
CREATE  PROCEDURE [HumanResources].[sp_GetEmployee_Person_Info_AsOf]
@asOf datetime2 = NULL
AS
IF @asOf IS NULL
	SELECT * FROM [HumanResources].[vEmployeePersonTemporalInfo]
ELSE
	SELECT * FROM [HumanResources].[vEmployeePersonTemporalInfo] FOR SYSTEM_TIME AS OF @asOf;
'
GO
PRINT N'Creating [Sales].[OrderTracking]'
GO
IF OBJECT_ID(N'[Sales].[OrderTracking]', 'U') IS NULL
CREATE TABLE [Sales].[OrderTracking]
(
[OrderTrackingID] [int] NOT NULL IDENTITY(1, 1),
[SalesOrderID] [int] NOT NULL,
[CarrierTrackingNumber] [nvarchar] (25) NULL,
[TrackingEventID] [int] NOT NULL,
[EventDetails] [nvarchar] (2000) NOT NULL,
[EventDateTime] [datetime2] NOT NULL
)
GO
PRINT N'Creating primary key [PK_OrderTracking] on [Sales].[OrderTracking]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[Sales].[PK_OrderTracking]', 'PK') AND parent_object_id = OBJECT_ID(N'[Sales].[OrderTracking]', 'U'))
ALTER TABLE [Sales].[OrderTracking] ADD CONSTRAINT [PK_OrderTracking] PRIMARY KEY CLUSTERED  ([OrderTrackingID])
GO
PRINT N'Creating index [IX_OrderTracking_CarrierTrackingNumber] on [Sales].[OrderTracking]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_OrderTracking_CarrierTrackingNumber' AND object_id = OBJECT_ID(N'[Sales].[OrderTracking]'))
CREATE NONCLUSTERED INDEX [IX_OrderTracking_CarrierTrackingNumber] ON [Sales].[OrderTracking] ([CarrierTrackingNumber])
GO
PRINT N'Creating index [IX_OrderTracking_SalesOrderID] on [Sales].[OrderTracking]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_OrderTracking_SalesOrderID' AND object_id = OBJECT_ID(N'[Sales].[OrderTracking]'))
CREATE NONCLUSTERED INDEX [IX_OrderTracking_SalesOrderID] ON [Sales].[OrderTracking] ([SalesOrderID])
GO
PRINT N'Creating [dbo].[uspAddOrderTrackingEvent]'
GO
IF OBJECT_ID(N'[dbo].[uspAddOrderTrackingEvent]', 'P') IS NULL
EXEC sp_executesql N'
----------------------------------------------------------------
-- Create supporting stored procedures
----------------------------------------------------------------
CREATE PROCEDURE [dbo].[uspAddOrderTrackingEvent]
   @SalesOrderID INT,
   @TrackingEventID INT,
   @EventDetails NVARCHAR(2000)
AS
BEGIN
/* Example:
      exec dbo.uspGetOrderTrackingBySalesOrderID 53498
      exec dbo.uspAddOrderTrackingEvent 53498, 7, ''invalid address, package is undeleverable''
      exec dbo.uspGetOrderTrackingBySalesOrderID 53498
*/
   SET NOCOUNT ON;

   BEGIN TRY
      BEGIN TRANSACTION;

      DECLARE @TrackingNumber NVARCHAR(25);

      SET @TrackingNumber = (
         SELECT TOP 1 ot.CarrierTrackingNumber 
           FROM Sales.OrderTracking ot
          WHERE ot.SalesOrderID = @SalesOrderID);

      IF (@TrackingNumber IS NULL)
      BEGIN
         SET @TrackingNumber = SUBSTRING(CONVERT(CHAR(255), NEWID()),2,25);
      END;

      INSERT INTO Sales.OrderTracking
         (SalesOrderID, CarrierTrackingNumber, TrackingEventID, EventDetails, EventDateTime)
      VALUES
         (@SalesOrderID, @TrackingNumber, @TrackingEventID, @EventDetails, GETDATE());

      COMMIT TRANSACTION;
   END TRY
   BEGIN CATCH
      -- Rollback any active or uncommittable transactions before
      -- inserting information in the ErrorLog
      IF @@TRANCOUNT > 0
      BEGIN
         ROLLBACK TRANSACTION;
      END

      EXECUTE [dbo].[uspLogError];
   END CATCH;
END;
'
GO
PRINT N'Creating [Sales].[TrackingEvent]'
GO
IF OBJECT_ID(N'[Sales].[TrackingEvent]', 'U') IS NULL
CREATE TABLE [Sales].[TrackingEvent]
(
[TrackingEventID] [int] NOT NULL IDENTITY(1, 1),
[EventName] [nvarchar] (255) NOT NULL
)
GO
PRINT N'Creating primary key [PK_TrackingEvent_TrackingEventID] on [Sales].[TrackingEvent]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[Sales].[PK_TrackingEvent_TrackingEventID]', 'PK') AND parent_object_id = OBJECT_ID(N'[Sales].[TrackingEvent]', 'U'))
ALTER TABLE [Sales].[TrackingEvent] ADD CONSTRAINT [PK_TrackingEvent_TrackingEventID] PRIMARY KEY CLUSTERED  ([TrackingEventID])
GO
PRINT N'Creating [dbo].[uspGetOrderTrackingByTrackingNumber]'
GO
IF OBJECT_ID(N'[dbo].[uspGetOrderTrackingByTrackingNumber]', 'P') IS NULL
EXEC sp_executesql N'
CREATE PROCEDURE [dbo].[uspGetOrderTrackingByTrackingNumber]
   @CarrierTrackingNumber [nvarchar](25) NULL
AS
BEGIN
/* Example:
      EXEC dbo.uspGetOrderTrackingByTrackingNumber ''EE33-45E8-9F''
      EXEC dbo.uspAddOrderTrackingEvent 53498, 7, ''invalid address, package is undeleverable''
      EXEC dbo.uspGetOrderTrackingByTrackingNumber ''EE33-45E8-9F''
*/
   SET NOCOUNT ON;

   IF (@CarrierTrackingNumber IS NULL)
   BEGIN
      RETURN;
   END;

   SELECT
      ot.SalesOrderID,
      ot.CarrierTrackingNumber,
      ot.OrderTrackingID,
      ot.TrackingEventID,
      te.EventName,
      ot.EventDetails,
      ot.EventDateTime
   FROM 
      Sales.OrderTracking ot, 
      Sales.TrackingEvent te
   WHERE
      ot.CarrierTrackingNumber = @CarrierTrackingNumber AND
      ot.TrackingEventID = te.TrackingEventID
   ORDER BY
      ot.SalesOrderID,
      ot.TrackingEventID;
END;

'
GO
PRINT N'Creating [dbo].[uspGetOrderTrackingBySalesOrderID]'
GO
IF OBJECT_ID(N'[dbo].[uspGetOrderTrackingBySalesOrderID]', 'P') IS NULL
EXEC sp_executesql N'
CREATE PROCEDURE [dbo].[uspGetOrderTrackingBySalesOrderID]
   @SalesOrderID [int] NULL
AS
BEGIN
/* Example:
      exec dbo.uspGetOrderTrackingBySalesOrderID 53498
*/
   SET NOCOUNT ON;

   SELECT 
      ot.SalesOrderID,
      ot.CarrierTrackingNumber,
      ot.OrderTrackingID,
      ot.TrackingEventID,
      te.EventName,
      ot.EventDetails,
      ot.EventDateTime
   FROM 
      Sales.OrderTracking ot, 
      Sales.TrackingEvent te
   WHERE
      ot.SalesOrderID = @SalesOrderID AND
      ot.TrackingEventID = te.TrackingEventID
   ORDER BY
      ot.SalesOrderID,
      ot.TrackingEventID;
END;
'
GO
PRINT N'Creating [dbo].[ufnGetDocumentStatusText]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ufnGetDocumentStatusText]') AND (type = 'IF' OR type = 'FN' OR type = 'TF'))
EXEC sp_executesql N'
CREATE FUNCTION [dbo].[ufnGetDocumentStatusText](@Status [tinyint])
RETURNS [nvarchar](16) 
AS 
-- Returns the sales order status text representation for the status value.
BEGIN
    DECLARE @ret [nvarchar](16);

    SET @ret = 
        CASE @Status
            WHEN 1 THEN N''Pending approval''
            WHEN 2 THEN N''Approved''
            WHEN 3 THEN N''Obsolete''
            ELSE N''** Invalid **''
        END;
    
    RETURN @ret
END;
'
GO
PRINT N'Creating [dbo].[ufnGetPurchaseOrderStatusText]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ufnGetPurchaseOrderStatusText]') AND (type = 'IF' OR type = 'FN' OR type = 'TF'))
EXEC sp_executesql N'
CREATE FUNCTION [dbo].[ufnGetPurchaseOrderStatusText](@Status [tinyint])
RETURNS [nvarchar](15) 
AS 
-- Returns the sales order status text representation for the status value.
BEGIN
    DECLARE @ret [nvarchar](15);

    SET @ret = 
        CASE @Status
            WHEN 1 THEN ''Pending''
            WHEN 2 THEN ''Approved''
            WHEN 3 THEN ''Rejected''
            WHEN 4 THEN ''Complete''
            ELSE ''** Invalid **''
        END;
    
    RETURN @ret
END;
'
GO
PRINT N'Creating [dbo].[ufnGetSalesOrderStatusText]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ufnGetSalesOrderStatusText]') AND (type = 'IF' OR type = 'FN' OR type = 'TF'))
EXEC sp_executesql N'
CREATE FUNCTION [dbo].[ufnGetSalesOrderStatusText](@Status [tinyint])
RETURNS [nvarchar](15) 
AS 
-- Returns the sales order status text representation for the status value.
BEGIN
    DECLARE @ret [nvarchar](15);

    SET @ret = 
        CASE @Status
            WHEN 1 THEN ''In process''
            WHEN 2 THEN ''Approved''
            WHEN 3 THEN ''Backordered''
            WHEN 4 THEN ''Rejected''
            WHEN 5 THEN ''Shipped''
            WHEN 6 THEN ''Cancelled''
            ELSE ''** Invalid **''
        END;
    
    RETURN @ret
END;
'
GO
PRINT N'Creating [dbo].[ufnToRawJsonArray]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ufnToRawJsonArray]') AND (type = 'IF' OR type = 'FN' OR type = 'TF'))
EXEC sp_executesql N'CREATE FUNCTION
[dbo].[ufnToRawJsonArray](@json nvarchar(max), @key nvarchar(400)) returns nvarchar(max)
as begin
       declare @new nvarchar(max) = replace(@json, CONCAT(''},{"'', @key,''":''),'','')
       return ''['' + substring(@new, 1 + (LEN(@key)+5), LEN(@new) -2 - (LEN(@key)+5)) + '']''
end
'
GO
PRINT N'Creating [dbo].[AWBuildVersion]'
GO
IF OBJECT_ID(N'[dbo].[AWBuildVersion]', 'U') IS NULL
CREATE TABLE [dbo].[AWBuildVersion]
(
[SystemInformationID] [tinyint] NOT NULL IDENTITY(1, 1),
[Database Version] [nvarchar] (25) NOT NULL,
[VersionDate] [datetime] NOT NULL,
[ModifiedDate] [datetime] NOT NULL CONSTRAINT [DF_AWBuildVersion_ModifiedDate] DEFAULT (getdate())
)
GO
PRINT N'Creating primary key [PK_AWBuildVersion_SystemInformationID] on [dbo].[AWBuildVersion]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[PK_AWBuildVersion_SystemInformationID]', 'PK') AND parent_object_id = OBJECT_ID(N'[dbo].[AWBuildVersion]', 'U'))
ALTER TABLE [dbo].[AWBuildVersion] ADD CONSTRAINT [PK_AWBuildVersion_SystemInformationID] PRIMARY KEY CLUSTERED  ([SystemInformationID])
GO
PRINT N'Creating [dbo].[DatabaseLog]'
GO
IF OBJECT_ID(N'[dbo].[DatabaseLog]', 'U') IS NULL
CREATE TABLE [dbo].[DatabaseLog]
(
[DatabaseLogID] [int] NOT NULL IDENTITY(1, 1),
[PostTime] [datetime] NOT NULL,
[DatabaseUser] [sys].[sysname] NOT NULL,
[Event] [sys].[sysname] NOT NULL,
[Schema] [sys].[sysname] NULL,
[Object] [sys].[sysname] NULL,
[TSQL] [nvarchar] (max) NOT NULL,
[XmlEvent] [xml] NOT NULL
)
GO
PRINT N'Creating primary key [PK_DatabaseLog_DatabaseLogID] on [dbo].[DatabaseLog]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[PK_DatabaseLog_DatabaseLogID]', 'PK') AND parent_object_id = OBJECT_ID(N'[dbo].[DatabaseLog]', 'U'))
ALTER TABLE [dbo].[DatabaseLog] ADD CONSTRAINT [PK_DatabaseLog_DatabaseLogID] PRIMARY KEY NONCLUSTERED  ([DatabaseLogID])
GO
PRINT N'Creating [dbo].[ufnGetAccountingEndDate_native]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ufnGetAccountingEndDate_native]') AND (type = 'IF' OR type = 'FN' OR type = 'TF'))
EXEC sp_executesql N'
CREATE FUNCTION [dbo].[ufnGetAccountingEndDate_native]()
RETURNS [datetime] 
WITH NATIVE_COMPILATION, SCHEMABINDING
AS 
BEGIN ATOMIC WITH (TRANSACTION ISOLATION LEVEL=SNAPSHOT, LANGUAGE=N''us_english'')

    RETURN DATEADD(millisecond, -2, CONVERT(datetime, ''20160701'', 112));

END
'
GO
PRINT N'Creating [dbo].[ufnGetAccountingStartDate_native]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ufnGetAccountingStartDate_native]') AND (type = 'IF' OR type = 'FN' OR type = 'TF'))
EXEC sp_executesql N'
CREATE FUNCTION [dbo].[ufnGetAccountingStartDate_native]()
RETURNS [datetime] 
WITH NATIVE_COMPILATION, SCHEMABINDING
AS 
BEGIN ATOMIC WITH (TRANSACTION ISOLATION LEVEL=SNAPSHOT, LANGUAGE=N''us_english'')

    RETURN CONVERT(datetime, ''20150701'', 112);

END
'
GO
PRINT N'Creating [dbo].[ufnGetDocumentStatusText_native]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ufnGetDocumentStatusText_native]') AND (type = 'IF' OR type = 'FN' OR type = 'TF'))
EXEC sp_executesql N'
CREATE FUNCTION [dbo].[ufnGetDocumentStatusText_native] (@Status tinyint)
RETURNS nvarchar(15) 
WITH NATIVE_COMPILATION, SCHEMABINDING
AS
BEGIN ATOMIC WITH (TRANSACTION ISOLATION LEVEL = SNAPSHOT, LANGUAGE = N''English'')

    IF @Status=1 RETURN ''Pending approval''
    IF @Status=2 RETURN ''Approved''
    IF @Status=3 RETURN ''Obsolete''
    
    RETURN ''** Invalid **''

END
'
GO
PRINT N'Creating [dbo].[ufnGetPurchaseOrderStatusText_native]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ufnGetPurchaseOrderStatusText_native]') AND (type = 'IF' OR type = 'FN' OR type = 'TF'))
EXEC sp_executesql N'
CREATE FUNCTION [dbo].[ufnGetPurchaseOrderStatusText_native] (@Status tinyint)
RETURNS nvarchar(15) 
WITH NATIVE_COMPILATION, SCHEMABINDING
AS
BEGIN ATOMIC WITH (TRANSACTION ISOLATION LEVEL = SNAPSHOT, LANGUAGE = N''English'')

    IF @Status=1 RETURN ''Pending''
    IF @Status=2 RETURN ''Approved''
    IF @Status=3 RETURN ''Rejected''
    IF @Status=4 RETURN ''Complete''
    
    RETURN ''** Invalid **''

END
'
GO
PRINT N'Creating [dbo].[ufnGetSalesOrderStatusText_native]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ufnGetSalesOrderStatusText_native]') AND (type = 'IF' OR type = 'FN' OR type = 'TF'))
EXEC sp_executesql N'
CREATE FUNCTION [dbo].[ufnGetSalesOrderStatusText_native] (@Status tinyint)
RETURNS nvarchar(15) 
WITH NATIVE_COMPILATION, SCHEMABINDING
AS
BEGIN ATOMIC WITH (TRANSACTION ISOLATION LEVEL = SNAPSHOT, LANGUAGE = N''English'')

    IF @Status=1 RETURN ''In Process''
    IF @Status=2 RETURN ''Approved''
    IF @Status=3 RETURN ''Backordered''
    IF @Status=4 RETURN ''Rejected''
    IF @Status=5 RETURN ''Shipped''
    IF @Status=6 RETURN ''Cancelled''

    
    RETURN ''** Invalid **''

END
'
GO
PRINT N'Creating [dbo].[ufnLeadingZeros_native]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ufnLeadingZeros_native]') AND (type = 'IF' OR type = 'FN' OR type = 'TF'))
EXEC sp_executesql N'
CREATE FUNCTION [dbo].[ufnLeadingZeros_native](
    @Value int
) 
RETURNS varchar(8) 
WITH NATIVE_COMPILATION, SCHEMABINDING
AS 
BEGIN ATOMIC WITH (TRANSACTION ISOLATION LEVEL = SNAPSHOT, LANGUAGE = N''English'')

    DECLARE @ReturnValue varchar(8);

    SET @ReturnValue = CONVERT(varchar(8), @Value);

	DECLARE @i int = 0, @count int = 8 - LEN(@ReturnValue)

	WHILE @i < @count
	BEGIN
		SET @ReturnValue = ''0'' + @ReturnValue;
		SET @i += 1
	END

    RETURN (@ReturnValue);

END

'
GO
PRINT N'Adding full text indexing to columns'
GO
PRINT N'Adding constraints to [HumanResources].[EmployeeDepartmentHistory]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE object_id = OBJECT_ID(N'[HumanResources].[CK_EmployeeDepartmentHistory_EndDate]', 'C') AND parent_object_id = OBJECT_ID(N'[HumanResources].[EmployeeDepartmentHistory]', 'U'))
ALTER TABLE [HumanResources].[EmployeeDepartmentHistory] ADD CONSTRAINT [CK_EmployeeDepartmentHistory_EndDate] CHECK (([EndDate]>=[StartDate] OR [EndDate] IS NULL))
GO
PRINT N'Adding constraints to [HumanResources].[EmployeePayHistory]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE object_id = OBJECT_ID(N'[HumanResources].[CK_EmployeePayHistory_Rate]', 'C') AND parent_object_id = OBJECT_ID(N'[HumanResources].[EmployeePayHistory]', 'U'))
ALTER TABLE [HumanResources].[EmployeePayHistory] ADD CONSTRAINT [CK_EmployeePayHistory_Rate] CHECK (([Rate]>=(6.50) AND [Rate]<=(200.00)))
GO
IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE object_id = OBJECT_ID(N'[HumanResources].[CK_EmployeePayHistory_PayFrequency]', 'C') AND parent_object_id = OBJECT_ID(N'[HumanResources].[EmployeePayHistory]', 'U'))
ALTER TABLE [HumanResources].[EmployeePayHistory] ADD CONSTRAINT [CK_EmployeePayHistory_PayFrequency] CHECK (([PayFrequency]=(2) OR [PayFrequency]=(1)))
GO
PRINT N'Adding constraints to [HumanResources].[Employee]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE object_id = OBJECT_ID(N'[HumanResources].[CK_Employee_BirthDate]', 'C') AND parent_object_id = OBJECT_ID(N'[HumanResources].[Employee]', 'U'))
ALTER TABLE [HumanResources].[Employee] ADD CONSTRAINT [CK_Employee_BirthDate] CHECK (([BirthDate]>='1930-01-01' AND [BirthDate]<=dateadd(year,(-18),getdate())))
GO
IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE object_id = OBJECT_ID(N'[HumanResources].[CK_Employee_MaritalStatus]', 'C') AND parent_object_id = OBJECT_ID(N'[HumanResources].[Employee]', 'U'))
ALTER TABLE [HumanResources].[Employee] ADD CONSTRAINT [CK_Employee_MaritalStatus] CHECK ((upper([MaritalStatus])='S' OR upper([MaritalStatus])='M'))
GO
IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE object_id = OBJECT_ID(N'[HumanResources].[CK_Employee_Gender]', 'C') AND parent_object_id = OBJECT_ID(N'[HumanResources].[Employee]', 'U'))
ALTER TABLE [HumanResources].[Employee] ADD CONSTRAINT [CK_Employee_Gender] CHECK ((upper([Gender])='F' OR upper([Gender])='M'))
GO
IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE object_id = OBJECT_ID(N'[HumanResources].[CK_Employee_HireDate]', 'C') AND parent_object_id = OBJECT_ID(N'[HumanResources].[Employee]', 'U'))
ALTER TABLE [HumanResources].[Employee] ADD CONSTRAINT [CK_Employee_HireDate] CHECK (([HireDate]>='1996-07-01' AND [HireDate]<=dateadd(day,(1),getdate())))
GO
IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE object_id = OBJECT_ID(N'[HumanResources].[CK_Employee_VacationHours]', 'C') AND parent_object_id = OBJECT_ID(N'[HumanResources].[Employee]', 'U'))
ALTER TABLE [HumanResources].[Employee] ADD CONSTRAINT [CK_Employee_VacationHours] CHECK (([VacationHours]>=((-40)) AND [VacationHours]<=(240)))
GO
IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE object_id = OBJECT_ID(N'[HumanResources].[CK_Employee_SickLeaveHours]', 'C') AND parent_object_id = OBJECT_ID(N'[HumanResources].[Employee]', 'U'))
ALTER TABLE [HumanResources].[Employee] ADD CONSTRAINT [CK_Employee_SickLeaveHours] CHECK (([SickLeaveHours]>=(0) AND [SickLeaveHours]<=(120)))
GO
PRINT N'Adding constraints to [Person].[Person]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE object_id = OBJECT_ID(N'[Person].[CK_Person_PersonType]', 'C') AND parent_object_id = OBJECT_ID(N'[Person].[Person]', 'U'))
ALTER TABLE [Person].[Person] ADD CONSTRAINT [CK_Person_PersonType] CHECK (([PersonType] IS NULL OR upper([PersonType])='GC' OR upper([PersonType])='SP' OR upper([PersonType])='EM' OR upper([PersonType])='IN' OR upper([PersonType])='VC' OR upper([PersonType])='SC'))
GO
IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE object_id = OBJECT_ID(N'[Person].[CK_Person_EmailPromotion]', 'C') AND parent_object_id = OBJECT_ID(N'[Person].[Person]', 'U'))
ALTER TABLE [Person].[Person] ADD CONSTRAINT [CK_Person_EmailPromotion] CHECK (([EmailPromotion]>=(0) AND [EmailPromotion]<=(2)))
GO
PRINT N'Adding constraints to [Person].[Person_json]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE object_id = OBJECT_ID(N'[Person].[AdditionalContactInfo must be formatted as JSON]', 'C') AND parent_object_id = OBJECT_ID(N'[Person].[Person_json]', 'U'))
ALTER TABLE [Person].[Person_json] ADD CONSTRAINT [AdditionalContactInfo must be formatted as JSON] CHECK ((isjson([AdditionalContactInfo])>(0)))
GO
IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE object_id = OBJECT_ID(N'[Person].[Demographics must be formatted as JSON]', 'C') AND parent_object_id = OBJECT_ID(N'[Person].[Person_json]', 'U'))
ALTER TABLE [Person].[Person_json] ADD CONSTRAINT [Demographics must be formatted as JSON] CHECK ((isjson([Demographics])>(0)))
GO
IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE object_id = OBJECT_ID(N'[Person].[Phone numbers must be formatted as JSON array]', 'C') AND parent_object_id = OBJECT_ID(N'[Person].[Person_json]', 'U'))
ALTER TABLE [Person].[Person_json] ADD CONSTRAINT [Phone numbers must be formatted as JSON array] CHECK ((isjson([PhoneNumbers])>(0)))
GO
IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE object_id = OBJECT_ID(N'[Person].[Email addresses must be formatted as JSON array]', 'C') AND parent_object_id = OBJECT_ID(N'[Person].[Person_json]', 'U'))
ALTER TABLE [Person].[Person_json] ADD CONSTRAINT [Email addresses must be formatted as JSON array] CHECK ((isjson([EmailAddresses])>(0)))
GO
PRINT N'Adding constraints to [Production].[BillOfMaterials]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE object_id = OBJECT_ID(N'[Production].[CK_BillOfMaterials_PerAssemblyQty]', 'C') AND parent_object_id = OBJECT_ID(N'[Production].[BillOfMaterials]', 'U'))
ALTER TABLE [Production].[BillOfMaterials] ADD CONSTRAINT [CK_BillOfMaterials_PerAssemblyQty] CHECK (([PerAssemblyQty]>=(1.00)))
GO
IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE object_id = OBJECT_ID(N'[Production].[CK_BillOfMaterials_BOMLevel]', 'C') AND parent_object_id = OBJECT_ID(N'[Production].[BillOfMaterials]', 'U'))
ALTER TABLE [Production].[BillOfMaterials] ADD CONSTRAINT [CK_BillOfMaterials_BOMLevel] CHECK (([ProductAssemblyID] IS NULL AND [BOMLevel]=(0) AND [PerAssemblyQty]=(1.00) OR [ProductAssemblyID] IS NOT NULL AND [BOMLevel]>=(1)))
GO
IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE object_id = OBJECT_ID(N'[Production].[CK_BillOfMaterials_EndDate]', 'C') AND parent_object_id = OBJECT_ID(N'[Production].[BillOfMaterials]', 'U'))
ALTER TABLE [Production].[BillOfMaterials] ADD CONSTRAINT [CK_BillOfMaterials_EndDate] CHECK (([EndDate]>[StartDate] OR [EndDate] IS NULL))
GO
IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE object_id = OBJECT_ID(N'[Production].[CK_BillOfMaterials_ProductAssemblyID]', 'C') AND parent_object_id = OBJECT_ID(N'[Production].[BillOfMaterials]', 'U'))
ALTER TABLE [Production].[BillOfMaterials] ADD CONSTRAINT [CK_BillOfMaterials_ProductAssemblyID] CHECK (([ProductAssemblyID]<>[ComponentID]))
GO
PRINT N'Adding constraints to [Production].[Document]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE object_id = OBJECT_ID(N'[Production].[CK_Document_Status]', 'C') AND parent_object_id = OBJECT_ID(N'[Production].[Document]', 'U'))
ALTER TABLE [Production].[Document] ADD CONSTRAINT [CK_Document_Status] CHECK (([Status]>=(1) AND [Status]<=(3)))
GO
PRINT N'Adding constraints to [Production].[Location]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE object_id = OBJECT_ID(N'[Production].[CK_Location_CostRate]', 'C') AND parent_object_id = OBJECT_ID(N'[Production].[Location]', 'U'))
ALTER TABLE [Production].[Location] ADD CONSTRAINT [CK_Location_CostRate] CHECK (([CostRate]>=(0.00)))
GO
IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE object_id = OBJECT_ID(N'[Production].[CK_Location_Availability]', 'C') AND parent_object_id = OBJECT_ID(N'[Production].[Location]', 'U'))
ALTER TABLE [Production].[Location] ADD CONSTRAINT [CK_Location_Availability] CHECK (([Availability]>=(0.00)))
GO
PRINT N'Adding constraints to [Production].[ProductCostHistory]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE object_id = OBJECT_ID(N'[Production].[CK_ProductCostHistory_StandardCost]', 'C') AND parent_object_id = OBJECT_ID(N'[Production].[ProductCostHistory]', 'U'))
ALTER TABLE [Production].[ProductCostHistory] ADD CONSTRAINT [CK_ProductCostHistory_StandardCost] CHECK (([StandardCost]>=(0.00)))
GO
IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE object_id = OBJECT_ID(N'[Production].[CK_ProductCostHistory_EndDate]', 'C') AND parent_object_id = OBJECT_ID(N'[Production].[ProductCostHistory]', 'U'))
ALTER TABLE [Production].[ProductCostHistory] ADD CONSTRAINT [CK_ProductCostHistory_EndDate] CHECK (([EndDate]>=[StartDate] OR [EndDate] IS NULL))
GO
PRINT N'Adding constraints to [Production].[ProductInventory]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE object_id = OBJECT_ID(N'[Production].[CK_ProductInventory_Shelf]', 'C') AND parent_object_id = OBJECT_ID(N'[Production].[ProductInventory]', 'U'))
ALTER TABLE [Production].[ProductInventory] ADD CONSTRAINT [CK_ProductInventory_Shelf] CHECK (([Shelf] like '[A-Za-z]' OR [Shelf]='N/A'))
GO
IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE object_id = OBJECT_ID(N'[Production].[CK_ProductInventory_Bin]', 'C') AND parent_object_id = OBJECT_ID(N'[Production].[ProductInventory]', 'U'))
ALTER TABLE [Production].[ProductInventory] ADD CONSTRAINT [CK_ProductInventory_Bin] CHECK (([Bin]>=(0) AND [Bin]<=(100)))
GO
PRINT N'Adding constraints to [Production].[ProductListPriceHistory]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE object_id = OBJECT_ID(N'[Production].[CK_ProductListPriceHistory_ListPrice]', 'C') AND parent_object_id = OBJECT_ID(N'[Production].[ProductListPriceHistory]', 'U'))
ALTER TABLE [Production].[ProductListPriceHistory] ADD CONSTRAINT [CK_ProductListPriceHistory_ListPrice] CHECK (([ListPrice]>(0.00)))
GO
IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE object_id = OBJECT_ID(N'[Production].[CK_ProductListPriceHistory_EndDate]', 'C') AND parent_object_id = OBJECT_ID(N'[Production].[ProductListPriceHistory]', 'U'))
ALTER TABLE [Production].[ProductListPriceHistory] ADD CONSTRAINT [CK_ProductListPriceHistory_EndDate] CHECK (([EndDate]>=[StartDate] OR [EndDate] IS NULL))
GO
PRINT N'Adding constraints to [Production].[ProductReview]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE object_id = OBJECT_ID(N'[Production].[CK_ProductReview_Rating]', 'C') AND parent_object_id = OBJECT_ID(N'[Production].[ProductReview]', 'U'))
ALTER TABLE [Production].[ProductReview] ADD CONSTRAINT [CK_ProductReview_Rating] CHECK (([Rating]>=(1) AND [Rating]<=(5)))
GO
PRINT N'Adding constraints to [Production].[Product]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE object_id = OBJECT_ID(N'[Production].[CK_Product_SafetyStockLevel]', 'C') AND parent_object_id = OBJECT_ID(N'[Production].[Product]', 'U'))
ALTER TABLE [Production].[Product] ADD CONSTRAINT [CK_Product_SafetyStockLevel] CHECK (([SafetyStockLevel]>(0)))
GO
IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE object_id = OBJECT_ID(N'[Production].[CK_Product_ReorderPoint]', 'C') AND parent_object_id = OBJECT_ID(N'[Production].[Product]', 'U'))
ALTER TABLE [Production].[Product] ADD CONSTRAINT [CK_Product_ReorderPoint] CHECK (([ReorderPoint]>(0)))
GO
IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE object_id = OBJECT_ID(N'[Production].[CK_Product_StandardCost]', 'C') AND parent_object_id = OBJECT_ID(N'[Production].[Product]', 'U'))
ALTER TABLE [Production].[Product] ADD CONSTRAINT [CK_Product_StandardCost] CHECK (([StandardCost]>=(0.00)))
GO
IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE object_id = OBJECT_ID(N'[Production].[CK_Product_ListPrice]', 'C') AND parent_object_id = OBJECT_ID(N'[Production].[Product]', 'U'))
ALTER TABLE [Production].[Product] ADD CONSTRAINT [CK_Product_ListPrice] CHECK (([ListPrice]>=(0.00)))
GO
IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE object_id = OBJECT_ID(N'[Production].[CK_Product_Weight]', 'C') AND parent_object_id = OBJECT_ID(N'[Production].[Product]', 'U'))
ALTER TABLE [Production].[Product] ADD CONSTRAINT [CK_Product_Weight] CHECK (([Weight]>(0.00)))
GO
IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE object_id = OBJECT_ID(N'[Production].[CK_Product_DaysToManufacture]', 'C') AND parent_object_id = OBJECT_ID(N'[Production].[Product]', 'U'))
ALTER TABLE [Production].[Product] ADD CONSTRAINT [CK_Product_DaysToManufacture] CHECK (([DaysToManufacture]>=(0)))
GO
IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE object_id = OBJECT_ID(N'[Production].[CK_Product_ProductLine]', 'C') AND parent_object_id = OBJECT_ID(N'[Production].[Product]', 'U'))
ALTER TABLE [Production].[Product] ADD CONSTRAINT [CK_Product_ProductLine] CHECK ((upper([ProductLine])='R' OR upper([ProductLine])='M' OR upper([ProductLine])='T' OR upper([ProductLine])='S' OR [ProductLine] IS NULL))
GO
IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE object_id = OBJECT_ID(N'[Production].[CK_Product_Class]', 'C') AND parent_object_id = OBJECT_ID(N'[Production].[Product]', 'U'))
ALTER TABLE [Production].[Product] ADD CONSTRAINT [CK_Product_Class] CHECK ((upper([Class])='H' OR upper([Class])='M' OR upper([Class])='L' OR [Class] IS NULL))
GO
IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE object_id = OBJECT_ID(N'[Production].[CK_Product_Style]', 'C') AND parent_object_id = OBJECT_ID(N'[Production].[Product]', 'U'))
ALTER TABLE [Production].[Product] ADD CONSTRAINT [CK_Product_Style] CHECK ((upper([Style])='U' OR upper([Style])='M' OR upper([Style])='W' OR [Style] IS NULL))
GO
IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE object_id = OBJECT_ID(N'[Production].[CK_Product_SellEndDate]', 'C') AND parent_object_id = OBJECT_ID(N'[Production].[Product]', 'U'))
ALTER TABLE [Production].[Product] ADD CONSTRAINT [CK_Product_SellEndDate] CHECK (([SellEndDate]>=[SellStartDate] OR [SellEndDate] IS NULL))
GO
PRINT N'Adding constraints to [Production].[Product_inmem]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE object_id = OBJECT_ID(N'[Production].[IMCK_Product_SafetyStockLevel]', 'C') AND parent_object_id = OBJECT_ID(N'[Production].[Product_inmem]', 'U'))
ALTER TABLE [Production].[Product_inmem] ADD CONSTRAINT [IMCK_Product_SafetyStockLevel] CHECK (([SafetyStockLevel]>(0)))
GO
IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE object_id = OBJECT_ID(N'[Production].[IMCK_Product_ReorderPoint]', 'C') AND parent_object_id = OBJECT_ID(N'[Production].[Product_inmem]', 'U'))
ALTER TABLE [Production].[Product_inmem] ADD CONSTRAINT [IMCK_Product_ReorderPoint] CHECK (([ReorderPoint]>(0)))
GO
IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE object_id = OBJECT_ID(N'[Production].[IMCK_Product_StandardCost]', 'C') AND parent_object_id = OBJECT_ID(N'[Production].[Product_inmem]', 'U'))
ALTER TABLE [Production].[Product_inmem] ADD CONSTRAINT [IMCK_Product_StandardCost] CHECK (([StandardCost]>=(0.00)))
GO
IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE object_id = OBJECT_ID(N'[Production].[IMCK_Product_ListPrice]', 'C') AND parent_object_id = OBJECT_ID(N'[Production].[Product_inmem]', 'U'))
ALTER TABLE [Production].[Product_inmem] ADD CONSTRAINT [IMCK_Product_ListPrice] CHECK (([ListPrice]>=(0.00)))
GO
IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE object_id = OBJECT_ID(N'[Production].[IMCK_Product_Weight]', 'C') AND parent_object_id = OBJECT_ID(N'[Production].[Product_inmem]', 'U'))
ALTER TABLE [Production].[Product_inmem] ADD CONSTRAINT [IMCK_Product_Weight] CHECK (([Weight]>(0.00)))
GO
IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE object_id = OBJECT_ID(N'[Production].[IMCK_Product_DaysToManufacture]', 'C') AND parent_object_id = OBJECT_ID(N'[Production].[Product_inmem]', 'U'))
ALTER TABLE [Production].[Product_inmem] ADD CONSTRAINT [IMCK_Product_DaysToManufacture] CHECK (([DaysToManufacture]>=(0)))
GO
IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE object_id = OBJECT_ID(N'[Production].[IMCK_Product_ProductLine]', 'C') AND parent_object_id = OBJECT_ID(N'[Production].[Product_inmem]', 'U'))
ALTER TABLE [Production].[Product_inmem] ADD CONSTRAINT [IMCK_Product_ProductLine] CHECK (([ProductLine]='R' OR [ProductLine]='r' OR [ProductLine]='M' OR [ProductLine]='m' OR [ProductLine]='T' OR [ProductLine]='t' OR [ProductLine]='S' OR [ProductLine]='s' OR [ProductLine] IS NULL))
GO
IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE object_id = OBJECT_ID(N'[Production].[IMCK_Product_Class]', 'C') AND parent_object_id = OBJECT_ID(N'[Production].[Product_inmem]', 'U'))
ALTER TABLE [Production].[Product_inmem] ADD CONSTRAINT [IMCK_Product_Class] CHECK (([Class]='H' OR [Class]='h' OR [Class]='M' OR [Class]='m' OR [Class]='L' OR [Class]='l' OR [Class] IS NULL))
GO
IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE object_id = OBJECT_ID(N'[Production].[IMCK_Product_Style]', 'C') AND parent_object_id = OBJECT_ID(N'[Production].[Product_inmem]', 'U'))
ALTER TABLE [Production].[Product_inmem] ADD CONSTRAINT [IMCK_Product_Style] CHECK (([Style]='U' OR [Style]='u' OR [Style]='M' OR [Style]='m' OR [Style]='W' OR [Style]='w' OR [Style] IS NULL))
GO
IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE object_id = OBJECT_ID(N'[Production].[IMCK_Product_SellEndDate]', 'C') AND parent_object_id = OBJECT_ID(N'[Production].[Product_inmem]', 'U'))
ALTER TABLE [Production].[Product_inmem] ADD CONSTRAINT [IMCK_Product_SellEndDate] CHECK (([SellEndDate]>=[SellStartDate] OR [SellEndDate] IS NULL))
GO
PRINT N'Adding constraints to [Production].[Product_ondisk]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE object_id = OBJECT_ID(N'[Production].[ODCK_Product_SafetyStockLevel]', 'C') AND parent_object_id = OBJECT_ID(N'[Production].[Product_ondisk]', 'U'))
ALTER TABLE [Production].[Product_ondisk] ADD CONSTRAINT [ODCK_Product_SafetyStockLevel] CHECK (([SafetyStockLevel]>(0)))
GO
IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE object_id = OBJECT_ID(N'[Production].[ODCK_Product_ReorderPoint]', 'C') AND parent_object_id = OBJECT_ID(N'[Production].[Product_ondisk]', 'U'))
ALTER TABLE [Production].[Product_ondisk] ADD CONSTRAINT [ODCK_Product_ReorderPoint] CHECK (([ReorderPoint]>(0)))
GO
IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE object_id = OBJECT_ID(N'[Production].[ODCK_Product_StandardCost]', 'C') AND parent_object_id = OBJECT_ID(N'[Production].[Product_ondisk]', 'U'))
ALTER TABLE [Production].[Product_ondisk] ADD CONSTRAINT [ODCK_Product_StandardCost] CHECK (([StandardCost]>=(0.00)))
GO
IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE object_id = OBJECT_ID(N'[Production].[ODCK_Product_ListPrice]', 'C') AND parent_object_id = OBJECT_ID(N'[Production].[Product_ondisk]', 'U'))
ALTER TABLE [Production].[Product_ondisk] ADD CONSTRAINT [ODCK_Product_ListPrice] CHECK (([ListPrice]>=(0.00)))
GO
IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE object_id = OBJECT_ID(N'[Production].[ODCK_Product_Weight]', 'C') AND parent_object_id = OBJECT_ID(N'[Production].[Product_ondisk]', 'U'))
ALTER TABLE [Production].[Product_ondisk] ADD CONSTRAINT [ODCK_Product_Weight] CHECK (([Weight]>(0.00)))
GO
IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE object_id = OBJECT_ID(N'[Production].[ODCK_Product_DaysToManufacture]', 'C') AND parent_object_id = OBJECT_ID(N'[Production].[Product_ondisk]', 'U'))
ALTER TABLE [Production].[Product_ondisk] ADD CONSTRAINT [ODCK_Product_DaysToManufacture] CHECK (([DaysToManufacture]>=(0)))
GO
IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE object_id = OBJECT_ID(N'[Production].[ODCK_Product_ProductLine]', 'C') AND parent_object_id = OBJECT_ID(N'[Production].[Product_ondisk]', 'U'))
ALTER TABLE [Production].[Product_ondisk] ADD CONSTRAINT [ODCK_Product_ProductLine] CHECK ((upper([ProductLine])='R' OR upper([ProductLine])='M' OR upper([ProductLine])='T' OR upper([ProductLine])='S' OR [ProductLine] IS NULL))
GO
IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE object_id = OBJECT_ID(N'[Production].[ODCK_Product_Class]', 'C') AND parent_object_id = OBJECT_ID(N'[Production].[Product_ondisk]', 'U'))
ALTER TABLE [Production].[Product_ondisk] ADD CONSTRAINT [ODCK_Product_Class] CHECK ((upper([Class])='H' OR upper([Class])='M' OR upper([Class])='L' OR [Class] IS NULL))
GO
IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE object_id = OBJECT_ID(N'[Production].[ODCK_Product_Style]', 'C') AND parent_object_id = OBJECT_ID(N'[Production].[Product_ondisk]', 'U'))
ALTER TABLE [Production].[Product_ondisk] ADD CONSTRAINT [ODCK_Product_Style] CHECK ((upper([Style])='U' OR upper([Style])='M' OR upper([Style])='W' OR [Style] IS NULL))
GO
IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE object_id = OBJECT_ID(N'[Production].[ODCK_Product_SellEndDate]', 'C') AND parent_object_id = OBJECT_ID(N'[Production].[Product_ondisk]', 'U'))
ALTER TABLE [Production].[Product_ondisk] ADD CONSTRAINT [ODCK_Product_SellEndDate] CHECK (([SellEndDate]>=[SellStartDate] OR [SellEndDate] IS NULL))
GO
PRINT N'Adding constraints to [Production].[TransactionHistoryArchive]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE object_id = OBJECT_ID(N'[Production].[CK_TransactionHistoryArchive_TransactionType]', 'C') AND parent_object_id = OBJECT_ID(N'[Production].[TransactionHistoryArchive]', 'U'))
ALTER TABLE [Production].[TransactionHistoryArchive] ADD CONSTRAINT [CK_TransactionHistoryArchive_TransactionType] CHECK ((upper([TransactionType])='P' OR upper([TransactionType])='S' OR upper([TransactionType])='W'))
GO
PRINT N'Adding constraints to [Production].[TransactionHistory]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE object_id = OBJECT_ID(N'[Production].[CK_TransactionHistory_TransactionType]', 'C') AND parent_object_id = OBJECT_ID(N'[Production].[TransactionHistory]', 'U'))
ALTER TABLE [Production].[TransactionHistory] ADD CONSTRAINT [CK_TransactionHistory_TransactionType] CHECK ((upper([TransactionType])='P' OR upper([TransactionType])='S' OR upper([TransactionType])='W'))
GO
PRINT N'Adding constraints to [Production].[WorkOrderRouting]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE object_id = OBJECT_ID(N'[Production].[CK_WorkOrderRouting_ActualResourceHrs]', 'C') AND parent_object_id = OBJECT_ID(N'[Production].[WorkOrderRouting]', 'U'))
ALTER TABLE [Production].[WorkOrderRouting] ADD CONSTRAINT [CK_WorkOrderRouting_ActualResourceHrs] CHECK (([ActualResourceHrs]>=(0.0000)))
GO
IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE object_id = OBJECT_ID(N'[Production].[CK_WorkOrderRouting_PlannedCost]', 'C') AND parent_object_id = OBJECT_ID(N'[Production].[WorkOrderRouting]', 'U'))
ALTER TABLE [Production].[WorkOrderRouting] ADD CONSTRAINT [CK_WorkOrderRouting_PlannedCost] CHECK (([PlannedCost]>(0.00)))
GO
IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE object_id = OBJECT_ID(N'[Production].[CK_WorkOrderRouting_ActualCost]', 'C') AND parent_object_id = OBJECT_ID(N'[Production].[WorkOrderRouting]', 'U'))
ALTER TABLE [Production].[WorkOrderRouting] ADD CONSTRAINT [CK_WorkOrderRouting_ActualCost] CHECK (([ActualCost]>(0.00)))
GO
IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE object_id = OBJECT_ID(N'[Production].[CK_WorkOrderRouting_ActualEndDate]', 'C') AND parent_object_id = OBJECT_ID(N'[Production].[WorkOrderRouting]', 'U'))
ALTER TABLE [Production].[WorkOrderRouting] ADD CONSTRAINT [CK_WorkOrderRouting_ActualEndDate] CHECK (([ActualEndDate]>=[ActualStartDate] OR [ActualEndDate] IS NULL OR [ActualStartDate] IS NULL))
GO
IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE object_id = OBJECT_ID(N'[Production].[CK_WorkOrderRouting_ScheduledEndDate]', 'C') AND parent_object_id = OBJECT_ID(N'[Production].[WorkOrderRouting]', 'U'))
ALTER TABLE [Production].[WorkOrderRouting] ADD CONSTRAINT [CK_WorkOrderRouting_ScheduledEndDate] CHECK (([ScheduledEndDate]>=[ScheduledStartDate]))
GO
PRINT N'Adding constraints to [Production].[WorkOrder]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE object_id = OBJECT_ID(N'[Production].[CK_WorkOrder_OrderQty]', 'C') AND parent_object_id = OBJECT_ID(N'[Production].[WorkOrder]', 'U'))
ALTER TABLE [Production].[WorkOrder] ADD CONSTRAINT [CK_WorkOrder_OrderQty] CHECK (([OrderQty]>(0)))
GO
IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE object_id = OBJECT_ID(N'[Production].[CK_WorkOrder_ScrappedQty]', 'C') AND parent_object_id = OBJECT_ID(N'[Production].[WorkOrder]', 'U'))
ALTER TABLE [Production].[WorkOrder] ADD CONSTRAINT [CK_WorkOrder_ScrappedQty] CHECK (([ScrappedQty]>=(0)))
GO
IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE object_id = OBJECT_ID(N'[Production].[CK_WorkOrder_EndDate]', 'C') AND parent_object_id = OBJECT_ID(N'[Production].[WorkOrder]', 'U'))
ALTER TABLE [Production].[WorkOrder] ADD CONSTRAINT [CK_WorkOrder_EndDate] CHECK (([EndDate]>=[StartDate] OR [EndDate] IS NULL))
GO
PRINT N'Adding constraints to [Purchasing].[ProductVendor]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE object_id = OBJECT_ID(N'[Purchasing].[CK_ProductVendor_AverageLeadTime]', 'C') AND parent_object_id = OBJECT_ID(N'[Purchasing].[ProductVendor]', 'U'))
ALTER TABLE [Purchasing].[ProductVendor] ADD CONSTRAINT [CK_ProductVendor_AverageLeadTime] CHECK (([AverageLeadTime]>=(1)))
GO
IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE object_id = OBJECT_ID(N'[Purchasing].[CK_ProductVendor_StandardPrice]', 'C') AND parent_object_id = OBJECT_ID(N'[Purchasing].[ProductVendor]', 'U'))
ALTER TABLE [Purchasing].[ProductVendor] ADD CONSTRAINT [CK_ProductVendor_StandardPrice] CHECK (([StandardPrice]>(0.00)))
GO
IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE object_id = OBJECT_ID(N'[Purchasing].[CK_ProductVendor_LastReceiptCost]', 'C') AND parent_object_id = OBJECT_ID(N'[Purchasing].[ProductVendor]', 'U'))
ALTER TABLE [Purchasing].[ProductVendor] ADD CONSTRAINT [CK_ProductVendor_LastReceiptCost] CHECK (([LastReceiptCost]>(0.00)))
GO
IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE object_id = OBJECT_ID(N'[Purchasing].[CK_ProductVendor_MinOrderQty]', 'C') AND parent_object_id = OBJECT_ID(N'[Purchasing].[ProductVendor]', 'U'))
ALTER TABLE [Purchasing].[ProductVendor] ADD CONSTRAINT [CK_ProductVendor_MinOrderQty] CHECK (([MinOrderQty]>=(1)))
GO
IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE object_id = OBJECT_ID(N'[Purchasing].[CK_ProductVendor_MaxOrderQty]', 'C') AND parent_object_id = OBJECT_ID(N'[Purchasing].[ProductVendor]', 'U'))
ALTER TABLE [Purchasing].[ProductVendor] ADD CONSTRAINT [CK_ProductVendor_MaxOrderQty] CHECK (([MaxOrderQty]>=(1)))
GO
IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE object_id = OBJECT_ID(N'[Purchasing].[CK_ProductVendor_OnOrderQty]', 'C') AND parent_object_id = OBJECT_ID(N'[Purchasing].[ProductVendor]', 'U'))
ALTER TABLE [Purchasing].[ProductVendor] ADD CONSTRAINT [CK_ProductVendor_OnOrderQty] CHECK (([OnOrderQty]>=(0)))
GO
PRINT N'Adding constraints to [Purchasing].[PurchaseOrderDetail]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE object_id = OBJECT_ID(N'[Purchasing].[CK_PurchaseOrderDetail_OrderQty]', 'C') AND parent_object_id = OBJECT_ID(N'[Purchasing].[PurchaseOrderDetail]', 'U'))
ALTER TABLE [Purchasing].[PurchaseOrderDetail] ADD CONSTRAINT [CK_PurchaseOrderDetail_OrderQty] CHECK (([OrderQty]>(0)))
GO
IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE object_id = OBJECT_ID(N'[Purchasing].[CK_PurchaseOrderDetail_UnitPrice]', 'C') AND parent_object_id = OBJECT_ID(N'[Purchasing].[PurchaseOrderDetail]', 'U'))
ALTER TABLE [Purchasing].[PurchaseOrderDetail] ADD CONSTRAINT [CK_PurchaseOrderDetail_UnitPrice] CHECK (([UnitPrice]>=(0.00)))
GO
IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE object_id = OBJECT_ID(N'[Purchasing].[CK_PurchaseOrderDetail_ReceivedQty]', 'C') AND parent_object_id = OBJECT_ID(N'[Purchasing].[PurchaseOrderDetail]', 'U'))
ALTER TABLE [Purchasing].[PurchaseOrderDetail] ADD CONSTRAINT [CK_PurchaseOrderDetail_ReceivedQty] CHECK (([ReceivedQty]>=(0.00)))
GO
IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE object_id = OBJECT_ID(N'[Purchasing].[CK_PurchaseOrderDetail_RejectedQty]', 'C') AND parent_object_id = OBJECT_ID(N'[Purchasing].[PurchaseOrderDetail]', 'U'))
ALTER TABLE [Purchasing].[PurchaseOrderDetail] ADD CONSTRAINT [CK_PurchaseOrderDetail_RejectedQty] CHECK (([RejectedQty]>=(0.00)))
GO
PRINT N'Adding constraints to [Purchasing].[PurchaseOrderHeader]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE object_id = OBJECT_ID(N'[Purchasing].[CK_PurchaseOrderHeader_Status]', 'C') AND parent_object_id = OBJECT_ID(N'[Purchasing].[PurchaseOrderHeader]', 'U'))
ALTER TABLE [Purchasing].[PurchaseOrderHeader] ADD CONSTRAINT [CK_PurchaseOrderHeader_Status] CHECK (([Status]>=(1) AND [Status]<=(4)))
GO
IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE object_id = OBJECT_ID(N'[Purchasing].[CK_PurchaseOrderHeader_SubTotal]', 'C') AND parent_object_id = OBJECT_ID(N'[Purchasing].[PurchaseOrderHeader]', 'U'))
ALTER TABLE [Purchasing].[PurchaseOrderHeader] ADD CONSTRAINT [CK_PurchaseOrderHeader_SubTotal] CHECK (([SubTotal]>=(0.00)))
GO
IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE object_id = OBJECT_ID(N'[Purchasing].[CK_PurchaseOrderHeader_TaxAmt]', 'C') AND parent_object_id = OBJECT_ID(N'[Purchasing].[PurchaseOrderHeader]', 'U'))
ALTER TABLE [Purchasing].[PurchaseOrderHeader] ADD CONSTRAINT [CK_PurchaseOrderHeader_TaxAmt] CHECK (([TaxAmt]>=(0.00)))
GO
IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE object_id = OBJECT_ID(N'[Purchasing].[CK_PurchaseOrderHeader_Freight]', 'C') AND parent_object_id = OBJECT_ID(N'[Purchasing].[PurchaseOrderHeader]', 'U'))
ALTER TABLE [Purchasing].[PurchaseOrderHeader] ADD CONSTRAINT [CK_PurchaseOrderHeader_Freight] CHECK (([Freight]>=(0.00)))
GO
IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE object_id = OBJECT_ID(N'[Purchasing].[CK_PurchaseOrderHeader_ShipDate]', 'C') AND parent_object_id = OBJECT_ID(N'[Purchasing].[PurchaseOrderHeader]', 'U'))
ALTER TABLE [Purchasing].[PurchaseOrderHeader] ADD CONSTRAINT [CK_PurchaseOrderHeader_ShipDate] CHECK (([ShipDate]>=[OrderDate] OR [ShipDate] IS NULL))
GO
PRINT N'Adding constraints to [Purchasing].[ShipMethod]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE object_id = OBJECT_ID(N'[Purchasing].[CK_ShipMethod_ShipBase]', 'C') AND parent_object_id = OBJECT_ID(N'[Purchasing].[ShipMethod]', 'U'))
ALTER TABLE [Purchasing].[ShipMethod] ADD CONSTRAINT [CK_ShipMethod_ShipBase] CHECK (([ShipBase]>(0.00)))
GO
IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE object_id = OBJECT_ID(N'[Purchasing].[CK_ShipMethod_ShipRate]', 'C') AND parent_object_id = OBJECT_ID(N'[Purchasing].[ShipMethod]', 'U'))
ALTER TABLE [Purchasing].[ShipMethod] ADD CONSTRAINT [CK_ShipMethod_ShipRate] CHECK (([ShipRate]>(0.00)))
GO
PRINT N'Adding constraints to [Purchasing].[Vendor]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE object_id = OBJECT_ID(N'[Purchasing].[CK_Vendor_CreditRating]', 'C') AND parent_object_id = OBJECT_ID(N'[Purchasing].[Vendor]', 'U'))
ALTER TABLE [Purchasing].[Vendor] ADD CONSTRAINT [CK_Vendor_CreditRating] CHECK (([CreditRating]>=(1) AND [CreditRating]<=(5)))
GO
PRINT N'Adding constraints to [Sales].[SalesOrderDetail]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE object_id = OBJECT_ID(N'[Sales].[CK_SalesOrderDetail_OrderQty]', 'C') AND parent_object_id = OBJECT_ID(N'[Sales].[SalesOrderDetail]', 'U'))
ALTER TABLE [Sales].[SalesOrderDetail] ADD CONSTRAINT [CK_SalesOrderDetail_OrderQty] CHECK (([OrderQty]>(0)))
GO
IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE object_id = OBJECT_ID(N'[Sales].[CK_SalesOrderDetail_UnitPrice]', 'C') AND parent_object_id = OBJECT_ID(N'[Sales].[SalesOrderDetail]', 'U'))
ALTER TABLE [Sales].[SalesOrderDetail] ADD CONSTRAINT [CK_SalesOrderDetail_UnitPrice] CHECK (([UnitPrice]>=(0.00)))
GO
IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE object_id = OBJECT_ID(N'[Sales].[CK_SalesOrderDetail_UnitPriceDiscount]', 'C') AND parent_object_id = OBJECT_ID(N'[Sales].[SalesOrderDetail]', 'U'))
ALTER TABLE [Sales].[SalesOrderDetail] ADD CONSTRAINT [CK_SalesOrderDetail_UnitPriceDiscount] CHECK (([UnitPriceDiscount]>=(0.00)))
GO
PRINT N'Adding constraints to [Sales].[SalesOrderDetail_inmem]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE object_id = OBJECT_ID(N'[Sales].[IMCK_SalesOrderDetail_OrderQty]', 'C') AND parent_object_id = OBJECT_ID(N'[Sales].[SalesOrderDetail_inmem]', 'U'))
ALTER TABLE [Sales].[SalesOrderDetail_inmem] ADD CONSTRAINT [IMCK_SalesOrderDetail_OrderQty] CHECK (([OrderQty]>(0)))
GO
IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE object_id = OBJECT_ID(N'[Sales].[IMCK_SalesOrderDetail_UnitPrice]', 'C') AND parent_object_id = OBJECT_ID(N'[Sales].[SalesOrderDetail_inmem]', 'U'))
ALTER TABLE [Sales].[SalesOrderDetail_inmem] ADD CONSTRAINT [IMCK_SalesOrderDetail_UnitPrice] CHECK (([UnitPrice]>=(0.00)))
GO
IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE object_id = OBJECT_ID(N'[Sales].[IMCK_SalesOrderDetail_UnitPriceDiscount]', 'C') AND parent_object_id = OBJECT_ID(N'[Sales].[SalesOrderDetail_inmem]', 'U'))
ALTER TABLE [Sales].[SalesOrderDetail_inmem] ADD CONSTRAINT [IMCK_SalesOrderDetail_UnitPriceDiscount] CHECK (([UnitPriceDiscount]>=(0.00)))
GO
PRINT N'Adding constraints to [Sales].[SalesOrderDetail_ondisk]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE object_id = OBJECT_ID(N'[Sales].[ODCK_SalesOrderDetail_OrderQty]', 'C') AND parent_object_id = OBJECT_ID(N'[Sales].[SalesOrderDetail_ondisk]', 'U'))
ALTER TABLE [Sales].[SalesOrderDetail_ondisk] ADD CONSTRAINT [ODCK_SalesOrderDetail_OrderQty] CHECK (([OrderQty]>(0)))
GO
IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE object_id = OBJECT_ID(N'[Sales].[ODCK_SalesOrderDetail_UnitPrice]', 'C') AND parent_object_id = OBJECT_ID(N'[Sales].[SalesOrderDetail_ondisk]', 'U'))
ALTER TABLE [Sales].[SalesOrderDetail_ondisk] ADD CONSTRAINT [ODCK_SalesOrderDetail_UnitPrice] CHECK (([UnitPrice]>=(0.00)))
GO
IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE object_id = OBJECT_ID(N'[Sales].[ODCK_SalesOrderDetail_UnitPriceDiscount]', 'C') AND parent_object_id = OBJECT_ID(N'[Sales].[SalesOrderDetail_ondisk]', 'U'))
ALTER TABLE [Sales].[SalesOrderDetail_ondisk] ADD CONSTRAINT [ODCK_SalesOrderDetail_UnitPriceDiscount] CHECK (([UnitPriceDiscount]>=(0.00)))
GO
PRINT N'Adding constraints to [Sales].[SalesOrderHeader]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE object_id = OBJECT_ID(N'[Sales].[CK_SalesOrderHeader_Status]', 'C') AND parent_object_id = OBJECT_ID(N'[Sales].[SalesOrderHeader]', 'U'))
ALTER TABLE [Sales].[SalesOrderHeader] ADD CONSTRAINT [CK_SalesOrderHeader_Status] CHECK (([Status]>=(0) AND [Status]<=(8)))
GO
IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE object_id = OBJECT_ID(N'[Sales].[CK_SalesOrderHeader_SubTotal]', 'C') AND parent_object_id = OBJECT_ID(N'[Sales].[SalesOrderHeader]', 'U'))
ALTER TABLE [Sales].[SalesOrderHeader] ADD CONSTRAINT [CK_SalesOrderHeader_SubTotal] CHECK (([SubTotal]>=(0.00)))
GO
IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE object_id = OBJECT_ID(N'[Sales].[CK_SalesOrderHeader_TaxAmt]', 'C') AND parent_object_id = OBJECT_ID(N'[Sales].[SalesOrderHeader]', 'U'))
ALTER TABLE [Sales].[SalesOrderHeader] ADD CONSTRAINT [CK_SalesOrderHeader_TaxAmt] CHECK (([TaxAmt]>=(0.00)))
GO
IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE object_id = OBJECT_ID(N'[Sales].[CK_SalesOrderHeader_Freight]', 'C') AND parent_object_id = OBJECT_ID(N'[Sales].[SalesOrderHeader]', 'U'))
ALTER TABLE [Sales].[SalesOrderHeader] ADD CONSTRAINT [CK_SalesOrderHeader_Freight] CHECK (([Freight]>=(0.00)))
GO
IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE object_id = OBJECT_ID(N'[Sales].[CK_SalesOrderHeader_DueDate]', 'C') AND parent_object_id = OBJECT_ID(N'[Sales].[SalesOrderHeader]', 'U'))
ALTER TABLE [Sales].[SalesOrderHeader] ADD CONSTRAINT [CK_SalesOrderHeader_DueDate] CHECK (([DueDate]>=[OrderDate]))
GO
IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE object_id = OBJECT_ID(N'[Sales].[CK_SalesOrderHeader_ShipDate]', 'C') AND parent_object_id = OBJECT_ID(N'[Sales].[SalesOrderHeader]', 'U'))
ALTER TABLE [Sales].[SalesOrderHeader] ADD CONSTRAINT [CK_SalesOrderHeader_ShipDate] CHECK (([ShipDate]>=[OrderDate] OR [ShipDate] IS NULL))
GO
PRINT N'Adding constraints to [Sales].[SalesOrderHeader_inmem]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE object_id = OBJECT_ID(N'[Sales].[IMCK_SalesOrderHeader_Status]', 'C') AND parent_object_id = OBJECT_ID(N'[Sales].[SalesOrderHeader_inmem]', 'U'))
ALTER TABLE [Sales].[SalesOrderHeader_inmem] ADD CONSTRAINT [IMCK_SalesOrderHeader_Status] CHECK (([Status]>=(0) AND [Status]<=(8)))
GO
IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE object_id = OBJECT_ID(N'[Sales].[IMCK_SalesOrderHeader_SubTotal]', 'C') AND parent_object_id = OBJECT_ID(N'[Sales].[SalesOrderHeader_inmem]', 'U'))
ALTER TABLE [Sales].[SalesOrderHeader_inmem] ADD CONSTRAINT [IMCK_SalesOrderHeader_SubTotal] CHECK (([SubTotal]>=(0.00)))
GO
IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE object_id = OBJECT_ID(N'[Sales].[IMCK_SalesOrderHeader_TaxAmt]', 'C') AND parent_object_id = OBJECT_ID(N'[Sales].[SalesOrderHeader_inmem]', 'U'))
ALTER TABLE [Sales].[SalesOrderHeader_inmem] ADD CONSTRAINT [IMCK_SalesOrderHeader_TaxAmt] CHECK (([TaxAmt]>=(0.00)))
GO
IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE object_id = OBJECT_ID(N'[Sales].[IMCK_SalesOrderHeader_Freight]', 'C') AND parent_object_id = OBJECT_ID(N'[Sales].[SalesOrderHeader_inmem]', 'U'))
ALTER TABLE [Sales].[SalesOrderHeader_inmem] ADD CONSTRAINT [IMCK_SalesOrderHeader_Freight] CHECK (([Freight]>=(0.00)))
GO
IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE object_id = OBJECT_ID(N'[Sales].[IMCK_SalesOrderHeader_DueDate]', 'C') AND parent_object_id = OBJECT_ID(N'[Sales].[SalesOrderHeader_inmem]', 'U'))
ALTER TABLE [Sales].[SalesOrderHeader_inmem] ADD CONSTRAINT [IMCK_SalesOrderHeader_DueDate] CHECK (([DueDate]>=[OrderDate]))
GO
IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE object_id = OBJECT_ID(N'[Sales].[IMCK_SalesOrderHeader_ShipDate]', 'C') AND parent_object_id = OBJECT_ID(N'[Sales].[SalesOrderHeader_inmem]', 'U'))
ALTER TABLE [Sales].[SalesOrderHeader_inmem] ADD CONSTRAINT [IMCK_SalesOrderHeader_ShipDate] CHECK (([ShipDate]>=[OrderDate] OR [ShipDate] IS NULL))
GO
PRINT N'Adding constraints to [Sales].[SalesOrderHeader_ondisk]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE object_id = OBJECT_ID(N'[Sales].[ODCK_SalesOrderHeader_Status]', 'C') AND parent_object_id = OBJECT_ID(N'[Sales].[SalesOrderHeader_ondisk]', 'U'))
ALTER TABLE [Sales].[SalesOrderHeader_ondisk] ADD CONSTRAINT [ODCK_SalesOrderHeader_Status] CHECK (([Status]>=(0) AND [Status]<=(8)))
GO
IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE object_id = OBJECT_ID(N'[Sales].[ODCK_SalesOrderHeader_SubTotal]', 'C') AND parent_object_id = OBJECT_ID(N'[Sales].[SalesOrderHeader_ondisk]', 'U'))
ALTER TABLE [Sales].[SalesOrderHeader_ondisk] ADD CONSTRAINT [ODCK_SalesOrderHeader_SubTotal] CHECK (([SubTotal]>=(0.00)))
GO
IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE object_id = OBJECT_ID(N'[Sales].[ODCK_SalesOrderHeader_TaxAmt]', 'C') AND parent_object_id = OBJECT_ID(N'[Sales].[SalesOrderHeader_ondisk]', 'U'))
ALTER TABLE [Sales].[SalesOrderHeader_ondisk] ADD CONSTRAINT [ODCK_SalesOrderHeader_TaxAmt] CHECK (([TaxAmt]>=(0.00)))
GO
IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE object_id = OBJECT_ID(N'[Sales].[ODCK_SalesOrderHeader_Freight]', 'C') AND parent_object_id = OBJECT_ID(N'[Sales].[SalesOrderHeader_ondisk]', 'U'))
ALTER TABLE [Sales].[SalesOrderHeader_ondisk] ADD CONSTRAINT [ODCK_SalesOrderHeader_Freight] CHECK (([Freight]>=(0.00)))
GO
IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE object_id = OBJECT_ID(N'[Sales].[ODCK_SalesOrderHeader_DueDate]', 'C') AND parent_object_id = OBJECT_ID(N'[Sales].[SalesOrderHeader_ondisk]', 'U'))
ALTER TABLE [Sales].[SalesOrderHeader_ondisk] ADD CONSTRAINT [ODCK_SalesOrderHeader_DueDate] CHECK (([DueDate]>=[OrderDate]))
GO
IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE object_id = OBJECT_ID(N'[Sales].[ODCK_SalesOrderHeader_ShipDate]', 'C') AND parent_object_id = OBJECT_ID(N'[Sales].[SalesOrderHeader_ondisk]', 'U'))
ALTER TABLE [Sales].[SalesOrderHeader_ondisk] ADD CONSTRAINT [ODCK_SalesOrderHeader_ShipDate] CHECK (([ShipDate]>=[OrderDate] OR [ShipDate] IS NULL))
GO
PRINT N'Adding constraints to [Sales].[SalesOrder_json]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE object_id = OBJECT_ID(N'[Sales].[SalesOrder reasons must be formatted as JSON array]', 'C') AND parent_object_id = OBJECT_ID(N'[Sales].[SalesOrder_json]', 'U'))
ALTER TABLE [Sales].[SalesOrder_json] ADD CONSTRAINT [SalesOrder reasons must be formatted as JSON array] CHECK ((isjson([SalesReasons])>(0)))
GO
IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE object_id = OBJECT_ID(N'[Sales].[SalesOrder items must be formatted as JSON array]', 'C') AND parent_object_id = OBJECT_ID(N'[Sales].[SalesOrder_json]', 'U'))
ALTER TABLE [Sales].[SalesOrder_json] ADD CONSTRAINT [SalesOrder items must be formatted as JSON array] CHECK ((isjson([OrderItems])>(0)))
GO
IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE object_id = OBJECT_ID(N'[Sales].[SalesOrder additional information must be formatted as JSON]', 'C') AND parent_object_id = OBJECT_ID(N'[Sales].[SalesOrder_json]', 'U'))
ALTER TABLE [Sales].[SalesOrder_json] ADD CONSTRAINT [SalesOrder additional information must be formatted as JSON] CHECK ((isjson([Info])>(0)))
GO
PRINT N'Adding constraints to [Sales].[SalesPersonQuotaHistory]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE object_id = OBJECT_ID(N'[Sales].[CK_SalesPersonQuotaHistory_SalesQuota]', 'C') AND parent_object_id = OBJECT_ID(N'[Sales].[SalesPersonQuotaHistory]', 'U'))
ALTER TABLE [Sales].[SalesPersonQuotaHistory] ADD CONSTRAINT [CK_SalesPersonQuotaHistory_SalesQuota] CHECK (([SalesQuota]>(0.00)))
GO
PRINT N'Adding constraints to [Sales].[SalesPerson]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE object_id = OBJECT_ID(N'[Sales].[CK_SalesPerson_SalesQuota]', 'C') AND parent_object_id = OBJECT_ID(N'[Sales].[SalesPerson]', 'U'))
ALTER TABLE [Sales].[SalesPerson] ADD CONSTRAINT [CK_SalesPerson_SalesQuota] CHECK (([SalesQuota]>(0.00)))
GO
IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE object_id = OBJECT_ID(N'[Sales].[CK_SalesPerson_Bonus]', 'C') AND parent_object_id = OBJECT_ID(N'[Sales].[SalesPerson]', 'U'))
ALTER TABLE [Sales].[SalesPerson] ADD CONSTRAINT [CK_SalesPerson_Bonus] CHECK (([Bonus]>=(0.00)))
GO
IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE object_id = OBJECT_ID(N'[Sales].[CK_SalesPerson_CommissionPct]', 'C') AND parent_object_id = OBJECT_ID(N'[Sales].[SalesPerson]', 'U'))
ALTER TABLE [Sales].[SalesPerson] ADD CONSTRAINT [CK_SalesPerson_CommissionPct] CHECK (([CommissionPct]>=(0.00)))
GO
IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE object_id = OBJECT_ID(N'[Sales].[CK_SalesPerson_SalesYTD]', 'C') AND parent_object_id = OBJECT_ID(N'[Sales].[SalesPerson]', 'U'))
ALTER TABLE [Sales].[SalesPerson] ADD CONSTRAINT [CK_SalesPerson_SalesYTD] CHECK (([SalesYTD]>=(0.00)))
GO
IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE object_id = OBJECT_ID(N'[Sales].[CK_SalesPerson_SalesLastYear]', 'C') AND parent_object_id = OBJECT_ID(N'[Sales].[SalesPerson]', 'U'))
ALTER TABLE [Sales].[SalesPerson] ADD CONSTRAINT [CK_SalesPerson_SalesLastYear] CHECK (([SalesLastYear]>=(0.00)))
GO
PRINT N'Adding constraints to [Sales].[SalesTaxRate]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE object_id = OBJECT_ID(N'[Sales].[CK_SalesTaxRate_TaxType]', 'C') AND parent_object_id = OBJECT_ID(N'[Sales].[SalesTaxRate]', 'U'))
ALTER TABLE [Sales].[SalesTaxRate] ADD CONSTRAINT [CK_SalesTaxRate_TaxType] CHECK (([TaxType]>=(1) AND [TaxType]<=(3)))
GO
PRINT N'Adding constraints to [Sales].[SalesTerritoryHistory]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE object_id = OBJECT_ID(N'[Sales].[CK_SalesTerritoryHistory_EndDate]', 'C') AND parent_object_id = OBJECT_ID(N'[Sales].[SalesTerritoryHistory]', 'U'))
ALTER TABLE [Sales].[SalesTerritoryHistory] ADD CONSTRAINT [CK_SalesTerritoryHistory_EndDate] CHECK (([EndDate]>=[StartDate] OR [EndDate] IS NULL))
GO
PRINT N'Adding constraints to [Sales].[SalesTerritory]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE object_id = OBJECT_ID(N'[Sales].[CK_SalesTerritory_SalesYTD]', 'C') AND parent_object_id = OBJECT_ID(N'[Sales].[SalesTerritory]', 'U'))
ALTER TABLE [Sales].[SalesTerritory] ADD CONSTRAINT [CK_SalesTerritory_SalesYTD] CHECK (([SalesYTD]>=(0.00)))
GO
IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE object_id = OBJECT_ID(N'[Sales].[CK_SalesTerritory_SalesLastYear]', 'C') AND parent_object_id = OBJECT_ID(N'[Sales].[SalesTerritory]', 'U'))
ALTER TABLE [Sales].[SalesTerritory] ADD CONSTRAINT [CK_SalesTerritory_SalesLastYear] CHECK (([SalesLastYear]>=(0.00)))
GO
IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE object_id = OBJECT_ID(N'[Sales].[CK_SalesTerritory_CostYTD]', 'C') AND parent_object_id = OBJECT_ID(N'[Sales].[SalesTerritory]', 'U'))
ALTER TABLE [Sales].[SalesTerritory] ADD CONSTRAINT [CK_SalesTerritory_CostYTD] CHECK (([CostYTD]>=(0.00)))
GO
IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE object_id = OBJECT_ID(N'[Sales].[CK_SalesTerritory_CostLastYear]', 'C') AND parent_object_id = OBJECT_ID(N'[Sales].[SalesTerritory]', 'U'))
ALTER TABLE [Sales].[SalesTerritory] ADD CONSTRAINT [CK_SalesTerritory_CostLastYear] CHECK (([CostLastYear]>=(0.00)))
GO
PRINT N'Adding constraints to [Sales].[ShoppingCartItem]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE object_id = OBJECT_ID(N'[Sales].[CK_ShoppingCartItem_Quantity]', 'C') AND parent_object_id = OBJECT_ID(N'[Sales].[ShoppingCartItem]', 'U'))
ALTER TABLE [Sales].[ShoppingCartItem] ADD CONSTRAINT [CK_ShoppingCartItem_Quantity] CHECK (([Quantity]>=(1)))
GO
PRINT N'Adding constraints to [Sales].[SpecialOffer]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE object_id = OBJECT_ID(N'[Sales].[CK_SpecialOffer_DiscountPct]', 'C') AND parent_object_id = OBJECT_ID(N'[Sales].[SpecialOffer]', 'U'))
ALTER TABLE [Sales].[SpecialOffer] ADD CONSTRAINT [CK_SpecialOffer_DiscountPct] CHECK (([DiscountPct]>=(0.00)))
GO
IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE object_id = OBJECT_ID(N'[Sales].[CK_SpecialOffer_MinQty]', 'C') AND parent_object_id = OBJECT_ID(N'[Sales].[SpecialOffer]', 'U'))
ALTER TABLE [Sales].[SpecialOffer] ADD CONSTRAINT [CK_SpecialOffer_MinQty] CHECK (([MinQty]>=(0)))
GO
IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE object_id = OBJECT_ID(N'[Sales].[CK_SpecialOffer_MaxQty]', 'C') AND parent_object_id = OBJECT_ID(N'[Sales].[SpecialOffer]', 'U'))
ALTER TABLE [Sales].[SpecialOffer] ADD CONSTRAINT [CK_SpecialOffer_MaxQty] CHECK (([MaxQty]>=(0)))
GO
IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE object_id = OBJECT_ID(N'[Sales].[CK_SpecialOffer_EndDate]', 'C') AND parent_object_id = OBJECT_ID(N'[Sales].[SpecialOffer]', 'U'))
ALTER TABLE [Sales].[SpecialOffer] ADD CONSTRAINT [CK_SpecialOffer_EndDate] CHECK (([EndDate]>=[StartDate]))
GO
PRINT N'Adding constraints to [Sales].[SpecialOffer_inmem]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE object_id = OBJECT_ID(N'[Sales].[IMCK_SpecialOffer_DiscountPct]', 'C') AND parent_object_id = OBJECT_ID(N'[Sales].[SpecialOffer_inmem]', 'U'))
ALTER TABLE [Sales].[SpecialOffer_inmem] ADD CONSTRAINT [IMCK_SpecialOffer_DiscountPct] CHECK (([DiscountPct]>=(0.00)))
GO
IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE object_id = OBJECT_ID(N'[Sales].[IMCK_SpecialOffer_MinQty]', 'C') AND parent_object_id = OBJECT_ID(N'[Sales].[SpecialOffer_inmem]', 'U'))
ALTER TABLE [Sales].[SpecialOffer_inmem] ADD CONSTRAINT [IMCK_SpecialOffer_MinQty] CHECK (([MinQty]>=(0)))
GO
IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE object_id = OBJECT_ID(N'[Sales].[IMCK_SpecialOffer_MaxQty]', 'C') AND parent_object_id = OBJECT_ID(N'[Sales].[SpecialOffer_inmem]', 'U'))
ALTER TABLE [Sales].[SpecialOffer_inmem] ADD CONSTRAINT [IMCK_SpecialOffer_MaxQty] CHECK (([MaxQty]>=(0)))
GO
IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE object_id = OBJECT_ID(N'[Sales].[IMCK_SpecialOffer_EndDate]', 'C') AND parent_object_id = OBJECT_ID(N'[Sales].[SpecialOffer_inmem]', 'U'))
ALTER TABLE [Sales].[SpecialOffer_inmem] ADD CONSTRAINT [IMCK_SpecialOffer_EndDate] CHECK (([EndDate]>=[StartDate]))
GO
PRINT N'Adding constraints to [Sales].[SpecialOffer_ondisk]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE object_id = OBJECT_ID(N'[Sales].[ODCK_SpecialOffer_DiscountPct]', 'C') AND parent_object_id = OBJECT_ID(N'[Sales].[SpecialOffer_ondisk]', 'U'))
ALTER TABLE [Sales].[SpecialOffer_ondisk] ADD CONSTRAINT [ODCK_SpecialOffer_DiscountPct] CHECK (([DiscountPct]>=(0.00)))
GO
IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE object_id = OBJECT_ID(N'[Sales].[ODCK_SpecialOffer_MinQty]', 'C') AND parent_object_id = OBJECT_ID(N'[Sales].[SpecialOffer_ondisk]', 'U'))
ALTER TABLE [Sales].[SpecialOffer_ondisk] ADD CONSTRAINT [ODCK_SpecialOffer_MinQty] CHECK (([MinQty]>=(0)))
GO
IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE object_id = OBJECT_ID(N'[Sales].[ODCK_SpecialOffer_MaxQty]', 'C') AND parent_object_id = OBJECT_ID(N'[Sales].[SpecialOffer_ondisk]', 'U'))
ALTER TABLE [Sales].[SpecialOffer_ondisk] ADD CONSTRAINT [ODCK_SpecialOffer_MaxQty] CHECK (([MaxQty]>=(0)))
GO
IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE object_id = OBJECT_ID(N'[Sales].[ODCK_SpecialOffer_EndDate]', 'C') AND parent_object_id = OBJECT_ID(N'[Sales].[SpecialOffer_ondisk]', 'U'))
ALTER TABLE [Sales].[SpecialOffer_ondisk] ADD CONSTRAINT [ODCK_SpecialOffer_EndDate] CHECK (([EndDate]>=[StartDate]))
GO
PRINT N'Adding foreign keys to [HumanResources].[EmployeeDepartmentHistory]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[HumanResources].[FK_EmployeeDepartmentHistory_Department_DepartmentID]','F') AND parent_object_id = OBJECT_ID(N'[HumanResources].[EmployeeDepartmentHistory]', 'U'))
ALTER TABLE [HumanResources].[EmployeeDepartmentHistory] ADD CONSTRAINT [FK_EmployeeDepartmentHistory_Department_DepartmentID] FOREIGN KEY ([DepartmentID]) REFERENCES [HumanResources].[Department] ([DepartmentID])
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[HumanResources].[FK_EmployeeDepartmentHistory_Employee_BusinessEntityID]','F') AND parent_object_id = OBJECT_ID(N'[HumanResources].[EmployeeDepartmentHistory]', 'U'))
ALTER TABLE [HumanResources].[EmployeeDepartmentHistory] ADD CONSTRAINT [FK_EmployeeDepartmentHistory_Employee_BusinessEntityID] FOREIGN KEY ([BusinessEntityID]) REFERENCES [HumanResources].[Employee] ([BusinessEntityID])
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[HumanResources].[FK_EmployeeDepartmentHistory_Shift_ShiftID]','F') AND parent_object_id = OBJECT_ID(N'[HumanResources].[EmployeeDepartmentHistory]', 'U'))
ALTER TABLE [HumanResources].[EmployeeDepartmentHistory] ADD CONSTRAINT [FK_EmployeeDepartmentHistory_Shift_ShiftID] FOREIGN KEY ([ShiftID]) REFERENCES [HumanResources].[Shift] ([ShiftID])
GO
PRINT N'Adding foreign keys to [HumanResources].[EmployeePayHistory]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[HumanResources].[FK_EmployeePayHistory_Employee_BusinessEntityID]','F') AND parent_object_id = OBJECT_ID(N'[HumanResources].[EmployeePayHistory]', 'U'))
ALTER TABLE [HumanResources].[EmployeePayHistory] ADD CONSTRAINT [FK_EmployeePayHistory_Employee_BusinessEntityID] FOREIGN KEY ([BusinessEntityID]) REFERENCES [HumanResources].[Employee] ([BusinessEntityID])
GO
PRINT N'Adding foreign keys to [Production].[Document]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[Production].[FK_Document_Employee_Owner]','F') AND parent_object_id = OBJECT_ID(N'[Production].[Document]', 'U'))
ALTER TABLE [Production].[Document] ADD CONSTRAINT [FK_Document_Employee_Owner] FOREIGN KEY ([Owner]) REFERENCES [HumanResources].[Employee] ([BusinessEntityID])
GO
PRINT N'Adding foreign keys to [HumanResources].[Employee]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[HumanResources].[FK_Employee_Person_BusinessEntityID]','F') AND parent_object_id = OBJECT_ID(N'[HumanResources].[Employee]', 'U'))
ALTER TABLE [HumanResources].[Employee] ADD CONSTRAINT [FK_Employee_Person_BusinessEntityID] FOREIGN KEY ([BusinessEntityID]) REFERENCES [Person].[Person] ([BusinessEntityID])
GO
PRINT N'Adding foreign keys to [HumanResources].[JobCandidate]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[HumanResources].[FK_JobCandidate_Employee_BusinessEntityID]','F') AND parent_object_id = OBJECT_ID(N'[HumanResources].[JobCandidate]', 'U'))
ALTER TABLE [HumanResources].[JobCandidate] ADD CONSTRAINT [FK_JobCandidate_Employee_BusinessEntityID] FOREIGN KEY ([BusinessEntityID]) REFERENCES [HumanResources].[Employee] ([BusinessEntityID])
GO
PRINT N'Adding foreign keys to [Purchasing].[PurchaseOrderHeader]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[Purchasing].[FK_PurchaseOrderHeader_Employee_EmployeeID]','F') AND parent_object_id = OBJECT_ID(N'[Purchasing].[PurchaseOrderHeader]', 'U'))
ALTER TABLE [Purchasing].[PurchaseOrderHeader] ADD CONSTRAINT [FK_PurchaseOrderHeader_Employee_EmployeeID] FOREIGN KEY ([EmployeeID]) REFERENCES [HumanResources].[Employee] ([BusinessEntityID])
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[Purchasing].[FK_PurchaseOrderHeader_Vendor_VendorID]','F') AND parent_object_id = OBJECT_ID(N'[Purchasing].[PurchaseOrderHeader]', 'U'))
ALTER TABLE [Purchasing].[PurchaseOrderHeader] ADD CONSTRAINT [FK_PurchaseOrderHeader_Vendor_VendorID] FOREIGN KEY ([VendorID]) REFERENCES [Purchasing].[Vendor] ([BusinessEntityID])
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[Purchasing].[FK_PurchaseOrderHeader_ShipMethod_ShipMethodID]','F') AND parent_object_id = OBJECT_ID(N'[Purchasing].[PurchaseOrderHeader]', 'U'))
ALTER TABLE [Purchasing].[PurchaseOrderHeader] ADD CONSTRAINT [FK_PurchaseOrderHeader_ShipMethod_ShipMethodID] FOREIGN KEY ([ShipMethodID]) REFERENCES [Purchasing].[ShipMethod] ([ShipMethodID])
GO
PRINT N'Adding foreign keys to [Sales].[SalesPerson]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[Sales].[FK_SalesPerson_Employee_BusinessEntityID]','F') AND parent_object_id = OBJECT_ID(N'[Sales].[SalesPerson]', 'U'))
ALTER TABLE [Sales].[SalesPerson] ADD CONSTRAINT [FK_SalesPerson_Employee_BusinessEntityID] FOREIGN KEY ([BusinessEntityID]) REFERENCES [HumanResources].[Employee] ([BusinessEntityID])
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[Sales].[FK_SalesPerson_SalesTerritory_TerritoryID]','F') AND parent_object_id = OBJECT_ID(N'[Sales].[SalesPerson]', 'U'))
ALTER TABLE [Sales].[SalesPerson] ADD CONSTRAINT [FK_SalesPerson_SalesTerritory_TerritoryID] FOREIGN KEY ([TerritoryID]) REFERENCES [Sales].[SalesTerritory] ([TerritoryID])
GO
PRINT N'Adding foreign keys to [Person].[BusinessEntityAddress]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[Person].[FK_BusinessEntityAddress_AddressType_AddressTypeID]','F') AND parent_object_id = OBJECT_ID(N'[Person].[BusinessEntityAddress]', 'U'))
ALTER TABLE [Person].[BusinessEntityAddress] ADD CONSTRAINT [FK_BusinessEntityAddress_AddressType_AddressTypeID] FOREIGN KEY ([AddressTypeID]) REFERENCES [Person].[AddressType] ([AddressTypeID])
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[Person].[FK_BusinessEntityAddress_Address_AddressID]','F') AND parent_object_id = OBJECT_ID(N'[Person].[BusinessEntityAddress]', 'U'))
ALTER TABLE [Person].[BusinessEntityAddress] ADD CONSTRAINT [FK_BusinessEntityAddress_Address_AddressID] FOREIGN KEY ([AddressID]) REFERENCES [Person].[Address] ([AddressID])
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[Person].[FK_BusinessEntityAddress_BusinessEntity_BusinessEntityID]','F') AND parent_object_id = OBJECT_ID(N'[Person].[BusinessEntityAddress]', 'U'))
ALTER TABLE [Person].[BusinessEntityAddress] ADD CONSTRAINT [FK_BusinessEntityAddress_BusinessEntity_BusinessEntityID] FOREIGN KEY ([BusinessEntityID]) REFERENCES [Person].[BusinessEntity] ([BusinessEntityID])
GO
PRINT N'Adding foreign keys to [Sales].[SalesOrderHeader]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[Sales].[FK_SalesOrderHeader_Address_BillToAddressID]','F') AND parent_object_id = OBJECT_ID(N'[Sales].[SalesOrderHeader]', 'U'))
ALTER TABLE [Sales].[SalesOrderHeader] ADD CONSTRAINT [FK_SalesOrderHeader_Address_BillToAddressID] FOREIGN KEY ([BillToAddressID]) REFERENCES [Person].[Address] ([AddressID])
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[Sales].[FK_SalesOrderHeader_Address_ShipToAddressID]','F') AND parent_object_id = OBJECT_ID(N'[Sales].[SalesOrderHeader]', 'U'))
ALTER TABLE [Sales].[SalesOrderHeader] ADD CONSTRAINT [FK_SalesOrderHeader_Address_ShipToAddressID] FOREIGN KEY ([ShipToAddressID]) REFERENCES [Person].[Address] ([AddressID])
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[Sales].[FK_SalesOrderHeader_ShipMethod_ShipMethodID]','F') AND parent_object_id = OBJECT_ID(N'[Sales].[SalesOrderHeader]', 'U'))
ALTER TABLE [Sales].[SalesOrderHeader] ADD CONSTRAINT [FK_SalesOrderHeader_ShipMethod_ShipMethodID] FOREIGN KEY ([ShipMethodID]) REFERENCES [Purchasing].[ShipMethod] ([ShipMethodID])
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[Sales].[FK_SalesOrderHeader_CreditCard_CreditCardID]','F') AND parent_object_id = OBJECT_ID(N'[Sales].[SalesOrderHeader]', 'U'))
ALTER TABLE [Sales].[SalesOrderHeader] ADD CONSTRAINT [FK_SalesOrderHeader_CreditCard_CreditCardID] FOREIGN KEY ([CreditCardID]) REFERENCES [Sales].[CreditCard] ([CreditCardID])
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[Sales].[FK_SalesOrderHeader_CurrencyRate_CurrencyRateID]','F') AND parent_object_id = OBJECT_ID(N'[Sales].[SalesOrderHeader]', 'U'))
ALTER TABLE [Sales].[SalesOrderHeader] ADD CONSTRAINT [FK_SalesOrderHeader_CurrencyRate_CurrencyRateID] FOREIGN KEY ([CurrencyRateID]) REFERENCES [Sales].[CurrencyRate] ([CurrencyRateID])
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[Sales].[FK_SalesOrderHeader_Customer_CustomerID]','F') AND parent_object_id = OBJECT_ID(N'[Sales].[SalesOrderHeader]', 'U'))
ALTER TABLE [Sales].[SalesOrderHeader] ADD CONSTRAINT [FK_SalesOrderHeader_Customer_CustomerID] FOREIGN KEY ([CustomerID]) REFERENCES [Sales].[Customer] ([CustomerID])
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[Sales].[FK_SalesOrderHeader_SalesPerson_SalesPersonID]','F') AND parent_object_id = OBJECT_ID(N'[Sales].[SalesOrderHeader]', 'U'))
ALTER TABLE [Sales].[SalesOrderHeader] ADD CONSTRAINT [FK_SalesOrderHeader_SalesPerson_SalesPersonID] FOREIGN KEY ([SalesPersonID]) REFERENCES [Sales].[SalesPerson] ([BusinessEntityID])
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[Sales].[FK_SalesOrderHeader_SalesTerritory_TerritoryID]','F') AND parent_object_id = OBJECT_ID(N'[Sales].[SalesOrderHeader]', 'U'))
ALTER TABLE [Sales].[SalesOrderHeader] ADD CONSTRAINT [FK_SalesOrderHeader_SalesTerritory_TerritoryID] FOREIGN KEY ([TerritoryID]) REFERENCES [Sales].[SalesTerritory] ([TerritoryID])
GO
PRINT N'Adding foreign keys to [Person].[Address]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[Person].[FK_Address_StateProvince_StateProvinceID]','F') AND parent_object_id = OBJECT_ID(N'[Person].[Address]', 'U'))
ALTER TABLE [Person].[Address] ADD CONSTRAINT [FK_Address_StateProvince_StateProvinceID] FOREIGN KEY ([StateProvinceID]) REFERENCES [Person].[StateProvince] ([StateProvinceID])
GO
PRINT N'Adding foreign keys to [Person].[BusinessEntityContact]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[Person].[FK_BusinessEntityContact_BusinessEntity_BusinessEntityID]','F') AND parent_object_id = OBJECT_ID(N'[Person].[BusinessEntityContact]', 'U'))
ALTER TABLE [Person].[BusinessEntityContact] ADD CONSTRAINT [FK_BusinessEntityContact_BusinessEntity_BusinessEntityID] FOREIGN KEY ([BusinessEntityID]) REFERENCES [Person].[BusinessEntity] ([BusinessEntityID])
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[Person].[FK_BusinessEntityContact_Person_PersonID]','F') AND parent_object_id = OBJECT_ID(N'[Person].[BusinessEntityContact]', 'U'))
ALTER TABLE [Person].[BusinessEntityContact] ADD CONSTRAINT [FK_BusinessEntityContact_Person_PersonID] FOREIGN KEY ([PersonID]) REFERENCES [Person].[Person] ([BusinessEntityID])
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[Person].[FK_BusinessEntityContact_ContactType_ContactTypeID]','F') AND parent_object_id = OBJECT_ID(N'[Person].[BusinessEntityContact]', 'U'))
ALTER TABLE [Person].[BusinessEntityContact] ADD CONSTRAINT [FK_BusinessEntityContact_ContactType_ContactTypeID] FOREIGN KEY ([ContactTypeID]) REFERENCES [Person].[ContactType] ([ContactTypeID])
GO
PRINT N'Adding foreign keys to [Person].[Person]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[Person].[FK_Person_BusinessEntity_BusinessEntityID]','F') AND parent_object_id = OBJECT_ID(N'[Person].[Person]', 'U'))
ALTER TABLE [Person].[Person] ADD CONSTRAINT [FK_Person_BusinessEntity_BusinessEntityID] FOREIGN KEY ([BusinessEntityID]) REFERENCES [Person].[BusinessEntity] ([BusinessEntityID])
GO
PRINT N'Adding foreign keys to [Sales].[Store]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[Sales].[FK_Store_BusinessEntity_BusinessEntityID]','F') AND parent_object_id = OBJECT_ID(N'[Sales].[Store]', 'U'))
ALTER TABLE [Sales].[Store] ADD CONSTRAINT [FK_Store_BusinessEntity_BusinessEntityID] FOREIGN KEY ([BusinessEntityID]) REFERENCES [Person].[BusinessEntity] ([BusinessEntityID])
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[Sales].[FK_Store_SalesPerson_SalesPersonID]','F') AND parent_object_id = OBJECT_ID(N'[Sales].[Store]', 'U'))
ALTER TABLE [Sales].[Store] ADD CONSTRAINT [FK_Store_SalesPerson_SalesPersonID] FOREIGN KEY ([SalesPersonID]) REFERENCES [Sales].[SalesPerson] ([BusinessEntityID])
GO
PRINT N'Adding foreign keys to [Purchasing].[Vendor]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[Purchasing].[FK_Vendor_BusinessEntity_BusinessEntityID]','F') AND parent_object_id = OBJECT_ID(N'[Purchasing].[Vendor]', 'U'))
ALTER TABLE [Purchasing].[Vendor] ADD CONSTRAINT [FK_Vendor_BusinessEntity_BusinessEntityID] FOREIGN KEY ([BusinessEntityID]) REFERENCES [Person].[BusinessEntity] ([BusinessEntityID])
GO
PRINT N'Adding foreign keys to [Sales].[CountryRegionCurrency]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[Sales].[FK_CountryRegionCurrency_CountryRegion_CountryRegionCode]','F') AND parent_object_id = OBJECT_ID(N'[Sales].[CountryRegionCurrency]', 'U'))
ALTER TABLE [Sales].[CountryRegionCurrency] ADD CONSTRAINT [FK_CountryRegionCurrency_CountryRegion_CountryRegionCode] FOREIGN KEY ([CountryRegionCode]) REFERENCES [Person].[CountryRegion] ([CountryRegionCode])
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[Sales].[FK_CountryRegionCurrency_Currency_CurrencyCode]','F') AND parent_object_id = OBJECT_ID(N'[Sales].[CountryRegionCurrency]', 'U'))
ALTER TABLE [Sales].[CountryRegionCurrency] ADD CONSTRAINT [FK_CountryRegionCurrency_Currency_CurrencyCode] FOREIGN KEY ([CurrencyCode]) REFERENCES [Sales].[Currency] ([CurrencyCode])
GO
PRINT N'Adding foreign keys to [Sales].[SalesTerritory]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[Sales].[FK_SalesTerritory_CountryRegion_CountryRegionCode]','F') AND parent_object_id = OBJECT_ID(N'[Sales].[SalesTerritory]', 'U'))
ALTER TABLE [Sales].[SalesTerritory] ADD CONSTRAINT [FK_SalesTerritory_CountryRegion_CountryRegionCode] FOREIGN KEY ([CountryRegionCode]) REFERENCES [Person].[CountryRegion] ([CountryRegionCode])
GO
PRINT N'Adding foreign keys to [Person].[StateProvince]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[Person].[FK_StateProvince_CountryRegion_CountryRegionCode]','F') AND parent_object_id = OBJECT_ID(N'[Person].[StateProvince]', 'U'))
ALTER TABLE [Person].[StateProvince] ADD CONSTRAINT [FK_StateProvince_CountryRegion_CountryRegionCode] FOREIGN KEY ([CountryRegionCode]) REFERENCES [Person].[CountryRegion] ([CountryRegionCode])
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[Person].[FK_StateProvince_SalesTerritory_TerritoryID]','F') AND parent_object_id = OBJECT_ID(N'[Person].[StateProvince]', 'U'))
ALTER TABLE [Person].[StateProvince] ADD CONSTRAINT [FK_StateProvince_SalesTerritory_TerritoryID] FOREIGN KEY ([TerritoryID]) REFERENCES [Sales].[SalesTerritory] ([TerritoryID])
GO
PRINT N'Adding foreign keys to [Person].[EmailAddress]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[Person].[FK_EmailAddress_Person_BusinessEntityID]','F') AND parent_object_id = OBJECT_ID(N'[Person].[EmailAddress]', 'U'))
ALTER TABLE [Person].[EmailAddress] ADD CONSTRAINT [FK_EmailAddress_Person_BusinessEntityID] FOREIGN KEY ([BusinessEntityID]) REFERENCES [Person].[Person] ([BusinessEntityID])
GO
PRINT N'Adding foreign keys to [Person].[Password]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[Person].[FK_Password_Person_BusinessEntityID]','F') AND parent_object_id = OBJECT_ID(N'[Person].[Password]', 'U'))
ALTER TABLE [Person].[Password] ADD CONSTRAINT [FK_Password_Person_BusinessEntityID] FOREIGN KEY ([BusinessEntityID]) REFERENCES [Person].[Person] ([BusinessEntityID])
GO
PRINT N'Adding foreign keys to [Person].[PersonPhone]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[Person].[FK_PersonPhone_Person_BusinessEntityID]','F') AND parent_object_id = OBJECT_ID(N'[Person].[PersonPhone]', 'U'))
ALTER TABLE [Person].[PersonPhone] ADD CONSTRAINT [FK_PersonPhone_Person_BusinessEntityID] FOREIGN KEY ([BusinessEntityID]) REFERENCES [Person].[Person] ([BusinessEntityID])
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[Person].[FK_PersonPhone_PhoneNumberType_PhoneNumberTypeID]','F') AND parent_object_id = OBJECT_ID(N'[Person].[PersonPhone]', 'U'))
ALTER TABLE [Person].[PersonPhone] ADD CONSTRAINT [FK_PersonPhone_PhoneNumberType_PhoneNumberTypeID] FOREIGN KEY ([PhoneNumberTypeID]) REFERENCES [Person].[PhoneNumberType] ([PhoneNumberTypeID])
GO
PRINT N'Adding foreign keys to [Sales].[Customer]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[Sales].[FK_Customer_Person_PersonID]','F') AND parent_object_id = OBJECT_ID(N'[Sales].[Customer]', 'U'))
ALTER TABLE [Sales].[Customer] ADD CONSTRAINT [FK_Customer_Person_PersonID] FOREIGN KEY ([PersonID]) REFERENCES [Person].[Person] ([BusinessEntityID])
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[Sales].[FK_Customer_Store_StoreID]','F') AND parent_object_id = OBJECT_ID(N'[Sales].[Customer]', 'U'))
ALTER TABLE [Sales].[Customer] ADD CONSTRAINT [FK_Customer_Store_StoreID] FOREIGN KEY ([StoreID]) REFERENCES [Sales].[Store] ([BusinessEntityID])
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[Sales].[FK_Customer_SalesTerritory_TerritoryID]','F') AND parent_object_id = OBJECT_ID(N'[Sales].[Customer]', 'U'))
ALTER TABLE [Sales].[Customer] ADD CONSTRAINT [FK_Customer_SalesTerritory_TerritoryID] FOREIGN KEY ([TerritoryID]) REFERENCES [Sales].[SalesTerritory] ([TerritoryID])
GO
PRINT N'Adding foreign keys to [Sales].[PersonCreditCard]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[Sales].[FK_PersonCreditCard_Person_BusinessEntityID]','F') AND parent_object_id = OBJECT_ID(N'[Sales].[PersonCreditCard]', 'U'))
ALTER TABLE [Sales].[PersonCreditCard] ADD CONSTRAINT [FK_PersonCreditCard_Person_BusinessEntityID] FOREIGN KEY ([BusinessEntityID]) REFERENCES [Person].[Person] ([BusinessEntityID])
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[Sales].[FK_PersonCreditCard_CreditCard_CreditCardID]','F') AND parent_object_id = OBJECT_ID(N'[Sales].[PersonCreditCard]', 'U'))
ALTER TABLE [Sales].[PersonCreditCard] ADD CONSTRAINT [FK_PersonCreditCard_CreditCard_CreditCardID] FOREIGN KEY ([CreditCardID]) REFERENCES [Sales].[CreditCard] ([CreditCardID])
GO
PRINT N'Adding foreign keys to [Sales].[SalesTaxRate]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[Sales].[FK_SalesTaxRate_StateProvince_StateProvinceID]','F') AND parent_object_id = OBJECT_ID(N'[Sales].[SalesTaxRate]', 'U'))
ALTER TABLE [Sales].[SalesTaxRate] ADD CONSTRAINT [FK_SalesTaxRate_StateProvince_StateProvinceID] FOREIGN KEY ([StateProvinceID]) REFERENCES [Person].[StateProvince] ([StateProvinceID])
GO
PRINT N'Adding foreign keys to [Production].[BillOfMaterials]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[Production].[FK_BillOfMaterials_Product_ProductAssemblyID]','F') AND parent_object_id = OBJECT_ID(N'[Production].[BillOfMaterials]', 'U'))
ALTER TABLE [Production].[BillOfMaterials] ADD CONSTRAINT [FK_BillOfMaterials_Product_ProductAssemblyID] FOREIGN KEY ([ProductAssemblyID]) REFERENCES [Production].[Product] ([ProductID])
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[Production].[FK_BillOfMaterials_Product_ComponentID]','F') AND parent_object_id = OBJECT_ID(N'[Production].[BillOfMaterials]', 'U'))
ALTER TABLE [Production].[BillOfMaterials] ADD CONSTRAINT [FK_BillOfMaterials_Product_ComponentID] FOREIGN KEY ([ComponentID]) REFERENCES [Production].[Product] ([ProductID])
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[Production].[FK_BillOfMaterials_UnitMeasure_UnitMeasureCode]','F') AND parent_object_id = OBJECT_ID(N'[Production].[BillOfMaterials]', 'U'))
ALTER TABLE [Production].[BillOfMaterials] ADD CONSTRAINT [FK_BillOfMaterials_UnitMeasure_UnitMeasureCode] FOREIGN KEY ([UnitMeasureCode]) REFERENCES [Production].[UnitMeasure] ([UnitMeasureCode])
GO
PRINT N'Adding foreign keys to [Production].[ProductModelProductDescriptionCulture]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[Production].[FK_ProductModelProductDescriptionCulture_Culture_CultureID]','F') AND parent_object_id = OBJECT_ID(N'[Production].[ProductModelProductDescriptionCulture]', 'U'))
ALTER TABLE [Production].[ProductModelProductDescriptionCulture] ADD CONSTRAINT [FK_ProductModelProductDescriptionCulture_Culture_CultureID] FOREIGN KEY ([CultureID]) REFERENCES [Production].[Culture] ([CultureID])
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[Production].[FK_ProductModelProductDescriptionCulture_ProductDescription_ProductDescriptionID]','F') AND parent_object_id = OBJECT_ID(N'[Production].[ProductModelProductDescriptionCulture]', 'U'))
ALTER TABLE [Production].[ProductModelProductDescriptionCulture] ADD CONSTRAINT [FK_ProductModelProductDescriptionCulture_ProductDescription_ProductDescriptionID] FOREIGN KEY ([ProductDescriptionID]) REFERENCES [Production].[ProductDescription] ([ProductDescriptionID])
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[Production].[FK_ProductModelProductDescriptionCulture_ProductModel_ProductModelID]','F') AND parent_object_id = OBJECT_ID(N'[Production].[ProductModelProductDescriptionCulture]', 'U'))
ALTER TABLE [Production].[ProductModelProductDescriptionCulture] ADD CONSTRAINT [FK_ProductModelProductDescriptionCulture_ProductModel_ProductModelID] FOREIGN KEY ([ProductModelID]) REFERENCES [Production].[ProductModel] ([ProductModelID])
GO
PRINT N'Adding foreign keys to [Production].[ProductDocument]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[Production].[FK_ProductDocument_Document_DocumentNode]','F') AND parent_object_id = OBJECT_ID(N'[Production].[ProductDocument]', 'U'))
ALTER TABLE [Production].[ProductDocument] ADD CONSTRAINT [FK_ProductDocument_Document_DocumentNode] FOREIGN KEY ([DocumentNode]) REFERENCES [Production].[Document] ([DocumentNode])
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[Production].[FK_ProductDocument_Product_ProductID]','F') AND parent_object_id = OBJECT_ID(N'[Production].[ProductDocument]', 'U'))
ALTER TABLE [Production].[ProductDocument] ADD CONSTRAINT [FK_ProductDocument_Product_ProductID] FOREIGN KEY ([ProductID]) REFERENCES [Production].[Product] ([ProductID])
GO
PRINT N'Adding foreign keys to [Production].[ProductModelIllustration]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[Production].[FK_ProductModelIllustration_Illustration_IllustrationID]','F') AND parent_object_id = OBJECT_ID(N'[Production].[ProductModelIllustration]', 'U'))
ALTER TABLE [Production].[ProductModelIllustration] ADD CONSTRAINT [FK_ProductModelIllustration_Illustration_IllustrationID] FOREIGN KEY ([IllustrationID]) REFERENCES [Production].[Illustration] ([IllustrationID])
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[Production].[FK_ProductModelIllustration_ProductModel_ProductModelID]','F') AND parent_object_id = OBJECT_ID(N'[Production].[ProductModelIllustration]', 'U'))
ALTER TABLE [Production].[ProductModelIllustration] ADD CONSTRAINT [FK_ProductModelIllustration_ProductModel_ProductModelID] FOREIGN KEY ([ProductModelID]) REFERENCES [Production].[ProductModel] ([ProductModelID])
GO
PRINT N'Adding foreign keys to [Production].[ProductInventory]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[Production].[FK_ProductInventory_Location_LocationID]','F') AND parent_object_id = OBJECT_ID(N'[Production].[ProductInventory]', 'U'))
ALTER TABLE [Production].[ProductInventory] ADD CONSTRAINT [FK_ProductInventory_Location_LocationID] FOREIGN KEY ([LocationID]) REFERENCES [Production].[Location] ([LocationID])
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[Production].[FK_ProductInventory_Product_ProductID]','F') AND parent_object_id = OBJECT_ID(N'[Production].[ProductInventory]', 'U'))
ALTER TABLE [Production].[ProductInventory] ADD CONSTRAINT [FK_ProductInventory_Product_ProductID] FOREIGN KEY ([ProductID]) REFERENCES [Production].[Product] ([ProductID])
GO
PRINT N'Adding foreign keys to [Production].[WorkOrderRouting]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[Production].[FK_WorkOrderRouting_Location_LocationID]','F') AND parent_object_id = OBJECT_ID(N'[Production].[WorkOrderRouting]', 'U'))
ALTER TABLE [Production].[WorkOrderRouting] ADD CONSTRAINT [FK_WorkOrderRouting_Location_LocationID] FOREIGN KEY ([LocationID]) REFERENCES [Production].[Location] ([LocationID])
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[Production].[FK_WorkOrderRouting_WorkOrder_WorkOrderID]','F') AND parent_object_id = OBJECT_ID(N'[Production].[WorkOrderRouting]', 'U'))
ALTER TABLE [Production].[WorkOrderRouting] ADD CONSTRAINT [FK_WorkOrderRouting_WorkOrder_WorkOrderID] FOREIGN KEY ([WorkOrderID]) REFERENCES [Production].[WorkOrder] ([WorkOrderID])
GO
PRINT N'Adding foreign keys to [Production].[ProductSubcategory]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[Production].[FK_ProductSubcategory_ProductCategory_ProductCategoryID]','F') AND parent_object_id = OBJECT_ID(N'[Production].[ProductSubcategory]', 'U'))
ALTER TABLE [Production].[ProductSubcategory] ADD CONSTRAINT [FK_ProductSubcategory_ProductCategory_ProductCategoryID] FOREIGN KEY ([ProductCategoryID]) REFERENCES [Production].[ProductCategory] ([ProductCategoryID])
GO
PRINT N'Adding foreign keys to [Production].[ProductCostHistory]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[Production].[FK_ProductCostHistory_Product_ProductID]','F') AND parent_object_id = OBJECT_ID(N'[Production].[ProductCostHistory]', 'U'))
ALTER TABLE [Production].[ProductCostHistory] ADD CONSTRAINT [FK_ProductCostHistory_Product_ProductID] FOREIGN KEY ([ProductID]) REFERENCES [Production].[Product] ([ProductID])
GO
PRINT N'Adding foreign keys to [Production].[ProductListPriceHistory]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[Production].[FK_ProductListPriceHistory_Product_ProductID]','F') AND parent_object_id = OBJECT_ID(N'[Production].[ProductListPriceHistory]', 'U'))
ALTER TABLE [Production].[ProductListPriceHistory] ADD CONSTRAINT [FK_ProductListPriceHistory_Product_ProductID] FOREIGN KEY ([ProductID]) REFERENCES [Production].[Product] ([ProductID])
GO
PRINT N'Adding foreign keys to [Production].[Product]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[Production].[FK_Product_ProductModel_ProductModelID]','F') AND parent_object_id = OBJECT_ID(N'[Production].[Product]', 'U'))
ALTER TABLE [Production].[Product] ADD CONSTRAINT [FK_Product_ProductModel_ProductModelID] FOREIGN KEY ([ProductModelID]) REFERENCES [Production].[ProductModel] ([ProductModelID])
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[Production].[FK_Product_ProductSubcategory_ProductSubcategoryID]','F') AND parent_object_id = OBJECT_ID(N'[Production].[Product]', 'U'))
ALTER TABLE [Production].[Product] ADD CONSTRAINT [FK_Product_ProductSubcategory_ProductSubcategoryID] FOREIGN KEY ([ProductSubcategoryID]) REFERENCES [Production].[ProductSubcategory] ([ProductSubcategoryID])
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[Production].[FK_Product_UnitMeasure_SizeUnitMeasureCode]','F') AND parent_object_id = OBJECT_ID(N'[Production].[Product]', 'U'))
ALTER TABLE [Production].[Product] ADD CONSTRAINT [FK_Product_UnitMeasure_SizeUnitMeasureCode] FOREIGN KEY ([SizeUnitMeasureCode]) REFERENCES [Production].[UnitMeasure] ([UnitMeasureCode])
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[Production].[FK_Product_UnitMeasure_WeightUnitMeasureCode]','F') AND parent_object_id = OBJECT_ID(N'[Production].[Product]', 'U'))
ALTER TABLE [Production].[Product] ADD CONSTRAINT [FK_Product_UnitMeasure_WeightUnitMeasureCode] FOREIGN KEY ([WeightUnitMeasureCode]) REFERENCES [Production].[UnitMeasure] ([UnitMeasureCode])
GO
PRINT N'Adding foreign keys to [Production].[ProductProductPhoto]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[Production].[FK_ProductProductPhoto_ProductPhoto_ProductPhotoID]','F') AND parent_object_id = OBJECT_ID(N'[Production].[ProductProductPhoto]', 'U'))
ALTER TABLE [Production].[ProductProductPhoto] ADD CONSTRAINT [FK_ProductProductPhoto_ProductPhoto_ProductPhotoID] FOREIGN KEY ([ProductPhotoID]) REFERENCES [Production].[ProductPhoto] ([ProductPhotoID])
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[Production].[FK_ProductProductPhoto_Product_ProductID]','F') AND parent_object_id = OBJECT_ID(N'[Production].[ProductProductPhoto]', 'U'))
ALTER TABLE [Production].[ProductProductPhoto] ADD CONSTRAINT [FK_ProductProductPhoto_Product_ProductID] FOREIGN KEY ([ProductID]) REFERENCES [Production].[Product] ([ProductID])
GO
PRINT N'Adding foreign keys to [Production].[ProductReview]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[Production].[FK_ProductReview_Product_ProductID]','F') AND parent_object_id = OBJECT_ID(N'[Production].[ProductReview]', 'U'))
ALTER TABLE [Production].[ProductReview] ADD CONSTRAINT [FK_ProductReview_Product_ProductID] FOREIGN KEY ([ProductID]) REFERENCES [Production].[Product] ([ProductID])
GO
PRINT N'Adding foreign keys to [Purchasing].[ProductVendor]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[Purchasing].[FK_ProductVendor_Product_ProductID]','F') AND parent_object_id = OBJECT_ID(N'[Purchasing].[ProductVendor]', 'U'))
ALTER TABLE [Purchasing].[ProductVendor] ADD CONSTRAINT [FK_ProductVendor_Product_ProductID] FOREIGN KEY ([ProductID]) REFERENCES [Production].[Product] ([ProductID])
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[Purchasing].[FK_ProductVendor_UnitMeasure_UnitMeasureCode]','F') AND parent_object_id = OBJECT_ID(N'[Purchasing].[ProductVendor]', 'U'))
ALTER TABLE [Purchasing].[ProductVendor] ADD CONSTRAINT [FK_ProductVendor_UnitMeasure_UnitMeasureCode] FOREIGN KEY ([UnitMeasureCode]) REFERENCES [Production].[UnitMeasure] ([UnitMeasureCode])
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[Purchasing].[FK_ProductVendor_Vendor_BusinessEntityID]','F') AND parent_object_id = OBJECT_ID(N'[Purchasing].[ProductVendor]', 'U'))
ALTER TABLE [Purchasing].[ProductVendor] ADD CONSTRAINT [FK_ProductVendor_Vendor_BusinessEntityID] FOREIGN KEY ([BusinessEntityID]) REFERENCES [Purchasing].[Vendor] ([BusinessEntityID])
GO
PRINT N'Adding foreign keys to [Purchasing].[PurchaseOrderDetail]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[Purchasing].[FK_PurchaseOrderDetail_Product_ProductID]','F') AND parent_object_id = OBJECT_ID(N'[Purchasing].[PurchaseOrderDetail]', 'U'))
ALTER TABLE [Purchasing].[PurchaseOrderDetail] ADD CONSTRAINT [FK_PurchaseOrderDetail_Product_ProductID] FOREIGN KEY ([ProductID]) REFERENCES [Production].[Product] ([ProductID])
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[Purchasing].[FK_PurchaseOrderDetail_PurchaseOrderHeader_PurchaseOrderID]','F') AND parent_object_id = OBJECT_ID(N'[Purchasing].[PurchaseOrderDetail]', 'U'))
ALTER TABLE [Purchasing].[PurchaseOrderDetail] ADD CONSTRAINT [FK_PurchaseOrderDetail_PurchaseOrderHeader_PurchaseOrderID] FOREIGN KEY ([PurchaseOrderID]) REFERENCES [Purchasing].[PurchaseOrderHeader] ([PurchaseOrderID])
GO
PRINT N'Adding foreign keys to [Sales].[ShoppingCartItem]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[Sales].[FK_ShoppingCartItem_Product_ProductID]','F') AND parent_object_id = OBJECT_ID(N'[Sales].[ShoppingCartItem]', 'U'))
ALTER TABLE [Sales].[ShoppingCartItem] ADD CONSTRAINT [FK_ShoppingCartItem_Product_ProductID] FOREIGN KEY ([ProductID]) REFERENCES [Production].[Product] ([ProductID])
GO
PRINT N'Adding foreign keys to [Sales].[SpecialOfferProduct]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[Sales].[FK_SpecialOfferProduct_Product_ProductID]','F') AND parent_object_id = OBJECT_ID(N'[Sales].[SpecialOfferProduct]', 'U'))
ALTER TABLE [Sales].[SpecialOfferProduct] ADD CONSTRAINT [FK_SpecialOfferProduct_Product_ProductID] FOREIGN KEY ([ProductID]) REFERENCES [Production].[Product] ([ProductID])
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[Sales].[FK_SpecialOfferProduct_SpecialOffer_SpecialOfferID]','F') AND parent_object_id = OBJECT_ID(N'[Sales].[SpecialOfferProduct]', 'U'))
ALTER TABLE [Sales].[SpecialOfferProduct] ADD CONSTRAINT [FK_SpecialOfferProduct_SpecialOffer_SpecialOfferID] FOREIGN KEY ([SpecialOfferID]) REFERENCES [Sales].[SpecialOffer] ([SpecialOfferID])
GO
PRINT N'Adding foreign keys to [Production].[TransactionHistory]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[Production].[FK_TransactionHistory_Product_ProductID]','F') AND parent_object_id = OBJECT_ID(N'[Production].[TransactionHistory]', 'U'))
ALTER TABLE [Production].[TransactionHistory] ADD CONSTRAINT [FK_TransactionHistory_Product_ProductID] FOREIGN KEY ([ProductID]) REFERENCES [Production].[Product] ([ProductID])
GO
PRINT N'Adding foreign keys to [Production].[WorkOrder]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[Production].[FK_WorkOrder_Product_ProductID]','F') AND parent_object_id = OBJECT_ID(N'[Production].[WorkOrder]', 'U'))
ALTER TABLE [Production].[WorkOrder] ADD CONSTRAINT [FK_WorkOrder_Product_ProductID] FOREIGN KEY ([ProductID]) REFERENCES [Production].[Product] ([ProductID])
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[Production].[FK_WorkOrder_ScrapReason_ScrapReasonID]','F') AND parent_object_id = OBJECT_ID(N'[Production].[WorkOrder]', 'U'))
ALTER TABLE [Production].[WorkOrder] ADD CONSTRAINT [FK_WorkOrder_ScrapReason_ScrapReasonID] FOREIGN KEY ([ScrapReasonID]) REFERENCES [Production].[ScrapReason] ([ScrapReasonID])
GO
PRINT N'Adding foreign keys to [Sales].[SpecialOfferProduct_inmem]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[Sales].[IMFK_SpecialOfferProduct_Product_ProductID]','F') AND parent_object_id = OBJECT_ID(N'[Sales].[SpecialOfferProduct_inmem]', 'U'))
ALTER TABLE [Sales].[SpecialOfferProduct_inmem] ADD CONSTRAINT [IMFK_SpecialOfferProduct_Product_ProductID] FOREIGN KEY ([ProductID]) REFERENCES [Production].[Product_inmem] ([ProductID])
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[Sales].[IMFK_SpecialOfferProduct_SpecialOffer_SpecialOfferID]','F') AND parent_object_id = OBJECT_ID(N'[Sales].[SpecialOfferProduct_inmem]', 'U'))
ALTER TABLE [Sales].[SpecialOfferProduct_inmem] ADD CONSTRAINT [IMFK_SpecialOfferProduct_SpecialOffer_SpecialOfferID] FOREIGN KEY ([SpecialOfferID]) REFERENCES [Sales].[SpecialOffer_inmem] ([SpecialOfferID])
GO
PRINT N'Adding foreign keys to [Sales].[SpecialOfferProduct_ondisk]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[Sales].[ODFK_SpecialOfferProduct_Product_ProductID]','F') AND parent_object_id = OBJECT_ID(N'[Sales].[SpecialOfferProduct_ondisk]', 'U'))
ALTER TABLE [Sales].[SpecialOfferProduct_ondisk] ADD CONSTRAINT [ODFK_SpecialOfferProduct_Product_ProductID] FOREIGN KEY ([ProductID]) REFERENCES [Production].[Product_ondisk] ([ProductID])
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[Sales].[ODFK_SpecialOfferProduct_SpecialOffer_SpecialOfferID]','F') AND parent_object_id = OBJECT_ID(N'[Sales].[SpecialOfferProduct_ondisk]', 'U'))
ALTER TABLE [Sales].[SpecialOfferProduct_ondisk] ADD CONSTRAINT [ODFK_SpecialOfferProduct_SpecialOffer_SpecialOfferID] FOREIGN KEY ([SpecialOfferID]) REFERENCES [Sales].[SpecialOffer_ondisk] ([SpecialOfferID])
GO
PRINT N'Adding foreign keys to [Sales].[CurrencyRate]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[Sales].[FK_CurrencyRate_Currency_FromCurrencyCode]','F') AND parent_object_id = OBJECT_ID(N'[Sales].[CurrencyRate]', 'U'))
ALTER TABLE [Sales].[CurrencyRate] ADD CONSTRAINT [FK_CurrencyRate_Currency_FromCurrencyCode] FOREIGN KEY ([FromCurrencyCode]) REFERENCES [Sales].[Currency] ([CurrencyCode])
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[Sales].[FK_CurrencyRate_Currency_ToCurrencyCode]','F') AND parent_object_id = OBJECT_ID(N'[Sales].[CurrencyRate]', 'U'))
ALTER TABLE [Sales].[CurrencyRate] ADD CONSTRAINT [FK_CurrencyRate_Currency_ToCurrencyCode] FOREIGN KEY ([ToCurrencyCode]) REFERENCES [Sales].[Currency] ([CurrencyCode])
GO
PRINT N'Adding foreign keys to [Sales].[SalesOrderDetail]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[Sales].[FK_SalesOrderDetail_SalesOrderHeader_SalesOrderID]','F') AND parent_object_id = OBJECT_ID(N'[Sales].[SalesOrderDetail]', 'U'))
ALTER TABLE [Sales].[SalesOrderDetail] ADD CONSTRAINT [FK_SalesOrderDetail_SalesOrderHeader_SalesOrderID] FOREIGN KEY ([SalesOrderID]) REFERENCES [Sales].[SalesOrderHeader] ([SalesOrderID]) ON DELETE CASCADE
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[Sales].[FK_SalesOrderDetail_SpecialOfferProduct_SpecialOfferIDProductID]','F') AND parent_object_id = OBJECT_ID(N'[Sales].[SalesOrderDetail]', 'U'))
ALTER TABLE [Sales].[SalesOrderDetail] ADD CONSTRAINT [FK_SalesOrderDetail_SpecialOfferProduct_SpecialOfferIDProductID] FOREIGN KEY ([SpecialOfferID], [ProductID]) REFERENCES [Sales].[SpecialOfferProduct] ([SpecialOfferID], [ProductID])
GO
PRINT N'Adding foreign keys to [Sales].[SalesOrderDetail_inmem]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[Sales].[IMFK_SalesOrderDetail_SalesOrderHeader_SalesOrderID]','F') AND parent_object_id = OBJECT_ID(N'[Sales].[SalesOrderDetail_inmem]', 'U'))
ALTER TABLE [Sales].[SalesOrderDetail_inmem] ADD CONSTRAINT [IMFK_SalesOrderDetail_SalesOrderHeader_SalesOrderID] FOREIGN KEY ([SalesOrderID]) REFERENCES [Sales].[SalesOrderHeader_inmem] ([SalesOrderID])
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[Sales].[IMFK_SalesOrderDetail_SpecialOfferProduct_SpecialOfferIDProductID]','F') AND parent_object_id = OBJECT_ID(N'[Sales].[SalesOrderDetail_inmem]', 'U'))
ALTER TABLE [Sales].[SalesOrderDetail_inmem] ADD CONSTRAINT [IMFK_SalesOrderDetail_SpecialOfferProduct_SpecialOfferIDProductID] FOREIGN KEY ([SpecialOfferID], [ProductID]) REFERENCES [Sales].[SpecialOfferProduct_inmem] ([SpecialOfferID], [ProductID])
GO
PRINT N'Adding foreign keys to [Sales].[SalesOrderDetail_ondisk]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[Sales].[ODFK_SalesOrderDetail_SalesOrderHeader_SalesOrderID]','F') AND parent_object_id = OBJECT_ID(N'[Sales].[SalesOrderDetail_ondisk]', 'U'))
ALTER TABLE [Sales].[SalesOrderDetail_ondisk] ADD CONSTRAINT [ODFK_SalesOrderDetail_SalesOrderHeader_SalesOrderID] FOREIGN KEY ([SalesOrderID]) REFERENCES [Sales].[SalesOrderHeader_ondisk] ([SalesOrderID])
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[Sales].[ODFK_SalesOrderDetail_SpecialOfferProduct_SpecialOfferIDProductID]','F') AND parent_object_id = OBJECT_ID(N'[Sales].[SalesOrderDetail_ondisk]', 'U'))
ALTER TABLE [Sales].[SalesOrderDetail_ondisk] ADD CONSTRAINT [ODFK_SalesOrderDetail_SpecialOfferProduct_SpecialOfferIDProductID] FOREIGN KEY ([SpecialOfferID], [ProductID]) REFERENCES [Sales].[SpecialOfferProduct_ondisk] ([SpecialOfferID], [ProductID])
GO
PRINT N'Adding foreign keys to [Sales].[SalesOrderHeaderSalesReason]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[Sales].[FK_SalesOrderHeaderSalesReason_SalesOrderHeader_SalesOrderID]','F') AND parent_object_id = OBJECT_ID(N'[Sales].[SalesOrderHeaderSalesReason]', 'U'))
ALTER TABLE [Sales].[SalesOrderHeaderSalesReason] ADD CONSTRAINT [FK_SalesOrderHeaderSalesReason_SalesOrderHeader_SalesOrderID] FOREIGN KEY ([SalesOrderID]) REFERENCES [Sales].[SalesOrderHeader] ([SalesOrderID]) ON DELETE CASCADE
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[Sales].[FK_SalesOrderHeaderSalesReason_SalesReason_SalesReasonID]','F') AND parent_object_id = OBJECT_ID(N'[Sales].[SalesOrderHeaderSalesReason]', 'U'))
ALTER TABLE [Sales].[SalesOrderHeaderSalesReason] ADD CONSTRAINT [FK_SalesOrderHeaderSalesReason_SalesReason_SalesReasonID] FOREIGN KEY ([SalesReasonID]) REFERENCES [Sales].[SalesReason] ([SalesReasonID])
GO
PRINT N'Adding foreign keys to [Sales].[SalesPersonQuotaHistory]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[Sales].[FK_SalesPersonQuotaHistory_SalesPerson_BusinessEntityID]','F') AND parent_object_id = OBJECT_ID(N'[Sales].[SalesPersonQuotaHistory]', 'U'))
ALTER TABLE [Sales].[SalesPersonQuotaHistory] ADD CONSTRAINT [FK_SalesPersonQuotaHistory_SalesPerson_BusinessEntityID] FOREIGN KEY ([BusinessEntityID]) REFERENCES [Sales].[SalesPerson] ([BusinessEntityID])
GO
PRINT N'Adding foreign keys to [Sales].[SalesTerritoryHistory]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[Sales].[FK_SalesTerritoryHistory_SalesPerson_BusinessEntityID]','F') AND parent_object_id = OBJECT_ID(N'[Sales].[SalesTerritoryHistory]', 'U'))
ALTER TABLE [Sales].[SalesTerritoryHistory] ADD CONSTRAINT [FK_SalesTerritoryHistory_SalesPerson_BusinessEntityID] FOREIGN KEY ([BusinessEntityID]) REFERENCES [Sales].[SalesPerson] ([BusinessEntityID])
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[Sales].[FK_SalesTerritoryHistory_SalesTerritory_TerritoryID]','F') AND parent_object_id = OBJECT_ID(N'[Sales].[SalesTerritoryHistory]', 'U'))
ALTER TABLE [Sales].[SalesTerritoryHistory] ADD CONSTRAINT [FK_SalesTerritoryHistory_SalesTerritory_TerritoryID] FOREIGN KEY ([TerritoryID]) REFERENCES [Sales].[SalesTerritory] ([TerritoryID])
GO
PRINT N'Creating DDL triggers'
GO
IF NOT EXISTS (SELECT 1 FROM sys.triggers WHERE name = N'ddlDatabaseTriggerLog' AND parent_class = 0)
EXEC sp_executesql N'CREATE TRIGGER [ddlDatabaseTriggerLog] ON DATABASE 
FOR DDL_DATABASE_LEVEL_EVENTS AS 
BEGIN
    SET NOCOUNT ON;

    DECLARE @data XML;
    DECLARE @schema sysname;
    DECLARE @object sysname;
    DECLARE @eventType sysname;

    SET @data = EVENTDATA();
    SET @eventType = @data.value(''(/EVENT_INSTANCE/EventType)[1]'', ''sysname'');
    SET @schema = @data.value(''(/EVENT_INSTANCE/SchemaName)[1]'', ''sysname'');
    SET @object = @data.value(''(/EVENT_INSTANCE/ObjectName)[1]'', ''sysname'') 

    IF @object IS NOT NULL
        PRINT ''  '' + @eventType + '' - '' + @schema + ''.'' + @object;
    ELSE
        PRINT ''  '' + @eventType + '' - '' + @schema;

    IF @eventType IS NULL
        PRINT CONVERT(nvarchar(max), @data);

    INSERT [dbo].[DatabaseLog] 
        (
        [PostTime], 
        [DatabaseUser], 
        [Event], 
        [Schema], 
        [Object], 
        [TSQL], 
        [XmlEvent]
        ) 
    VALUES 
        (
        GETDATE(), 
        CONVERT(sysname, CURRENT_USER), 
        @eventType, 
        CONVERT(sysname, @schema), 
        CONVERT(sysname, @object), 
        @data.value(''(/EVENT_INSTANCE/TSQLCommand)[1]'', ''nvarchar(max)''), 
        @data
        );
END;
'
GO
DISABLE TRIGGER ddlDatabaseTriggerLog ON DATABASE
GO
