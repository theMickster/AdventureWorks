// Provisions a Standard Service Bus namespace with the sales-order-events topic and
// sales-order-saga subscription, plus RBAC role assignments for the publisher (API) and
// consumer (Sales Order Saga Function App) identities. Standard tier is required for topics
// (Basic tier supports queues only).

@description('Name of the Service Bus namespace')
param name string

@description('Azure region')
param location string

@description('Name of the sales order events topic')
param topicName string = 'sales-order-events'

@description('Name of the sales order saga subscription')
param subscriptionName string = 'sales-order-saga'

@description('Principal ID of the identity that publishes OrderCreated (granted Service Bus Data Sender)')
param publisherPrincipalId string

@description('Principal ID of the identity that consumes from the subscription (granted Service Bus Data Receiver)')
param subscriberPrincipalId string

@description('Resource tags')
param tags object

resource serviceBusNamespace 'Microsoft.ServiceBus/namespaces@2024-01-01' = {
  name: name
  location: location
  tags: tags
  sku: {
    name: 'Standard'
    tier: 'Standard'
  }
}

resource topic 'Microsoft.ServiceBus/namespaces/topics@2024-01-01' = {
  parent: serviceBusNamespace
  name: topicName
  properties: {
    defaultMessageTimeToLive: 'P1D'
  }
}

resource subscription 'Microsoft.ServiceBus/namespaces/topics/subscriptions@2024-01-01' = {
  parent: topic
  name: subscriptionName
  properties: {
    defaultMessageTimeToLive: 'P1D'
    maxDeliveryCount: 10
    deadLetteringOnMessageExpiration: true
  }
}

// Built-in role IDs
var serviceBusDataSenderRoleId = '69a216fc-b8fb-44d8-bc22-1f3c2cd27a39'
var serviceBusDataReceiverRoleId = '4f6d3b9b-027b-4f4c-9142-0e5a2a2247e0'

resource publisherRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(topic.id, publisherPrincipalId, serviceBusDataSenderRoleId)
  scope: topic
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', serviceBusDataSenderRoleId)
    principalId: publisherPrincipalId
    principalType: 'ServicePrincipal'
  }
}

// Azure RBAC for Service Bus does not support scoping to an individual subscription — topic is
// the finest-grained scope available for a subscription consumer.
resource subscriberRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(subscription.id, subscriberPrincipalId, serviceBusDataReceiverRoleId)
  scope: topic
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', serviceBusDataReceiverRoleId)
    principalId: subscriberPrincipalId
    principalType: 'ServicePrincipal'
  }
}

output id string = serviceBusNamespace.id
output name string = serviceBusNamespace.name
output fullyQualifiedNamespace string = '${serviceBusNamespace.name}.servicebus.windows.net'
output topicName string = topic.name
output subscriptionName string = subscription.name
