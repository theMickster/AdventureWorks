IF OBJECT_ID('[dbo].[ufnGetAccountingEndDate]') IS NOT NULL
	DROP FUNCTION [dbo].[ufnGetAccountingEndDate];

GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_NULLS ON
GO

CREATE FUNCTION [dbo].[ufnGetAccountingEndDate]()
RETURNS [datetime] 
AS 
BEGIN
    RETURN DATEADD(millisecond, -2, CONVERT(datetime, '20040701', 112));
END;
GO
EXEC sp_addextendedproperty N'MS_Description', N'Scalar function used in the uSalesOrderHeader trigger to set the starting account date.', 'SCHEMA', N'dbo', 'FUNCTION', N'ufnGetAccountingEndDate', NULL, NULL
GO
