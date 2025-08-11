// Provisions an Azure SQL Database on an existing SQL Server. Defaults to Basic 5 DTU tier.

@description('Name of the SQL Database')
param name string

@description('Name of the parent SQL Server')
param serverName string

@description('Azure region')
param location string

@description('SKU for the SQL Database')
param sku object = {
  name: 'Basic'
  tier: 'Basic'
  capacity: 5
}

@description('Resource tags')
param tags object

resource sqlServer 'Microsoft.Sql/servers@2023-08-01-preview' existing = {
  name: serverName
}

resource sqlDatabase 'Microsoft.Sql/servers/databases@2023-08-01-preview' = {
  parent: sqlServer
  name: name
  location: location
  tags: tags
  sku: sku
}

output id string = sqlDatabase.id
output name string = sqlDatabase.name
