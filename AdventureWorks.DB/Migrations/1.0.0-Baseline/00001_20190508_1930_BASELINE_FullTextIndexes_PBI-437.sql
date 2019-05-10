-- <Migration ID="e35283e9-e52d-4be6-a361-4f81a6b837c0" />
GO
/****************************************************************************************************************
** CREATED BY:   Mick Letofsky
** CREATED DATE: 2019.05.08
** CREATED FOR:  PBI 437
** CREATED:      Baseline code for AdventureWorks SQL Change Automation Project.
****************************************************************************************************************/			
PRINT 'This code must be deployed manually...'
/*
IF NOT EXISTS (SELECT 1 FROM sys.fulltext_catalogs WHERE name = N'AW2014FullTextCatalog')
BEGIN
	CREATE FULLTEXT CATALOG [AW2014FullTextCatalog]
	WITH ACCENT_SENSITIVITY = ON
	AUTHORIZATION [dbo]
END

IF NOT EXISTS (SELECT 1 FROM sys.fulltext_catalogs WHERE name = N'AdventureWorksFullTextCatalog')
BEGIN
	CREATE FULLTEXT CATALOG [AdventureWorksFullTextCatalog]
	WITH ACCENT_SENSITIVITY = ON
	AS DEFAULT
	AUTHORIZATION [dbo]
END
*/