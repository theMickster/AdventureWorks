using System.Text.Json;
using AdventureWorks.SalesOrderSaga.Models;
using AdventureWorks.SalesOrderSaga.Contracts;
using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask.Client;

namespace AdventureWorks.SalesOrderSaga.Functions;

/// <summary>Correlates payment-result topic messages with the deterministic durable saga instance.</summary>
public static class PaymentResultReceiver
{
    [Function(nameof(PaymentResultReceiver))]
    public static async Task RunAsync(
        [ServiceBusTrigger("%ServiceBusSalesOrderEventsTopicName%", "%ServiceBusSalesOrderPaymentSubscriptionName%", Connection = "ServiceBusConnection")]
        ServiceBusReceivedMessage message,
        [DurableClient] DurableTaskClient durableTaskClient,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(message);
        ArgumentNullException.ThrowIfNull(durableTaskClient);

        var eventName = message.Subject;
        if (eventName is not (SagaEventNames.PaymentApproved or SagaEventNames.PaymentDeclined))
        {
            return;
        }

        var payment = JsonSerializer.Deserialize<PaymentResultEvent>(message.Body)
            ?? throw new InvalidOperationException("Payment-result message deserialized to null.");
        var instanceId = SagaIds.InstanceIdFor(payment.SalesOrderId);
        if (!string.Equals(payment.IdempotencyKey, instanceId, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Payment-result idempotency key does not match its sales-order saga instance.");
        }

        await durableTaskClient.RaiseEventAsync(instanceId, eventName, payment, cancellationToken);
    }
}
