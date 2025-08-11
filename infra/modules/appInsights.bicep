// Provisions Application Insights (workspace-based) and its backing Log Analytics workspace.

@description('Name of the Application Insights resource')
param name string

@description('Azure region')
param location string

@description('Name of the Log Analytics workspace')
param logAnalyticsWorkspaceName string

@description('Resource tags')
param tags object

resource logAnalyticsWorkspace 'Microsoft.OperationalInsights/workspaces@2023-09-01' = {
  name: logAnalyticsWorkspaceName
  location: location
  tags: tags
  properties: {
    sku: {
      name: 'PerGB2018'
    }
    retentionInDays: 30
  }
}

resource appInsights 'Microsoft.Insights/components@2020-02-02' = {
  name: name
  location: location
  tags: tags
  kind: 'web'
  properties: {
    Application_Type: 'web'
    WorkspaceResourceId: logAnalyticsWorkspace.id
  }
}

output id string = appInsights.id
output connectionString string = appInsights.properties.ConnectionString
