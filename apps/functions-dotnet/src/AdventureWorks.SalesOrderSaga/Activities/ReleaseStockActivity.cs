using AdventureWorks.Domain.Entities.Production;
using System.Data;
using System.Text.Json;
using AdventureWorks.SalesOrderSaga.Models;
using AdventureWorks.SalesOrderSaga.Contracts;
using AdventureWorks.SalesOrderSaga.Persistence;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask;
using Microsoft.EntityFrameworkCore;

namespace AdventureWorks.SalesOrderSaga.Activities;

/// <summary>Restores the exact inventory allocations made by a failed sales-order saga.</summary>
public sealed class ReleaseStockActivityCore(SalesOrderSagaDbContext dbContext) : TaskActivity<ReleaseStockRequest, ReleaseStockResult>
{
    public override async Task<ReleaseStockResult> RunAsync(TaskActivityContext context, ReleaseStockRequest? input)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(input);

        // Serializable isolation prevents two at-least-once activity executions from both seeing
        // unreversed rows and releasing the same allocation twice.
        await using var transaction = await dbContext.Database.BeginTransactionAsync(IsolationLevel.Serializable, CancellationToken.None);
        var allocations = await dbContext.InventoryAllocations
            .Where(a => a.SagaInstanceId == input.SagaInstanceId && a.ReversedAt == null)
            .ToListAsync();

        if (allocations.Count == 0)
        {
            return new ReleaseStockResult(AlreadyCompensated: true, ReleasedAllocationCount: 0);
        }

        foreach (var allocation in allocations)
        {
            var inventory = await dbContext.ProductInventories.SingleOrDefaultAsync(
                p => p.ProductId == allocation.ProductId && p.LocationId == allocation.LocationId);
            if (inventory is null)
            {
                throw new InvalidOperationException($"Inventory allocation {allocation.ProductId}/{allocation.LocationId} no longer exists.");
            }

            inventory.Quantity += allocation.Quantity;
            inventory.Rowguid = Guid.NewGuid();
            allocation.ReversedAt = DateTime.UtcNow;

            // TransactionHistory's nchar(1) TransactionType cannot store the requested SR
            // literal. S denotes a sales-order reversal; the allocation ledger identifies it.
            dbContext.TransactionHistories.Add(new TransactionHistory
            {
                ProductId = allocation.ProductId,
                ReferenceOrderId = input.SalesOrderId,
                ReferenceOrderLineId = allocation.LocationId,
                TransactionDate = DateTime.UtcNow,
                TransactionType = TransactionHistoryConstants.TransactionTypeSalesOrder,
                Quantity = allocation.Quantity,
                ActualCost = 0m,
                ModifiedDate = DateTime.UtcNow
            });
        }

        var failureMessageId = $"{SagaEventNames.OrderFailed}:{input.SagaInstanceId}";
        if (!await dbContext.OutboxMessages.AnyAsync(message => message.MessageId == failureMessageId, CancellationToken.None))
        {
            dbContext.OutboxMessages.Add(new SalesOrderSagaOutboxMessage
            {
                MessageId = failureMessageId,
                EventName = SagaEventNames.OrderFailed,
                Payload = JsonSerializer.Serialize(new OrderFailedEvent(input.SalesOrderId, input.FailureStatus.ToString(), input.FailureReason)),
                OccurredAt = DateTime.UtcNow
            });
        }

        await dbContext.SaveChangesAsync(CancellationToken.None);
        await transaction.CommitAsync(CancellationToken.None);
        return new ReleaseStockResult(AlreadyCompensated: false, allocations.Count);
    }
}

/// <summary>Functions adapter for <see cref="ReleaseStockActivityCore"/>.</summary>
public sealed class ReleaseStockActivity(SalesOrderSagaDbContext dbContext)
{
    [Function(nameof(ReleaseStockActivity))]
    public Task<ReleaseStockResult> RunAsync([ActivityTrigger] ReleaseStockRequest input) =>
        new ReleaseStockActivityCore(dbContext).RunAsync(new FunctionsTaskActivityContext(nameof(ReleaseStockActivity)), input);
}
