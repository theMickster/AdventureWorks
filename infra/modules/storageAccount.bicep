// Provisions a Storage account used for the Sales Order Saga Function App:
// the Flex Consumption deployment package container, and the Durable Task hub
// (queues/tables/blobs created automatically by the Durable Functions runtime).
// Access is identity-based (RBAC) only — no shared keys are read by this module or its consumers.

@description('Name of the Storage account')
param name string

@description('Azure region')
param location string

@description('Name of the blob container used for Flex Consumption deployment packages')
param deploymentContainerName string = 'app-package'

@description('Resource tags')
param tags object

resource storageAccount 'Microsoft.Storage/storageAccounts@2023-05-01' = {
  name: name
  location: location
  tags: tags
  kind: 'StorageV2'
  sku: {
    name: 'Standard_LRS'
  }
  properties: {
    minimumTlsVersion: 'TLS1_2'
    allowSharedKeyAccess: false
    allowBlobPublicAccess: false
    supportsHttpsTrafficOnly: true
  }
}

resource blobService 'Microsoft.Storage/storageAccounts/blobServices@2023-05-01' = {
  parent: storageAccount
  name: 'default'
}

resource deploymentContainer 'Microsoft.Storage/storageAccounts/blobServices/containers@2023-05-01' = {
  parent: blobService
  name: deploymentContainerName
}

output id string = storageAccount.id
output name string = storageAccount.name
output blobEndpoint string = storageAccount.properties.primaryEndpoints.blob
output deploymentContainerName string = deploymentContainer.name
