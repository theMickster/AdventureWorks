IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_BusinessEntity_IsEntraUser_Entra')
BEGIN
    CREATE NONCLUSTERED INDEX IX_BusinessEntity_IsEntraUser_Entra
    ON Person.BusinessEntity(IsEntraUser)
    INCLUDE (BusinessEntityID, rowguid)
    WHERE IsEntraUser = 1;
END
