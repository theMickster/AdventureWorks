// Provisions an Azure SQL Server with Entra ID admin and SQL auth. Includes AllowAzureServices firewall rule.

@description('Name of the SQL Server')
param name string

@description('Azure region')
param location string

@description('SQL admin login')
param adminLogin string

@secure()
@description('SQL admin password')
param adminPassword string

@description('Object ID of the Entra admin')
param entraAdminObjectId string

@description('Display name of the Entra admin')
param entraAdminDisplayName string

@description('Resource tags')
param tags object

resource sqlServer 'Microsoft.Sql/servers@2023-08-01-preview' = {
  name: name
  location: location
  tags: tags
  properties: {
    administratorLogin: adminLogin
    administratorLoginPassword: adminPassword
    administrators: {
      administratorType: 'ActiveDirectory'
      azureADOnlyAuthentication: false
      login: entraAdminDisplayName
      principalType: 'Group'
      sid: entraAdminObjectId
      tenantId: tenant().tenantId
    }
  }
}

// TODO: Replace with VNet service endpoints or private endpoints for production.
// AllowAzureServices (0.0.0.0) permits any Azure service in any subscription to reach this server.
// Acceptable for dev; must be tightened before production use.
resource firewallAllowAzure 'Microsoft.Sql/servers/firewallRules@2023-08-01-preview' = {
  parent: sqlServer
  name: 'AllowAzureServices'
  properties: {
    startIpAddress: '0.0.0.0'
    endIpAddress: '0.0.0.0'
  }
}

output id string = sqlServer.id
output name string = sqlServer.name
output fullyQualifiedDomainName string = sqlServer.properties.fullyQualifiedDomainName
