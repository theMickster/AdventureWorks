-- <Migration TransactionHandling="Custom" />
-- <Migration ID="dc2ffe87-aee7-4719-b794-a04ba0bc6557" />
GO
CREATE OR ALTER FUNCTION [dbo].[ufnGetAccountingStartDate_native]()
RETURNS [datetime] 
WITH NATIVE_COMPILATION, SCHEMABINDING
AS 
BEGIN ATOMIC WITH (TRANSACTION ISOLATION LEVEL=SNAPSHOT, LANGUAGE=N'us_english')

    RETURN CONVERT(datetime, '20150701', 112);

END
GO
