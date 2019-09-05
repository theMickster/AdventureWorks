IF OBJECT_ID('[Sales].[usp_InsertSalesOrder_ondisk]') IS NOT NULL
	DROP PROCEDURE [Sales].[usp_InsertSalesOrder_ondisk];

GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_NULLS ON
GO
CREATE PROCEDURE [Sales].[usp_InsertSalesOrder_ondisk]
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
GO
