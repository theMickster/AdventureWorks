GO
/*
CREATE OR ALTER PROCEDURE [Sales].[usp_UpdateSalesOrderShipInfo_native]
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
   LANGUAGE = N'us_english')

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
GO
*/