// Provisions a Linux App Service with system-assigned managed identity, HTTPS-only, and security hardening.

@description('Name of the App Service')
param name string

@description('Azure region')
param location string

@description('Resource ID of the App Service Plan')
param appServicePlanId string

@description('Linux runtime stack (e.g., DOTNETCORE|10.0, NODE|24-lts)')
param runtimeStack string

type appSetting = {
  name: string
  value: string
}

@description('Application settings')
param appSettings appSetting[] = []

@description('Resource tags')
param tags object

resource appService 'Microsoft.Web/sites@2024-04-01' = {
  name: name
  location: location
  tags: tags
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    serverFarmId: appServicePlanId
    httpsOnly: true
    siteConfig: {
      linuxFxVersion: runtimeStack
      ftpsState: 'Disabled'
      minTlsVersion: '1.2'
      appSettings: appSettings
    }
  }
}

output id string = appService.id
output name string = appService.name
output principalId string = appService.identity.principalId
output defaultHostName string = appService.properties.defaultHostName
