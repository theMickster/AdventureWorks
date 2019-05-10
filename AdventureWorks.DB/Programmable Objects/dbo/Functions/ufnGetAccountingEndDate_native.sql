-- <Migration TransactionHandling="Custom" />
-- <Migration ID="3b832b1b-d204-4e23-a614-5ed9f277e801" />
GO
CREATE OR ALTER FUNCTION dbo.ufnGetAccountingEndDate_native()
RETURNS [datetime] 
WITH NATIVE_COMPILATION, SCHEMABINDING
AS 
BEGIN ATOMIC WITH (TRANSACTION ISOLATION LEVEL=SNAPSHOT, LANGUAGE=N'us_english')

    RETURN DATEADD(millisecond, -2, CONVERT(datetime, '20160701', 112));

END
GO
