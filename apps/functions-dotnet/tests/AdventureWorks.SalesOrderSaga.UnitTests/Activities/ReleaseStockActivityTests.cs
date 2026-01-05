using AdventureWorks.Domain.Entities.Production;
using AdventureWorks.SalesOrderSaga.Activities;
using AdventureWorks.SalesOrderSaga.Models;
using AdventureWorks.SalesOrderSaga.Contracts;
using AdventureWorks.SalesOrderSaga.Persistence;
using AdventureWorks.SalesOrderSaga.UnitTests.Persistence;
using Microsoft.DurableTask;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace AdventureWorks.SalesOrderSaga.UnitTests.Activities;

/// <summary>Regression coverage for exact, replay-safe inventory compensation.</summary>
public sealed class ReleaseStockActivityTests
{
    [Fact]
    public async Task RunAsync_RestoresTheRecordedLocationOnce_ThenShortCircuitsOnReplay()
    {
        using var dbContext = SalesOrderSagaDbContextFactory.Create();
        const string instanceId = "sales-order-saga-71774";
        dbContext.ProductInventories.Add(new ProductInventory
        {
            ProductId = 1, LocationId = 2, Shelf = "A", Bin = 1, Quantity = 7
        });
        dbContext.InventoryAllocations.Add(new SalesOrderSagaInventoryAllocation
        {
            SagaInstanceId = instanceId, SalesOrderId = 71774, LineNumber = 1,
            ProductId = 1, LocationId = 2, Quantity = 3, ReservedAt = DateTime.UtcNow
        });
        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        var activity = new ReleaseStockActivityCore(dbContext);
        var context = new Mock<TaskActivityContext>(MockBehavior.Strict);
        var request = new ReleaseStockRequest(71774, instanceId, SalesOrderSagaStatus.PaymentDeclined, "declined");

        var first = await activity.RunAsync(context.Object, request);
        var replay = await activity.RunAsync(context.Object, request);

        Assert.False(first.AlreadyCompensated);
        Assert.Equal(1, first.ReleasedAllocationCount);
        Assert.True(replay.AlreadyCompensated);
        Assert.Equal(10, (await dbContext.ProductInventories.SingleAsync(TestContext.Current.CancellationToken)).Quantity);
        Assert.NotNull((await dbContext.InventoryAllocations.SingleAsync(TestContext.Current.CancellationToken)).ReversedAt);
        Assert.Single(await dbContext.TransactionHistories.ToListAsync(TestContext.Current.CancellationToken));
    }
}
