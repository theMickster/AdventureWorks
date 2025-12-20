using AdventureWorks.Application.Features.Sales.Saga.Models;
using AdventureWorks.Functions.SalesOrderSaga.Activities;
using AdventureWorks.Functions.SalesOrderSaga.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask;
using Microsoft.Extensions.Logging;

namespace AdventureWorks.Functions.SalesOrderSaga.Functions;

/// <summary>
/// Core orchestration logic for the sales order saga (US 807 scope): validate the order, check
/// inventory for every line via <c>CheckInventorySubOrchestrator</c>, then reserve stock.
/// Short-circuits after validation or inventory-check failure — <c>ReserveStockActivity</c> is
/// only called once every line is confirmed available. Payment authorization and compensation
/// are added by stories 808-810. Kept as a plain <see cref="TaskOrchestrator{TInput,TResult}"/>
/// (no direct I/O; only orchestration-context calls) so it can be driven directly by
/// <c>Microsoft.DurableTask.InProcessTestHost</c> in addition to Moq.
/// </summary>
public sealed class SalesOrderSagaOrchestratorCore : TaskOrchestrator<SalesOrderSagaInput, SalesOrderSagaResult>
{
    public override async Task<SalesOrderSagaResult> RunAsync(TaskOrchestrationContext context, SalesOrderSagaInput input)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(input);

        var logger = context.CreateReplaySafeLogger(nameof(SalesOrderSagaOrchestratorCore));
        logger.LogInformation(
            "Sales order saga orchestration accepted for SalesOrderId {SalesOrderId} with {LineCount} line item(s).",
            input.SalesOrderId, input.Lines.Count);

        var validation = await context.CallActivityAsync<ValidateOrderResult>(nameof(ValidateOrderActivity), input);
        if (!validation.IsValid)
        {
            logger.LogWarning(
                "Sales order saga {SalesOrderId} failed validation: {Errors}",
                input.SalesOrderId, string.Join("; ", validation.Errors));
            return SalesOrderSagaResult.ValidationFailed(input.SalesOrderId, validation.Errors);
        }

        var inventory = await context.CallSubOrchestratorAsync<CheckInventoryResult>(nameof(CheckInventorySubOrchestrator), input);
        if (!inventory.AllAvailable)
        {
            logger.LogWarning("Sales order saga {SalesOrderId} has insufficient stock for one or more line items.", input.SalesOrderId);
            return SalesOrderSagaResult.InsufficientStock(input.SalesOrderId, inventory.Lines);
        }

        var receipt = await context.CallActivityAsync<ReservationReceipt>(nameof(ReserveStockActivity), input);
        logger.LogInformation("Sales order saga {SalesOrderId} reserved stock successfully.", input.SalesOrderId);

        return SalesOrderSagaResult.Reserved(input.SalesOrderId, receipt);
    }
}

/// <summary>
/// Thin <c>[Function]</c> adapter for <see cref="SalesOrderSagaOrchestratorCore"/>. Real
/// activities — inventory validation, stock reservation — were added by US 807; payment
/// authorization and compensation are added by stories 808-810.
/// </summary>
public static class SalesOrderSagaOrchestrator
{
    [Function(nameof(SalesOrderSagaOrchestrator))]
    public static Task<SalesOrderSagaResult> RunAsync([OrchestrationTrigger] TaskOrchestrationContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var input = context.GetInput<SalesOrderSagaInput>()
            ?? throw new InvalidOperationException("Sales order saga orchestrator started without input.");

        return new SalesOrderSagaOrchestratorCore().RunAsync(context, input);
    }
}
