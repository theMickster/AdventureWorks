using AdventureWorks.SalesOrderSaga.Infrastructure;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration;
using Moq;

namespace AdventureWorks.SalesOrderSaga.UnitTests.Infrastructure;

/// <summary>Coverage for topic selection and Service Bus message shape.</summary>
public sealed class SalesOrderSagaEventPublisherTests
{
    private const string Topic = "sales-order-events";

    private static IConfiguration Config(string? topic = Topic) =>
        new ConfigurationBuilder().AddInMemoryCollection(topic is null ? [] : [new("ServiceBusSalesOrderEventsTopicName", topic)]).Build();

    [Fact]
    public async Task PublishSerializedAsync_SendsPayloadWithSubjectAndMessageIdToConfiguredTopic()
    {
        ServiceBusMessage? sent = null;
        var sender = new Mock<ServiceBusSender>();
        sender.Setup(s => s.SendMessageAsync(It.IsAny<ServiceBusMessage>(), It.IsAny<CancellationToken>()))
            .Callback<ServiceBusMessage, CancellationToken>((m, _) => sent = m)
            .Returns(Task.CompletedTask);
        var client = new Mock<ServiceBusClient>();
        client.Setup(c => c.CreateSender(Topic)).Returns(sender.Object);

        await new SalesOrderSagaEventPublisher(client.Object, Config())
            .PublishSerializedAsync("OrderApproved", "OrderApproved:sales-order-saga-1", "{\"a\":1}", TestContext.Current.CancellationToken);

        Assert.Equal("OrderApproved", sent!.Subject);
        Assert.Equal("OrderApproved:sales-order-saga-1", sent.MessageId);
        Assert.Equal("{\"a\":1}", sent.Body.ToString());
    }

    [Fact]
    public async Task PublishAsync_SerializesPayloadAsJson()
    {
        ServiceBusMessage? sent = null;
        var sender = new Mock<ServiceBusSender>();
        sender.Setup(s => s.SendMessageAsync(It.IsAny<ServiceBusMessage>(), It.IsAny<CancellationToken>()))
            .Callback<ServiceBusMessage, CancellationToken>((m, _) => sent = m)
            .Returns(Task.CompletedTask);
        var client = new Mock<ServiceBusClient>();
        client.Setup(c => c.CreateSender(Topic)).Returns(sender.Object);

        await new SalesOrderSagaEventPublisher(client.Object, Config())
            .PublishAsync("OrderFailed", "id-1", new { SalesOrderId = 7 }, TestContext.Current.CancellationToken);

        Assert.Equal("{\"SalesOrderId\":7}", sent!.Body.ToString());
    }

    [Fact]
    public async Task PublishSerializedAsync_WhenTopicIsNotConfigured_ThrowsBeforeCreatingSender()
    {
        var client = new Mock<ServiceBusClient>(MockBehavior.Strict);

        await Assert.ThrowsAsync<InvalidOperationException>(() => new SalesOrderSagaEventPublisher(client.Object, Config(topic: null))
            .PublishSerializedAsync("OrderApproved", "id", "{}", TestContext.Current.CancellationToken));
    }
}
