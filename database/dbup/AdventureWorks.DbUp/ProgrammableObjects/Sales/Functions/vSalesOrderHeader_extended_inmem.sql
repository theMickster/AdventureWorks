GO
/*
CREATE OR ALTER FUNCTION Sales.vSalesOrderHeader_extended_inmem ()
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
		ISNULL(N'SO' + CONVERT([nvarchar](23), SalesOrderID), N'*** ERROR ***') AS SalesOrderNumber, 
		ISNULL(SubTotal + TaxAmt + Freight, 0) AS TotalDue
	FROM Sales.SalesOrderHeader_inmem
GO
*/