// Grants Key Vault Secrets User role to App Service managed identities on an existing Key Vault.
// This module must be deployed with scope set to the Key Vault's resource group.

@description('Name of the existing Key Vault')
param keyVaultName string

@description('Principal IDs to grant Key Vault Secrets User role')
param principalIds array

resource keyVault 'Microsoft.KeyVault/vaults@2023-07-01' existing = {
  name: keyVaultName
}

// Key Vault Secrets User role
var keyVaultSecretsUserRoleId = '4633458b-17de-408a-b874-0445c86b69e6'

resource roleAssignments 'Microsoft.Authorization/roleAssignments@2022-04-01' = [
  for (principalId, i) in principalIds: {
    name: guid(keyVault.id, principalId, keyVaultSecretsUserRoleId)
    scope: keyVault
    properties: {
      roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', keyVaultSecretsUserRoleId)
      principalId: principalId
      principalType: 'ServicePrincipal'
    }
  }
]
