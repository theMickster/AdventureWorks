using System.Text.Json;
using AdventureWorks.SalesOrderSaga.Models;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration;

namespace AdventureWorks.SalesOrderSaga.Infrastructure;

/// <summary>Publishes terminal saga events to the configured sales-order topic.</summary>
public interface ISalesOrderSagaEventPublisher
{
    /// <summary>Publishes a typed message with its event name in Service Bus Subject.</summary>
    Task PublishAsync<T>(string eventName, string messageId, T payload, CancellationToken cancellationToken);

    /// <summary>Publishes an outbox payload that is already serialized as JSON.</summary>
    Task PublishSerializedAsync(string eventName, string messageId, string payload, CancellationToken cancellationToken);
}

/// <summary>Service Bus implementation of <see cref="ISalesOrderSagaEventPublisher"/>.</summary>
public sealed class SalesOrderSagaEventPublisher(ServiceBusClient client, IConfiguration configuration) : ISalesOrderSagaEventPublisher
{
    public async Task PublishAsync<T>(string eventName, string messageId, T payload, CancellationToken cancellationToken)
        => await PublishSerializedAsync(eventName, messageId, JsonSerializer.Serialize(payload), cancellationToken);

    public async Task PublishSerializedAsync(string eventName, string messageId, string payload, CancellationToken cancellationToken)
    {
        var topic = configuration["ServiceBusSalesOrderEventsTopicName"]
            ?? throw new InvalidOperationException("Missing required configuration value 'ServiceBusSalesOrderEventsTopicName'.");
        await using var sender = client.CreateSender(topic);
        var message = new ServiceBusMessage(BinaryData.FromString(payload))
        {
            Subject = eventName,
            // A replayed publish activity must be harmless to a duplicate-detection-enabled topic.
            MessageId = messageId
        };
        await sender.SendMessageAsync(message, cancellationToken);
    }
}
