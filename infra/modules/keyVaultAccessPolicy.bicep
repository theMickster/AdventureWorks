// Adds access policies granting secret get to managed identities on an existing Key Vault that uses
// access policies (enableRbacAuthorization = false), such as MickKeyVaultWestUS. The 'add' policy name
// merges into the existing policies instead of replacing them. This module must be deployed with scope
// set to the Key Vault's resource group. Use keyVaultAccess.bicep for RBAC-enabled vaults.

@description('Name of the existing Key Vault')
param keyVaultName string

@description('Principal IDs to grant secret get')
param principalIds array

resource keyVault 'Microsoft.KeyVault/vaults@2023-07-01' existing = {
  name: keyVaultName
}

resource accessPolicies 'Microsoft.KeyVault/vaults/accessPolicies@2023-07-01' = {
  parent: keyVault
  name: 'add'
  properties: {
    accessPolicies: [
      for principalId in principalIds: {
        tenantId: tenant().tenantId
        objectId: principalId
        permissions: {
          secrets: [
            'get'
          ]
        }
      }
    ]
  }
}
