-- <Migration ID="95d17735-5e39-4c1c-90e5-cf2b6c891ae2" />
GO
/****************************************************************************************************************
** CREATED BY:   Mick Letofsky
** CREATED DATE: 2019.05.08
** CREATED FOR:  PBI 437
** CREATED:      Baseline code for AdventureWorks SQL Change Automation Project.
****************************************************************************************************************/

PRINT 'THERE BE DRAGONS.... Care will need to be taken when/if this programmable object must change.'

/*
IF OBJECT_ID('[dbo].[ufnLeadingZeros]') IS NOT NULL
BEGIN
    DROP INDEX Sales.Customer.AK_Customer_AccountNumber;
    ALTER TABLE Sales.Customer DROP COLUMN AccountNumber;
    DROP FUNCTION dbo.ufnLeadingZeros;
END
GO
CREATE FUNCTION dbo.ufnLeadingZeros(
    @Value int
)
RETURNS varchar(8)
WITH SCHEMABINDING
AS
BEGIN
    DECLARE @ReturnValue varchar(8);
 
    SET @ReturnValue = CONVERT(varchar(8), @Value);
    SET @ReturnValue = REPLICATE('0', 8 - DATALENGTH(@ReturnValue)) + @ReturnValue;
 
    RETURN (@ReturnValue);
END;
GO
 
ALTER TABLE Sales.Customer ADD AccountNumber AS (isnull('AW'+dbo.ufnLeadingZeros(CustomerID),''));
 
CREATE UNIQUE NONCLUSTERED INDEX AK_Customer_AccountNumber ON Sales.Customer (AccountNumber);
EXEC sp_addextendedproperty N'MS_Description', N'Scalar function used by the Sales.Customer table to help set the account number.', 'SCHEMA', N'dbo', 'FUNCTION', N'ufnLeadingZeros', NULL, NULL
EXEC sp_addextendedproperty N'MS_Description', N'Input parameter for the scalar function ufnLeadingZeros. Enter a valid integer.', 'SCHEMA', N'dbo', 'FUNCTION', N'ufnLeadingZeros', 'PARAMETER', N'@Value'
*/