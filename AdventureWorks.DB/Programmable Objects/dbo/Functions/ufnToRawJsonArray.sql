IF OBJECT_ID('[dbo].[ufnToRawJsonArray]') IS NOT NULL
	DROP FUNCTION [dbo].[ufnToRawJsonArray];

GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_NULLS ON
GO
CREATE FUNCTION
[dbo].[ufnToRawJsonArray](@json nvarchar(max), @key nvarchar(400)) returns nvarchar(max)
as begin
       declare @new nvarchar(max) = replace(@json, CONCAT('},{"', @key,'":'),',')
       return '[' + substring(@new, 1 + (LEN(@key)+5), LEN(@new) -2 - (LEN(@key)+5)) + ']'
end
GO
