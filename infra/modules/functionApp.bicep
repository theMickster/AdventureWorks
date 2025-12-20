// Provisions a Flex Consumption Function App (system-assigned MI) hosting the Sales Order
// Saga orchestrator. Flex Consumption was chosen over the classic Consumption plan because it
// has full support for managed identity authentication with the Durable Functions Azure
// Storage provider (verified against current Microsoft Learn docs before this module was
// written) — no connection strings are used anywhere in this module.

@description('Name of the Function App')
param name string

@description('Azure region')
param location string

@description('Name of the storage account backing this Function App (deployment package + Durable Task hub)')
param storageAccountName string

@description('Blob endpoint of the storage account')
param storageAccountBlobEndpoint string

@description('Name of the blob container used for Flex Consumption deployment packages')
param deploymentContainerName string

@description('Fully qualified Service Bus namespace host name (e.g. my-namespace.servicebus.windows.net)')
param serviceBusFullyQualifiedNamespace string

@description('Name of the sales order events Service Bus topic')
param serviceBusTopicName string

@description('Name of the sales order saga Service Bus subscription')
param serviceBusSubscriptionName string

@description('Application Insights connection string for this Function App')
param appInsightsConnectionString string = ''

@description('Resource tags')
param tags object

resource plan 'Microsoft.Web/serverfarms@2024-04-01' = {
  name: '${name}-plan'
  location: location
  tags: tags
  kind: 'functionapp'
  sku: {
    name: 'FC1'
    tier: 'FlexConsumption'
  }
  properties: {
    reserved: true
  }
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
    serverFarmId: plan.id
    httpsOnly: true
    functionAppConfig: {
      deployment: {
        storage: {
          type: 'blobContainer'
          value: '${storageAccountBlobEndpoint}${deploymentContainerName}'
          authentication: {
            type: 'SystemAssignedIdentity'
          }
        }
      }
      scaleAndConcurrency: {
        maximumInstanceCount: 40
        instanceMemoryMB: 2048
      }
      runtime: {
        name: 'dotnet-isolated'
        version: '10.0'
      }
    }
    siteConfig: {
      ftpsState: 'Disabled'
      minTlsVersion: '1.2'
      appSettings: concat(
        [
          { name: 'AzureWebJobsStorage__accountName', value: storageAccountName }
          { name: 'AzureWebJobsStorage__credential', value: 'managedidentity' }
          { name: 'DurableStorage__accountName', value: storageAccountName }
          { name: 'DurableStorage__credential', value: 'managedidentity' }
          { name: 'ServiceBusConnection__fullyQualifiedNamespace', value: serviceBusFullyQualifiedNamespace }
          { name: 'ServiceBusSalesOrderEventsTopicName', value: serviceBusTopicName }
          { name: 'ServiceBusSalesOrderSagaSubscriptionName', value: serviceBusSubscriptionName }
        ],
        empty(appInsightsConnectionString)
          ? []
          : [{ name: 'APPLICATIONINSIGHTS_CONNECTION_STRING', value: appInsightsConnectionString }]
      )
    }
  }
}

output id string = functionApp.id
output name string = functionApp.name
output principalId string = functionApp.identity.principalId
output defaultHostName string = functionApp.properties.defaultHostName
