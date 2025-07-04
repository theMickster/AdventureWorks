GO
/*
CREATE OR ALTER FUNCTION dbo.ufnGetSalesOrderStatusText_native (@Status tinyint)
RETURNS nvarchar(15) 
WITH NATIVE_COMPILATION, SCHEMABINDING
AS
BEGIN ATOMIC WITH (TRANSACTION ISOLATION LEVEL = SNAPSHOT, LANGUAGE = N'English')

    IF @Status=1 RETURN 'In Process'
    IF @Status=2 RETURN 'Approved'
    IF @Status=3 RETURN 'Backordered'
    IF @Status=4 RETURN 'Rejected'
    IF @Status=5 RETURN 'Shipped'
    IF @Status=6 RETURN 'Cancelled'

    
    RETURN '** Invalid **'

END
GO
*/