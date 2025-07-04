
GO
/*
IF OBJECT_ID('[Sales].[usp_UpdateSalesOrderShipInfo_ondisk]') IS NOT NULL
	DROP PROCEDURE [Sales].[usp_UpdateSalesOrderShipInfo_ondisk];

GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_NULLS ON
GO
-- for simplicity, we assume all items in the order are shipped in the same package, and thus have the same carrier tracking number
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
GO
*/