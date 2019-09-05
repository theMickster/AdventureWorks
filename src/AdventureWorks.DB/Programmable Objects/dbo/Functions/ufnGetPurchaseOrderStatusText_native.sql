-- <Migration TransactionHandling="Custom" />
-- <Migration ID="268c859b-f5e2-42c8-8bce-f3e3a3c57d02" />
GO
CREATE OR ALTER FUNCTION [dbo].[ufnGetPurchaseOrderStatusText_native] (@Status tinyint)
RETURNS nvarchar(15) 
WITH NATIVE_COMPILATION, SCHEMABINDING
AS
BEGIN ATOMIC WITH (TRANSACTION ISOLATION LEVEL = SNAPSHOT, LANGUAGE = N'English')

    IF @Status=1 RETURN 'Pending'
    IF @Status=2 RETURN 'Approved'
    IF @Status=3 RETURN 'Rejected'
    IF @Status=4 RETURN 'Complete'
    
    RETURN '** Invalid **'

END
GO
