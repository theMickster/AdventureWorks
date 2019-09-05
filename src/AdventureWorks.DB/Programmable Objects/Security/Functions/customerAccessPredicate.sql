-- <Migration ID="964a55ea-5642-42fa-a7d7-f4a342d188e9" />
GO
DROP SECURITY POLICY IF EXISTS [Security].[customerPolicy]
GO
CREATE OR ALTER FUNCTION [Security].[customerAccessPredicate](@TerritoryID int)
	RETURNS TABLE
	WITH SCHEMABINDING
AS
	RETURN SELECT 1 AS accessResult
	FROM HumanResources.Employee e 
	INNER JOIN Sales.SalesPerson sp ON sp.BusinessEntityID = e.BusinessEntityID
	WHERE
		( RIGHT(e.LoginID, LEN(e.LoginID) - LEN('adventure-works\')) = USER_NAME() AND sp.TerritoryID = @TerritoryID ) 
		OR IS_MEMBER('SalesManagers') = 1
		OR IS_MEMBER('db_owner') = 1
GO
CREATE SECURITY POLICY [Security].[customerPolicy]
ADD FILTER PREDICATE [Security].[customerAccessPredicate]([TerritoryID])
ON [Sales].[CustomerPII],
ADD BLOCK PREDICATE [Security].[customerAccessPredicate]([TerritoryID])
ON [Sales].[CustomerPII] 
WITH (STATE = ON)
GO