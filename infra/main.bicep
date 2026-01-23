// Deployment strategy: Creates App Service Plan + 4 App Services (dev + prod API and Web).
// References existing App Insights (same RG) and Key Vault (cross-RG). SQL is not managed here.
// The Service Bus namespace (mick-adventureworks-events) and Cosmos account (mickcosmos) already exist
// and are shared by every environment; this file only manages entities and role assignments in them.
targetScope = 'resourceGroup'

@allowed(['dev', 'prod'])
@description('Deployment environment')
param environment string

@description('Azure region for all resources')
param location string

@description('Project name used for tagging')
param projectName string

@description('Name of the existing Application Insights resource (same RG)')
param appInsightsName string

@description('Name of the existing Key Vault')
param keyVaultName string

@description('Resource group containing the existing Key Vault')
param keyVaultResourceGroup string

@description('Base name prefix for App Service Plan and App Services')
param namingPrefix string = 'mick-adventureworks'

@description('.NET runtime stack for API App Services')
param apiRuntimeStack string = 'DOTNETCORE|10.0'

@description('Node runtime stack for Web App Services')
param webRuntimeStack string = 'NODE|24-lts'

@description('Whether to deploy the Sales Order Saga Function App slice (Feature 608/610). Dev-only for now — prod stays false until a later story provisions it.')
param deploySalesOrderSaga bool = false

@description('Required base URL for the payment authorization service.')
param paymentAuthorizationBaseUrl string

@description('Whether to deploy the BOM cost Rust Function App (Feature 609, Enabler Feature 1024) on the shared App Service plan.')
param deployBomCostFunction bool = false

@description('Whether to also create the -local topic and queue in the shared Service Bus namespace, for running the apps on a developer machine without touching the cloud dev entities.')
param provisionLocalMessaging bool = false

@description('Name of the existing Cosmos DB account')
param cosmosAccountName string = 'mickcosmos'

@description('Resource group containing the existing Cosmos DB account')
param cosmosResourceGroup string = 'Mick-West-US-3-CosmosDb'

@description('Name of the existing Key Vault secret that holds the AdventureWorks SQL connection string (ADO.NET format)')
param sqlConnectionStringSecretName string = 'adventureworks-sql-connection-string'

@description('Name of the Key Vault secret that holds the Redis connection string. Leave empty to run without Redis: the BOM cost app then caches results in process.')
param redisConnectionStringSecretName string = ''

// --- Naming ---
var envSuffix = environment == 'prod' ? '' : '-${environment}'
var apiAppName = '${namingPrefix}-api${envSuffix}'
var webAppName = '${namingPrefix}-web${envSuffix}'
var planName = '${namingPrefix}-plan'
// One namespace for every environment; entities are separated by envSuffix ('' prod, -dev, -local).
var serviceBusNamespaceName = 'mick-adventureworks-events'
var salesOrderSagaFunctionAppName = '${namingPrefix}-saga${envSuffix}'
// Storage account names must be 3-24 lowercase alphanumeric characters — namingPrefix contains
// hyphens and would exceed the limit, so this uses a dedicated short name instead.
var salesOrderSagaStorageAccountName = toLower('awsagafn${environment}')
// Must match the topic and subscription names serviceBus.bicep creates for this envSuffix.
var salesOrderSagaTopicName = 'sales-order-events${envSuffix}'
var salesOrderSagaSubscriptionName = 'sales-order-saga'
var salesOrderSagaPaymentSubscriptionName = 'sales-order-payment-results'
var bomCostFunctionAppName = '${namingPrefix}-bomcost${envSuffix}'
var bomCostStorageAccountName = toLower('awbomfn${environment}')
var bomBatchQueueName = 'bom-batch-requests${envSuffix}'
var bomCostContainerName = 'bom-cost-results${envSuffix}'
var cosmosDatabaseName = 'PlatformDatabases'
var keyVaultUri = 'https://${keyVaultName}${az.environment().suffixes.keyvaultDns}/'

// --- Tags ---
var tags = {
  Environment: environment
  Project: projectName
  ManagedBy: 'Bicep'
}

// --- Existing Resources ---

resource appInsights 'Microsoft.Insights/components@2020-02-02' existing = {
  name: appInsightsName
}

// --- Modules ---

module appServicePlan 'modules/appServicePlan.bicep' = {
  name: 'appServicePlan'
  params: {
    name: planName
    location: location
    tags: tags
  }
}

module apiAppService 'modules/appService.bicep' = {
  name: 'apiAppService'
  params: {
    name: apiAppName
    location: location
    appServicePlanId: appServicePlan.outputs.id
    runtimeStack: apiRuntimeStack
    appSettings: [
      { name: 'APPLICATIONINSIGHTS_CONNECTION_STRING', value: appInsights.properties.ConnectionString }
      { name: 'ASPNETCORE_ENVIRONMENT', value: environment == 'prod' ? 'Production' : 'Development' }
      { name: 'KeyVault__VaultUri', value: 'https://${keyVaultName}${az.environment().suffixes.keyvaultDns}/' }
    ]
    tags: tags
  }
}

module webAppService 'modules/appService.bicep' = {
  name: 'webAppService'
  params: {
    name: webAppName
    location: location
    appServicePlanId: appServicePlan.outputs.id
    runtimeStack: webRuntimeStack
    appSettings: [
      { name: 'APPLICATIONINSIGHTS_CONNECTION_STRING', value: appInsights.properties.ConnectionString }
    ]
    tags: tags
  }
}

// Grant App Service managed identities access to the existing Key Vault (cross-RG)
module keyVaultAccess 'modules/keyVaultAccess.bicep' = {
  name: 'keyVaultAccess-${environment}'
  scope: resourceGroup(keyVaultResourceGroup)
  params: {
    keyVaultName: keyVaultName
    principalIds: [
      apiAppService.outputs.principalId
    ]
  }
}

// --- Sales Order Saga slice (US 806 / Feature 608, 610) — dev-only until a later story provisions prod ---

module salesOrderSagaStorage 'modules/storageAccount.bicep' = if (deploySalesOrderSaga) {
  name: 'salesOrderSagaStorage-${environment}'
  params: {
    name: salesOrderSagaStorageAccountName
    location: location
    tags: tags
  }
}

// Topic, subscriptions, filter rules and queue for THIS environment's suffix in the shared namespace.
module serviceBus 'modules/serviceBus.bicep' = if (deploySalesOrderSaga || deployBomCostFunction) {
  name: 'serviceBus-${environment}'
  params: {
    namespaceName: serviceBusNamespaceName
    entitySuffix: envSuffix
    // The live prod topic was created without duplicate detection, which cannot be changed in place.
    requiresDuplicateDetection: environment != 'prod'
    publisherPrincipalId: apiAppService.outputs.principalId
    sagaPrincipalId: deploySalesOrderSaga ? salesOrderSagaFunctionApp!.outputs.principalId : ''
    bomCostPrincipalId: deployBomCostFunction ? bomCostFunctionApp!.outputs.principalId : ''
  }
}

// The -local entities exist only so a developer machine can run against real Service Bus without
// touching the cloud dev topic and queue. No app runs in Azure for them, so no principals are granted
// here; the developer's own login needs Service Bus Data Sender/Receiver on them.
module serviceBusLocal 'modules/serviceBus.bicep' = if (provisionLocalMessaging) {
  name: 'serviceBusLocal-${environment}'
  params: {
    namespaceName: serviceBusNamespaceName
    entitySuffix: '-local'
  }
}

module salesOrderSagaFunctionApp 'modules/functionApp.bicep' = if (deploySalesOrderSaga) {
  name: 'salesOrderSagaFunctionApp-${environment}'
  params: {
    name: salesOrderSagaFunctionAppName
    location: location
    storageAccountName: salesOrderSagaStorage.outputs.name
    storageAccountBlobEndpoint: salesOrderSagaStorage.outputs.blobEndpoint
    deploymentContainerName: salesOrderSagaStorage.outputs.deploymentContainerName
    serviceBusFullyQualifiedNamespace: '${serviceBusNamespaceName}.servicebus.windows.net'
    serviceBusTopicName: salesOrderSagaTopicName
    serviceBusSubscriptionName: salesOrderSagaSubscriptionName
    serviceBusPaymentSubscriptionName: salesOrderSagaPaymentSubscriptionName
    paymentAuthorizationBaseUrl: paymentAuthorizationBaseUrl
    appInsightsConnectionString: appInsights.properties.ConnectionString
    tags: tags
  }
}

// Grant the Function App's identity access to its own storage account (Durable Task hub +
// deployment container). Storage RBAC has no single "least privilege for Durable Functions"
// role, so the three roles Microsoft's docs recommend are granted individually, scoped to just
// this storage account (not the resource group).
resource salesOrderSagaStorageAccountExisting 'Microsoft.Storage/storageAccounts@2023-05-01' existing = if (deploySalesOrderSaga) {
  name: salesOrderSagaStorageAccountName
  dependsOn: [
    salesOrderSagaStorage
  ]
}

var storageDataRoleIds = [
  'ba92f5b4-2d11-453d-a403-e96b0029c9fe' // Storage Blob Data Contributor
  '974c5e8b-45b9-4653-ba55-5f855dd0fb88' // Storage Queue Data Contributor
  '0a9a7e1f-b9d0-4cc4-a60d-0319b160aaa3' // Storage Table Data Contributor
]

resource salesOrderSagaStorageRoleAssignments 'Microsoft.Authorization/roleAssignments@2022-04-01' = [
  for roleId in storageDataRoleIds: if (deploySalesOrderSaga) {
    name: guid(salesOrderSagaStorageAccountName, salesOrderSagaFunctionAppName, roleId)
    scope: salesOrderSagaStorageAccountExisting
    properties: {
      roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', roleId)
      principalId: salesOrderSagaFunctionApp.outputs.principalId
      principalType: 'ServicePrincipal'
    }
  }
]

// --- BOM cost Rust Function App (Feature 609, Enabler Feature 1024) ---
// Runs on the shared App Service plan (B1) with Always On, not on its own plan.

module bomCostStorage 'modules/storageAccount.bicep' = if (deployBomCostFunction) {
  name: 'bomCostStorage-${environment}'
  params: {
    name: bomCostStorageAccountName
    location: location
    tags: tags
  }
}

module bomCostFunctionApp 'modules/functionAppRust.bicep' = if (deployBomCostFunction) {
  name: 'bomCostFunctionApp-${environment}'
  params: {
    name: bomCostFunctionAppName
    location: location
    appServicePlanId: appServicePlan.outputs.id
    storageAccountName: bomCostStorage!.outputs.name
    appInsightsConnectionString: appInsights.properties.ConnectionString
    serviceBusFullyQualifiedNamespace: '${serviceBusNamespaceName}.servicebus.windows.net'
    bomBatchQueueName: bomBatchQueueName
    keyVaultUri: keyVaultUri
    cosmosEndpoint: 'https://${cosmosAccountName}.documents.azure.com:443/'
    cosmosDatabase: cosmosDatabaseName
    cosmosContainer: bomCostContainerName
    cosmosPreferredRegion: location == 'westus3' ? 'West US 3' : location
    sqlConnectionStringReference: '@Microsoft.KeyVault(SecretUri=${keyVaultUri}secrets/${sqlConnectionStringSecretName}/)'
    redisConnectionStringReference: empty(redisConnectionStringSecretName)
      ? ''
      : '@Microsoft.KeyVault(SecretUri=${keyVaultUri}secrets/${redisConnectionStringSecretName}/)'
    tags: tags
  }
}

// The vault MickKeyVaultWestUS uses access policies, so keyVaultAccess.bicep (an RBAC role) would
// have no effect here.
module bomCostKeyVaultAccess 'modules/keyVaultAccessPolicy.bicep' = if (deployBomCostFunction) {
  name: 'bomCostKeyVaultAccess-${environment}'
  scope: resourceGroup(keyVaultResourceGroup)
  params: {
    keyVaultName: keyVaultName
    principalIds: [
      bomCostFunctionApp!.outputs.principalId
    ]
  }
}

module bomCostCosmosAccess 'modules/cosmosRoleAssignment.bicep' = if (deployBomCostFunction) {
  name: 'bomCostCosmosAccess-${environment}'
  scope: resourceGroup(cosmosResourceGroup)
  params: {
    accountName: cosmosAccountName
    databaseName: cosmosDatabaseName
    containerName: bomCostContainerName
    principalId: bomCostFunctionApp!.outputs.principalId
  }
}

// --- Outputs ---
output apiAppServiceName string = apiAppService.outputs.name
output apiAppServiceHostName string = apiAppService.outputs.defaultHostName
output webAppServiceName string = webAppService.outputs.name
output webAppServiceHostName string = webAppService.outputs.defaultHostName
output keyVaultUri string = 'https://${keyVaultName}${az.environment().suffixes.keyvaultDns}/'
output salesOrderSagaFunctionAppName string = deploySalesOrderSaga ? salesOrderSagaFunctionApp.outputs.name : ''
output salesOrderSagaServiceBusNamespace string = deploySalesOrderSaga ? '${serviceBusNamespaceName}.servicebus.windows.net' : ''
output bomCostFunctionAppName string = deployBomCostFunction ? bomCostFunctionApp!.outputs.name : ''
