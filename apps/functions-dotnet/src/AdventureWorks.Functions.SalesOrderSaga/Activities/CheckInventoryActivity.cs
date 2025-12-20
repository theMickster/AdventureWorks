using AdventureWorks.Application.Features.Sales.Saga.Models;
using AdventureWorks.Functions.SalesOrderSaga.Models;
using AdventureWorks.Functions.SalesOrderSaga.Persistence;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask;
using Microsoft.EntityFrameworkCore;

namespace AdventureWorks.Functions.SalesOrderSaga.Activities;

/// <summary>
/// Core inventory-check logic for a single order line — queries
/// <c>Production.ProductInventory.Quantity</c> summed across every location for the line's
/// <c>ProductId</c> and compares it to the requested <c>OrderQty</c>. Mirrors
/// <c>apps/api-dotnet</c>'s <c>ProductRepository.GetProductInventoryByProductIdAsync</c>
/// <c>.AsNoTracking()</c> read pattern. Fanned out per line item by
/// <c>CheckInventorySubOrchestrator</c> via <c>Task.WhenAll</c>.
/// </summary>
public sealed class CheckInventoryActivityCore(SalesOrderSagaDbContext dbContext) : TaskActivity<SalesOrderSagaLineItem, LineItemAvailability>
{
    public override async Task<LineItemAvailability> RunAsync(TaskActivityContext context, SalesOrderSagaLineItem? input)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(input);

        var availableQuantity = await dbContext.ProductInventories
            .AsNoTracking()
            .Where(pi => pi.ProductId == input.ProductId)
            .SumAsync(pi => (int)pi.Quantity);

        return new LineItemAvailability(input.ProductId, input.OrderQty, availableQuantity);
    }
}

/// <summary>
/// Thin <c>[Function]</c> adapter for <see cref="CheckInventoryActivityCore"/>. Registered per
/// order line — <c>CheckInventorySubOrchestrator</c> calls this once per line item.
/// </summary>
public sealed class CheckInventoryActivity(SalesOrderSagaDbContext dbContext)
{
    [Function(nameof(CheckInventoryActivity))]
    public Task<LineItemAvailability> RunAsync(
        [ActivityTrigger] SalesOrderSagaLineItem input, TaskActivityContext context) =>
        new CheckInventoryActivityCore(dbContext).RunAsync(context, input);
}
