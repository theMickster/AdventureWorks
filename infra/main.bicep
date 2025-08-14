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

// --- Naming ---
var envSuffix = environment == 'prod' ? '' : '-${environment}'
var apiAppName = '${namingPrefix}-api${envSuffix}'
var webAppName = '${namingPrefix}-web${envSuffix}'
var planName = '${namingPrefix}-plan'

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

// --- Outputs ---
output apiAppServiceName string = apiAppService.outputs.name
output apiAppServiceHostName string = apiAppService.outputs.defaultHostName
output webAppServiceName string = webAppService.outputs.name
output webAppServiceHostName string = webAppService.outputs.defaultHostName
output keyVaultUri string = 'https://${keyVaultName}${az.environment().suffixes.keyvaultDns}/'
