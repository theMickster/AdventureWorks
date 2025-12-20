using System.Text.Json;
using AdventureWorks.Functions.SalesOrderSaga.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask;
using Microsoft.DurableTask.Client;
using Microsoft.Extensions.Logging;

namespace AdventureWorks.Functions.SalesOrderSaga.Functions;

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
    public static string BuildInstanceId(int salesOrderId) => $"sales-order-saga-{salesOrderId}";

    [Function(nameof(SalesOrderSagaStarter))]
    public async Task RunAsync(
        [ServiceBusTrigger("%ServiceBusSalesOrderEventsTopicName%", "%ServiceBusSalesOrderSagaSubscriptionName%", Connection = "ServiceBusConnection")] string message,
        [DurableClient] DurableTaskClient durableTaskClient,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(durableTaskClient);

        var input = JsonSerializer.Deserialize<SalesOrderSagaInput>(message)
            ?? throw new InvalidOperationException("OrderCreated message deserialized to null.");

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
