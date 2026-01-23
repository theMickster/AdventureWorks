// Provisions the Service Bus entities for ONE environment inside the shared, existing namespace
// mick-adventureworks-events: the sales-order-events topic (with the sales-order-saga and
// sales-order-payment-results subscriptions and their filter rules) and the bom-batch-requests queue.
// The namespace is not created here. Prod, dev and local share it and are separated by entity name
// suffix: '' (prod), '-dev', '-local'. Standard tier is required for topics.

@description('Name of the existing, shared Service Bus namespace')
param namespaceName string

@description('Suffix appended to every entity name: empty for prod, -dev, -local')
param entitySuffix string

@description('Duplicate detection cannot be changed on an existing topic and the live prod topic was created without it, so prod passes false.')
param requiresDuplicateDetection bool = true

@description('Principal ID of the identity that publishes OrderCreated (granted Service Bus Data Sender). Empty skips the assignment.')
param publisherPrincipalId string = ''

@description('Principal ID of the Sales Order Saga Function App (granted Service Bus Data Sender and Receiver on the topic). Empty skips the assignments.')
param sagaPrincipalId string = ''

@description('Principal ID of the BOM cost Function App (granted Service Bus Data Receiver on the queue). Empty skips the assignment.')
param bomCostPrincipalId string = ''

var topicName = 'sales-order-events${entitySuffix}'
var sagaSubscriptionName = 'sales-order-saga'
var paymentSubscriptionName = 'sales-order-payment-results'
var queueName = 'bom-batch-requests${entitySuffix}'

resource serviceBusNamespace 'Microsoft.ServiceBus/namespaces@2024-01-01' existing = {
  name: namespaceName
}

resource topic 'Microsoft.ServiceBus/namespaces/topics@2024-01-01' = {
  parent: serviceBusNamespace
  name: topicName
  properties: {
    defaultMessageTimeToLive: 'P1D'
    requiresDuplicateDetection: requiresDuplicateDetection
    duplicateDetectionHistoryTimeWindow: 'PT10M'
  }
}

resource sagaSubscription 'Microsoft.ServiceBus/namespaces/topics/subscriptions@2024-01-01' = {
  parent: topic
  name: sagaSubscriptionName
  properties: {
    defaultMessageTimeToLive: 'P1D'
    maxDeliveryCount: 10
    deadLetteringOnMessageExpiration: true
  }
}

// Each subscription must filter on the message label so a Function does not consume unrelated topic
// messages. The rules are NOT named '$Default': a rule with that name did not persist (verified
// 2026-11-08 on the -local subscriptions, which ended up with no rules and so received nothing).
// Verify the rules after the first deploy of a new suffix and confirm no TrueFilter '$Default' remains.
resource orderCreatedRule 'Microsoft.ServiceBus/namespaces/topics/subscriptions/rules@2024-01-01' = {
  parent: sagaSubscription
  name: 'OrderCreatedOnly'
  properties: {
    filterType: 'SqlFilter'
    sqlFilter: {
      sqlExpression: 'sys.Label = \'OrderCreated\''
      compatibilityLevel: 20
    }
  }
}

resource paymentSubscription 'Microsoft.ServiceBus/namespaces/topics/subscriptions@2024-01-01' = {
  parent: topic
  name: paymentSubscriptionName
  properties: {
    defaultMessageTimeToLive: 'P1D'
    maxDeliveryCount: 10
    deadLetteringOnMessageExpiration: true
  }
}

resource paymentResultRule 'Microsoft.ServiceBus/namespaces/topics/subscriptions/rules@2024-01-01' = {
  parent: paymentSubscription
  name: 'PaymentResultsOnly'
  properties: {
    filterType: 'SqlFilter'
    sqlFilter: {
      sqlExpression: 'sys.Label IN (\'PaymentApproved\', \'PaymentDeclined\')'
      compatibilityLevel: 20
    }
  }
}

resource bomBatchQueue 'Microsoft.ServiceBus/namespaces/queues@2024-01-01' = {
  parent: serviceBusNamespace
  name: queueName
  properties: {
    lockDuration: 'PT1M'
    maxDeliveryCount: 3
    defaultMessageTimeToLive: 'P1D'
    deadLetteringOnMessageExpiration: true
  }
}

// Built-in role IDs
var serviceBusDataSenderRoleId = '69a216fc-b8fb-44d8-bc22-1f3c2cd27a39'
var serviceBusDataReceiverRoleId = '4f6d3b9b-027b-4f4c-9142-0e5a2a2247e0'

resource publisherRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = if (!empty(publisherPrincipalId)) {
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
resource sagaReceiverRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = if (!empty(sagaPrincipalId)) {
  name: guid(sagaSubscription.id, sagaPrincipalId, serviceBusDataReceiverRoleId)
  scope: topic
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', serviceBusDataReceiverRoleId)
    principalId: sagaPrincipalId
    principalType: 'ServicePrincipal'
  }
}

// The saga Function publishes outbox messages as well as consuming its two subscriptions.
resource sagaSenderRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = if (!empty(sagaPrincipalId)) {
  name: guid(topic.id, sagaPrincipalId, serviceBusDataSenderRoleId)
  scope: topic
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', serviceBusDataSenderRoleId)
    principalId: sagaPrincipalId
    principalType: 'ServicePrincipal'
  }
}

resource bomCostReceiverRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = if (!empty(bomCostPrincipalId)) {
  name: guid(bomBatchQueue.id, bomCostPrincipalId, serviceBusDataReceiverRoleId)
  scope: bomBatchQueue
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', serviceBusDataReceiverRoleId)
    principalId: bomCostPrincipalId
    principalType: 'ServicePrincipal'
  }
}

output topicName string = topic.name
output sagaSubscriptionName string = sagaSubscription.name
output queueName string = bomBatchQueue.name
