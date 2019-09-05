-- <Migration ID="b67f41ef-f618-4e60-afb8-d6c6e8415657" />
-- <Migration TransactionHandling="Custom" />
GO
CREATE OR ALTER FUNCTION Sales.vSalesOrderDetail_extended_inmem ()
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
GO
