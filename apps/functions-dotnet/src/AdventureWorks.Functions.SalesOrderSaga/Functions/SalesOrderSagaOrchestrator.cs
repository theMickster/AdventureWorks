using AdventureWorks.Functions.SalesOrderSaga.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask;
using Microsoft.Extensions.Logging;

namespace AdventureWorks.Functions.SalesOrderSaga.Functions;

/// <summary>
/// Orchestrator stub for the sales order saga (US 806 scope only). Confirms the durable
/// runtime can start and complete an instance end-to-end. Real activities — inventory
/// validation, stock reservation, payment authorization, and compensation — are added by
/// stories 807-810; this stub intentionally does not call any SQL-touching activity.
/// </summary>
public static class SalesOrderSagaOrchestrator
{
    [Function(nameof(SalesOrderSagaOrchestrator))]
    public static Task<string> RunAsync([OrchestrationTrigger] TaskOrchestrationContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var input = context.GetInput<SalesOrderSagaInput>()
            ?? throw new InvalidOperationException("Sales order saga orchestrator started without input.");

        var logger = context.CreateReplaySafeLogger(nameof(SalesOrderSagaOrchestrator));
        logger.LogInformation(
            "Sales order saga orchestration accepted for SalesOrderId {SalesOrderId} with {LineCount} line item(s).",
            input.SalesOrderId, input.Lines.Count);

        return Task.FromResult($"Accepted-{input.SalesOrderId}");
    }
}
