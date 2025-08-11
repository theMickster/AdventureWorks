// Deployment strategy: Deploy once per environment (dev/prod) to the SAME resource group.
// Shared resources (plan, SQL, Key Vault, App Insights) have no env suffix — they are
// created on first deploy and reused on subsequent deploys. App Services are env-specific.
// Two deployments to one RG yield 4 App Services on 1 shared plan, as intended.
targetScope = 'resourceGroup'

@allowed(['dev', 'prod'])
@description('Deployment environment')
param environment string

@description('Azure region for all resources')
param location string

@description('Project name used for tagging')
param projectName string

@description('Object ID of the Entra admin for SQL Server')
param sqlAdminObjectId string

@description('SQL admin login name')
param sqlAdminLogin string

@secure()
@description('SQL admin password')
param sqlAdminPassword string

// --- Naming ---
var envSuffix = environment == 'prod' ? '' : '-${environment}'
var apiAppName = 'mick-adventureworks-api${envSuffix}'
var webAppName = 'mick-adventureworks-web${envSuffix}'
var planName = 'mick-adventureworks-plan'
var sqlServerName = 'mick-adventureworks-sql'
var sqlDatabaseName = 'AdventureWorks'
var keyVaultName = 'mick-aw-kv'
var appInsightsName = 'mick-adventureworks-insights'
var logAnalyticsName = 'mick-adventureworks-logs'

// --- Tags ---
var tags = {
  Environment: environment
  Project: projectName
  ManagedBy: 'Bicep'
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

module appInsights 'modules/appInsights.bicep' = {
  name: 'appInsights'
  params: {
    name: appInsightsName
    location: location
    logAnalyticsWorkspaceName: logAnalyticsName
    tags: tags
  }
}

module apiAppService 'modules/appService.bicep' = {
  name: 'apiAppService'
  params: {
    name: apiAppName
    location: location
    appServicePlanId: appServicePlan.outputs.id
    runtimeStack: 'DOTNETCORE|10.0'
    appSettings: [
      { name: 'APPLICATIONINSIGHTS_CONNECTION_STRING', value: appInsights.outputs.connectionString }
      { name: 'ASPNETCORE_ENVIRONMENT', value: environment == 'prod' ? 'Production' : 'Development' }
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
    runtimeStack: 'NODE|24-lts'
    appSettings: [
      { name: 'APPLICATIONINSIGHTS_CONNECTION_STRING', value: appInsights.outputs.connectionString }
    ]
    tags: tags
  }
}

module sqlServer 'modules/sqlServer.bicep' = {
  name: 'sqlServer'
  params: {
    name: sqlServerName
    location: location
    adminLogin: sqlAdminLogin
    adminPassword: sqlAdminPassword
    entraAdminObjectId: sqlAdminObjectId
    entraAdminDisplayName: '${projectName} SQL Admins'
    tags: tags
  }
}

module sqlDatabase 'modules/sqlDatabase.bicep' = {
  name: 'sqlDatabase'
  params: {
    name: sqlDatabaseName
    serverName: sqlServer.outputs.name
    location: location
    tags: tags
  }
}

module keyVault 'modules/keyVault.bicep' = {
  name: 'keyVault'
  params: {
    name: keyVaultName
    location: location
    tenantId: tenant().tenantId
    principalIds: [
      apiAppService.outputs.principalId
      webAppService.outputs.principalId
    ]
    tags: tags
  }
}

// --- Outputs ---
output apiAppServiceName string = apiAppService.outputs.name
output apiAppServiceHostName string = apiAppService.outputs.defaultHostName
output webAppServiceName string = webAppService.outputs.name
output webAppServiceHostName string = webAppService.outputs.defaultHostName
output sqlServerFqdn string = sqlServer.outputs.fullyQualifiedDomainName
output sqlDatabaseName string = sqlDatabase.outputs.name
output keyVaultUri string = keyVault.outputs.uri
