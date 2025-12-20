using AdventureWorks.Application.Features.Sales.Saga.Models;
using AdventureWorks.Functions.SalesOrderSaga.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask;

namespace AdventureWorks.Functions.SalesOrderSaga.Activities;

/// <summary>
/// Core fan-out logic for the sales order saga's inventory-check step. Calls
/// <c>CheckInventoryActivity</c> once per order line via <see cref="Task.WhenAll{TResult}"/> and
/// waits for every line to resolve before consolidating into a single
/// <see cref="CheckInventoryResult"/> — e.g. 5 line items produce 5 parallel activity calls.
/// Kept as a plain <see cref="TaskOrchestrator{TInput,TResult}"/> (no direct I/O) so it can be
/// driven directly by <c>Microsoft.DurableTask.InProcessTestHost</c> in addition to Moq.
/// </summary>
public sealed class CheckInventorySubOrchestratorCore : TaskOrchestrator<SalesOrderSagaInput, CheckInventoryResult>
{
    public override async Task<CheckInventoryResult> RunAsync(TaskOrchestrationContext context, SalesOrderSagaInput input)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(input);

        var checks = input.Lines
            .Select(line => context.CallActivityAsync<LineItemAvailability>(nameof(CheckInventoryActivity), line))
            .ToArray();

        var lines = await Task.WhenAll(checks);

        return new CheckInventoryResult(lines);
    }
}

/// <summary>
/// Thin <c>[Function]</c> adapter for <see cref="CheckInventorySubOrchestratorCore"/>. Called by
/// <c>SalesOrderSagaOrchestrator</c> as a sub-orchestration after order validation succeeds.
/// </summary>
public static class CheckInventorySubOrchestrator
{
    [Function(nameof(CheckInventorySubOrchestrator))]
    public static Task<CheckInventoryResult> RunAsync([OrchestrationTrigger] TaskOrchestrationContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var input = context.GetInput<SalesOrderSagaInput>()
            ?? throw new InvalidOperationException("CheckInventorySubOrchestrator started without input.");

        return new CheckInventorySubOrchestratorCore().RunAsync(context, input);
    }
}
