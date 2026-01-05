using System.Text.Json;
using AdventureWorks.Application.Features.Sales.Saga.Models;
using AdventureWorks.SalesOrderSaga.Models;
using AdventureWorks.SalesOrderSaga.Contracts;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask;
using Microsoft.DurableTask.Client;
using Microsoft.Extensions.Logging;
using Azure.Messaging.ServiceBus;

namespace AdventureWorks.SalesOrderSaga.Functions;

/// <summary>
/// Service Bus-triggered entry point for the sales order saga. Deserializes the
/// <c>OrderCreated</c> event and starts (or no-ops against an already-running) orchestration
/// instance with a deterministic instance ID derived from the order ID, so a re-delivered
/// message never starts a duplicate saga for the same order.
/// </summary>
public sealed class SalesOrderSagaStarter
{
    private readonly ILogger<SalesOrderSagaStarter> _logger;

    public SalesOrderSagaStarter(ILogger<SalesOrderSagaStarter> logger)
    {
        ArgumentNullException.ThrowIfNull(logger);
        _logger = logger;
    }

    /// <summary>
    /// Builds the deterministic Durable Functions instance ID for a given sales order.
    /// Exposed for unit testing and for future stories that need to query saga status by order ID.
    /// </summary>
    public static string BuildInstanceId(int salesOrderId) => SagaIds.InstanceIdFor(salesOrderId);

    /// <summary>Ignores non-OrderCreated topic messages before starting a durable instance.</summary>
    [Function(nameof(SalesOrderSagaStarter))]
    public Task RunMessageAsync(
        [ServiceBusTrigger("%ServiceBusSalesOrderEventsTopicName%", "%ServiceBusSalesOrderSagaSubscriptionName%", Connection = "ServiceBusConnection")] ServiceBusReceivedMessage message,
        [DurableClient] DurableTaskClient durableTaskClient,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(message);
        return message.Subject == SagaEventNames.OrderCreated
            ? RunAsync(message.Body.ToString(), durableTaskClient, cancellationToken)
            : Task.CompletedTask;
    }

    /// <summary>Deserializes an OrderCreated payload and starts its saga; exposed for unit tests.</summary>
    public async Task RunAsync(
        string message,
        DurableTaskClient durableTaskClient,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(durableTaskClient);

        var orderCreated = JsonSerializer.Deserialize<OrderCreatedEvent>(message)
            ?? throw new InvalidOperationException("OrderCreated message deserialized to null.");
        var input = new SalesOrderSagaInput
        {
            SalesOrderId = orderCreated.SalesOrderId,
            CustomerId = orderCreated.CustomerId,
            OrderDate = orderCreated.OrderDate,
            Lines = orderCreated.Lines.Select(line => new SalesOrderSagaLineItem
            {
                ProductId = line.ProductId,
                OrderQty = line.OrderQty,
                UnitPrice = line.UnitPrice
            }).ToArray()
        };

        var instanceId = BuildInstanceId(input.SalesOrderId);

        var existing = await durableTaskClient.GetInstanceAsync(instanceId, getInputsAndOutputs: false, cancellationToken);
        if (existing is { RuntimeStatus: OrchestrationRuntimeStatus.Running or OrchestrationRuntimeStatus.Pending })
        {
            _logger.LogInformation(
                "Sales order saga {InstanceId} is already {RuntimeStatus}; skipping duplicate OrderCreated delivery for SalesOrderId {SalesOrderId}.",
                instanceId, existing.RuntimeStatus, input.SalesOrderId);
            return;
        }

        await durableTaskClient.ScheduleNewOrchestrationInstanceAsync(
            nameof(SalesOrderSagaOrchestrator),
            input,
            new StartOrchestrationOptions { InstanceId = instanceId },
            cancellationToken);

        _logger.LogInformation(
            "Started sales order saga {InstanceId} for SalesOrderId {SalesOrderId}.", instanceId, input.SalesOrderId);
    }
}
