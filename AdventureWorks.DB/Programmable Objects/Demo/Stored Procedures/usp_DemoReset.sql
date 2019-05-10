IF OBJECT_ID('[Demo].[usp_DemoReset]') IS NOT NULL
	DROP PROCEDURE [Demo].[usp_DemoReset];

GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_NULLS ON
GO
CREATE PROCEDURE [Demo].[usp_DemoReset]
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
GO
