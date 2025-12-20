// Deployment strategy: Creates App Service Plan + 4 App Services (dev + prod API and Web).
// References existing App Insights (same RG) and Key Vault (cross-RG). SQL is not managed here.
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

// --- Naming ---
var envSuffix = environment == 'prod' ? '' : '-${environment}'
var apiAppName = '${namingPrefix}-api${envSuffix}'
var webAppName = '${namingPrefix}-web${envSuffix}'
var planName = '${namingPrefix}-plan'
var serviceBusNamespaceName = '${namingPrefix}-sb${envSuffix}'
var salesOrderSagaFunctionAppName = '${namingPrefix}-saga${envSuffix}'
// Storage account names must be 3-24 lowercase alphanumeric characters — namingPrefix contains
// hyphens and would exceed the limit, so this uses a dedicated short name instead.
var salesOrderSagaStorageAccountName = toLower('awsagafn${environment}')
var salesOrderSagaTopicName = 'sales-order-events'
var salesOrderSagaSubscriptionName = 'sales-order-saga'

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

module salesOrderSagaServiceBus 'modules/serviceBus.bicep' = if (deploySalesOrderSaga) {
  name: 'salesOrderSagaServiceBus-${environment}'
  params: {
    name: serviceBusNamespaceName
    location: location
    topicName: salesOrderSagaTopicName
    subscriptionName: salesOrderSagaSubscriptionName
    publisherPrincipalId: apiAppService.outputs.principalId
    subscriberPrincipalId: salesOrderSagaFunctionApp.outputs.principalId
    tags: tags
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

// --- Outputs ---
output apiAppServiceName string = apiAppService.outputs.name
output apiAppServiceHostName string = apiAppService.outputs.defaultHostName
output webAppServiceName string = webAppService.outputs.name
output webAppServiceHostName string = webAppService.outputs.defaultHostName
output keyVaultUri string = 'https://${keyVaultName}${az.environment().suffixes.keyvaultDns}/'
output salesOrderSagaFunctionAppName string = deploySalesOrderSaga ? salesOrderSagaFunctionApp.outputs.name : ''
output salesOrderSagaServiceBusNamespace string = deploySalesOrderSaga ? salesOrderSagaServiceBus.outputs.fullyQualifiedNamespace : ''
