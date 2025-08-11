// Provisions a Linux App Service Plan. Shared across all App Services (API + Web, dev + prod).

@description('Name of the App Service Plan')
param name string

@description('Azure region')
param location string

@description('SKU for the App Service Plan')
param sku object = {
  name: 'B1'
  tier: 'Basic'
}

@description('Resource tags')
param tags object

resource appServicePlan 'Microsoft.Web/serverfarms@2024-04-01' = {
  name: name
  location: location
  tags: tags
  kind: 'linux'
  properties: {
    reserved: true
  }
  sku: sku
}

output id string = appServicePlan.id
output name string = appServicePlan.name
