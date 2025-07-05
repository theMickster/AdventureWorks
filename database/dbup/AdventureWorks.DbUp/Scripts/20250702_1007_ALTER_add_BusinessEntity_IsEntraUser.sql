IF COL_LENGTH('Person.BusinessEntity', 'IsEntraUser') IS NULL
BEGIN
    ALTER TABLE Person.BusinessEntity ADD IsEntraUser BIT NOT NULL DEFAULT 0;

	EXEC sys.sp_addextendedproperty
		@name = N'MS_Description',
		@value = N'Indicates if this BusinessEntity is mapped to a Microsoft Entra ID user account. When true, the entity can authenticate via API using rowguid as Entra ObjectId.',
		@level0type = N'SCHEMA', @level0name = 'Person',
		@level1type = N'TABLE',  @level1name = 'BusinessEntity',
		@level2type = N'COLUMN', @level2name = 'IsEntraUser';
END