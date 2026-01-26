// Provisions the Rust BOM cost Function App (Feature 609, custom handler) on an existing Linux App
// Service plan (the shared B1 plan). Always On replaces the cold-start protection of a Premium plan.
// Host storage, Service Bus and Cosmos use the system-assigned identity; the only secrets are Key Vault
// references. The role assignments for Service Bus, Key Vault and Cosmos live in their own modules.

@description('Name of the Function App')
param name string

@description('Azure region')
param location string

@description('Resource ID of the existing Linux App Service plan')
param appServicePlanId string

@description('Name of the existing storage account used as host storage (AzureWebJobsStorage)')
param storageAccountName string

@description('Application Insights connection string')
param appInsightsConnectionString string = ''

@description('Fully qualified Service Bus namespace host name')
param serviceBusFullyQualifiedNamespace string

@description('Name of the BOM batch queue for this environment')
param bomBatchQueueName string

@description('Key Vault URI; the handler reads the Cosmos endpoint from it')
param keyVaultUri string

@description('Cosmos DB account endpoint')
param cosmosEndpoint string

@description('Cosmos DB database name')
param cosmosDatabase string

@description('Cosmos DB container for results')
param cosmosContainer string

@description('Region the Cosmos SDK prefers for routing; must be a region of the account')
param cosmosPreferredRegion string

@description('AdventureWorks SQL connection string. It holds no secret: Authentication=Active Directory Default makes the handler log in with its managed identity, which needs a database user (see infra/AZURE_FUNCTIONS_SETUP.md).')
param sqlConnectionString string

@description('Key Vault reference value for the Redis connection string. Empty omits the setting, and the handler then caches results in process instead of in Redis.')
param redisConnectionStringReference string = ''

@description('Resource tags')
param tags object

resource storageAccount 'Microsoft.Storage/storageAccounts@2023-05-01' existing = {
  name: storageAccountName
}

resource functionApp 'Microsoft.Web/sites@2024-04-01' = {
  name: name
  location: location
  tags: tags
  kind: 'functionapp,linux'
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    serverFarmId: appServicePlanId
    httpsOnly: true
    siteConfig: {
      // Microsoft's custom handler docs say to pick the .NET Core stack; FUNCTIONS_WORKER_RUNTIME=custom
      // is what makes the host run the Rust binary instead of a .NET worker.
      linuxFxVersion: 'DOTNET-ISOLATED|10.0'
      alwaysOn: true
      ftpsState: 'Disabled'
      minTlsVersion: '1.2'
      appSettings: concat(
        [
          { name: 'FUNCTIONS_EXTENSION_VERSION', value: '~4' }
          { name: 'FUNCTIONS_WORKER_RUNTIME', value: 'custom' }
          { name: 'WEBSITE_RUN_FROM_PACKAGE', value: '1' }
          { name: 'AzureWebJobsStorage__accountName', value: storageAccountName }
          { name: 'AzureWebJobsStorage__credential', value: 'managedidentity' }
          { name: 'ServiceBusConnection__fullyQualifiedNamespace', value: serviceBusFullyQualifiedNamespace }
          { name: 'BomBatchQueueName', value: bomBatchQueueName }
          { name: 'KeyVault__VaultUri', value: keyVaultUri }
          { name: 'Cosmos__Endpoint', value: cosmosEndpoint }
          { name: 'Cosmos__Database', value: cosmosDatabase }
          { name: 'Cosmos__Container', value: cosmosContainer }
          { name: 'Cosmos__PreferredRegion', value: cosmosPreferredRegion }
          { name: 'ConnectionStrings__AdventureWorks', value: sqlConnectionString }
        ],
        empty(redisConnectionStringReference)
          ? []
          : [{ name: 'ConnectionStrings__Redis', value: redisConnectionStringReference }],
        empty(appInsightsConnectionString)
          ? []
          : [{ name: 'APPLICATIONINSIGHTS_CONNECTION_STRING', value: appInsightsConnectionString }]
      )
    }
  }
}

// Storage Blob Data Owner is the documented minimum for the AzureWebJobsStorage identity connection;
// Table Data Contributor lets the host raise diagnostic events when the app cannot start.
var hostStorageRoleIds = [
  'b7e6dc6d-f1e8-4753-8033-0f276bb0955b' // Storage Blob Data Owner
  '0a9a7e1f-b9d0-4cc4-a60d-0319b160aaa3' // Storage Table Data Contributor
]

resource hostStorageRoleAssignments 'Microsoft.Authorization/roleAssignments@2022-04-01' = [
  for roleId in hostStorageRoleIds: {
    name: guid(storageAccount.id, functionApp.id, roleId)
    scope: storageAccount
    properties: {
      roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', roleId)
      principalId: functionApp.identity.principalId
      principalType: 'ServicePrincipal'
    }
  }
]

output id string = functionApp.id
output name string = functionApp.name
output principalId string = functionApp.identity.principalId
output defaultHostName string = functionApp.properties.defaultHostName
