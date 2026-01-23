// Grants a managed identity the Cosmos DB Built-in Data Contributor data-plane role, scoped to one
// container. This module must be deployed with scope set to the Cosmos account's resource group.
// Cosmos data-plane roles are separate from Azure RBAC, so this is a sqlRoleAssignment, not a roleAssignment.

@description('Name of the existing Cosmos DB account')
param accountName string

@description('Database that holds the container')
param databaseName string

@description('Container the identity may read and write')
param containerName string

@description('Principal ID of the identity to grant')
param principalId string

// Cosmos DB Built-in Data Contributor
var dataContributorRoleId = '00000000-0000-0000-0000-000000000002'

resource account 'Microsoft.DocumentDB/databaseAccounts@2024-05-15' existing = {
  name: accountName
}

resource roleAssignment 'Microsoft.DocumentDB/databaseAccounts/sqlRoleAssignments@2024-05-15' = {
  parent: account
  name: guid(account.id, principalId, databaseName, containerName, dataContributorRoleId)
  properties: {
    roleDefinitionId: '${account.id}/sqlRoleDefinitions/${dataContributorRoleId}'
    principalId: principalId
    scope: '${account.id}/dbs/${databaseName}/colls/${containerName}'
  }
}
