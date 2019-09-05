IF OBJECT_ID('[Production].[vProductModelInstructions]') IS NOT NULL
	DROP VIEW [Production].[vProductModelInstructions];

GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_NULLS ON
GO

CREATE VIEW [Production].[vProductModelInstructions] 
AS 
SELECT 
    [ProductModelID] 
    ,[Name] 
    ,[Instructions].value(N'declare default element namespace "http://schemas.microsoft.com/sqlserver/2004/07/adventure-works/ProductModelManuInstructions"; 
        (/root/text())[1]', 'nvarchar(max)') AS [Instructions] 
    ,[MfgInstructions].ref.value('@LocationID[1]', 'int') AS [LocationID] 
    ,[MfgInstructions].ref.value('@SetupHours[1]', 'decimal(9, 4)') AS [SetupHours] 
    ,[MfgInstructions].ref.value('@MachineHours[1]', 'decimal(9, 4)') AS [MachineHours] 
    ,[MfgInstructions].ref.value('@LaborHours[1]', 'decimal(9, 4)') AS [LaborHours] 
    ,[MfgInstructions].ref.value('@LotSize[1]', 'int') AS [LotSize] 
    ,[Steps].ref.value('string(.)[1]', 'nvarchar(1024)') AS [Step] 
    ,[rowguid] 
    ,[ModifiedDate]
FROM [Production].[ProductModel] 
CROSS APPLY [Instructions].nodes(N'declare default element namespace "http://schemas.microsoft.com/sqlserver/2004/07/adventure-works/ProductModelManuInstructions"; 
    /root/Location') MfgInstructions(ref)
CROSS APPLY [MfgInstructions].ref.nodes('declare default element namespace "http://schemas.microsoft.com/sqlserver/2004/07/adventure-works/ProductModelManuInstructions"; 
    step') Steps(ref);
GO
EXEC sp_addextendedproperty N'MS_Description', N'Displays the content from each element in the xml column Instructions for each product in the Production.ProductModel table that has manufacturing instructions.', 'SCHEMA', N'Production', 'VIEW', N'vProductModelInstructions', NULL, NULL
GO
