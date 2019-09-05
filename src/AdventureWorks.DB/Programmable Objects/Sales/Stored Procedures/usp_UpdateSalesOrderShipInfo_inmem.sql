IF OBJECT_ID('[Sales].[usp_UpdateSalesOrderShipInfo_inmem]') IS NOT NULL
	DROP PROCEDURE [Sales].[usp_UpdateSalesOrderShipInfo_inmem];

GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_NULLS ON
GO
-- wrapper stored procedure that contains retry logic to deal with update conflicts
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
GO
