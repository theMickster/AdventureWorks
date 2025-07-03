IF OBJECT_ID('[Demo].[usp_DemoInitSeed]') IS NOT NULL
	DROP PROCEDURE [Demo].[usp_DemoInitSeed];

GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_NULLS ON
GO
CREATE PROCEDURE [Demo].[usp_DemoInitSeed] @items_per_order int = 5
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
GO
