using System.Text.Json;
using AdventureWorks.SalesOrderSaga.Contracts;
using AdventureWorks.SalesOrderSaga.Functions;
using Azure.Messaging.ServiceBus;
using Microsoft.DurableTask.Client;
using Moq;

namespace AdventureWorks.SalesOrderSaga.UnitTests.Functions;

/// <summary>Coverage for correlating payment-result messages to their saga instance.</summary>
public sealed class PaymentResultReceiverTests
{
    private const int SalesOrderId = 71774;
    private const string InstanceId = "sales-order-saga-71774";

    [Theory]
    [InlineData(SagaEventNames.PaymentApproved)]
    [InlineData(SagaEventNames.PaymentDeclined)]
    public async Task RunAsync_RaisesEventOnDeterministicInstance(string eventName)
    {
        var payment = new PaymentResultEvent(SalesOrderId, InstanceId, "code", "reason");
        var client = new Mock<DurableTaskClient>("test");

        await PaymentResultReceiver.RunAsync(Message(eventName, payment), client.Object, TestContext.Current.CancellationToken);

        client.Verify(c => c.RaiseEventAsync(
            InstanceId, eventName, It.Is<object?>(p => p is PaymentResultEvent && ((PaymentResultEvent)p).SalesOrderId == SalesOrderId), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RunAsync_IgnoresUnrelatedSubjects()
    {
        var client = new Mock<DurableTaskClient>(MockBehavior.Strict, "test");

        await PaymentResultReceiver.RunAsync(
            Message(SagaEventNames.OrderCreated, new PaymentResultEvent(SalesOrderId, InstanceId)), client.Object, TestContext.Current.CancellationToken);

        client.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task RunAsync_WhenIdempotencyKeyDoesNotMatchInstance_ThrowsWithoutRaisingEvent()
    {
        var client = new Mock<DurableTaskClient>(MockBehavior.Strict, "test");

        await Assert.ThrowsAsync<InvalidOperationException>(() => PaymentResultReceiver.RunAsync(
            Message(SagaEventNames.PaymentApproved, new PaymentResultEvent(SalesOrderId, "sales-order-saga-1")), client.Object, TestContext.Current.CancellationToken));

        client.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task RunAsync_WhenBodyIsJsonNull_Throws()
    {
        var client = new Mock<DurableTaskClient>(MockBehavior.Strict, "test");
        var message = ServiceBusModelFactory.ServiceBusReceivedMessage(body: BinaryData.FromString("null"), subject: SagaEventNames.PaymentApproved);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => PaymentResultReceiver.RunAsync(message, client.Object, TestContext.Current.CancellationToken));
    }

    private static ServiceBusReceivedMessage Message(string subject, PaymentResultEvent payment) =>
        ServiceBusModelFactory.ServiceBusReceivedMessage(body: BinaryData.FromString(JsonSerializer.Serialize(payment)), subject: subject);
}
