using AdventureWorks.Application.Features.Sales.Saga.Models;
using AdventureWorks.Domain.Entities.Production;
using AdventureWorks.Functions.SalesOrderSaga.Activities;
using AdventureWorks.Functions.SalesOrderSaga.Persistence;
using AdventureWorks.Functions.SalesOrderSaga.UnitTests.Persistence;
using Microsoft.DurableTask;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace AdventureWorks.Functions.SalesOrderSaga.UnitTests.Activities;

public class ReserveStockActivityTests
{
    private static SalesOrderSagaInput Input(params SalesOrderSagaLineItem[] lines) => new()
    {
        SalesOrderId = 71774,
        CustomerId = 29825,
        OrderDate = new DateTimeOffset(2026, 7, 1, 0, 0, 0, TimeSpan.Zero),
        Lines = lines
    };

    [Fact]
    public async Task RunAsync_DecrementsInventoryAndInsertsTransactionHistory_ForEachLine()
    {
        using var dbContext = SalesOrderSagaDbContextFactory.Create();
        dbContext.ProductInventories.AddRange(
            new ProductInventory { ProductId = 1, LocationId = 1, Shelf = "A", Bin = 1, Quantity = 400 },
            new ProductInventory { ProductId = 1, LocationId = 2, Shelf = "B", Bin = 2, Quantity = 485 });
        await dbContext.SaveChangesAsync();

        var context = new Mock<TaskActivityContext>(MockBehavior.Strict);
        var input = Input(new SalesOrderSagaLineItem { ProductId = 1, OrderQty = 450, UnitPrice = 10m });

        var receipt = await new ReserveStockActivityCore(dbContext).RunAsync(context.Object, input);

        Assert.Equal(71774, receipt.SalesOrderId);
        Assert.Equal([1], receipt.ReservedProductIds);

        var remainingQuantity = await dbContext.ProductInventories
            .Where(pi => pi.ProductId == 1)
            .SumAsync(pi => (int)pi.Quantity);
        Assert.Equal(885 - 450, remainingQuantity);

        var transaction = await dbContext.TransactionHistories.SingleAsync(th => th.ProductId == 1);
        Assert.Equal("S", transaction.TransactionType);
        Assert.Equal(71774, transaction.ReferenceOrderId);
        Assert.Equal(450, transaction.Quantity);
    }

    [Fact]
    public async Task RunAsync_DecrementsAcrossMultipleLocations_WhenSingleLocationCannotCoverOrderQty()
    {
        using var dbContext = SalesOrderSagaDbContextFactory.Create();
        dbContext.ProductInventories.AddRange(
            new ProductInventory { ProductId = 1, LocationId = 1, Shelf = "A", Bin = 1, Quantity = 100 },
            new ProductInventory { ProductId = 1, LocationId = 2, Shelf = "B", Bin = 2, Quantity = 100 });
        await dbContext.SaveChangesAsync();

        var context = new Mock<TaskActivityContext>(MockBehavior.Strict);
        var input = Input(new SalesOrderSagaLineItem { ProductId = 1, OrderQty = 150, UnitPrice = 10m });

        await new ReserveStockActivityCore(dbContext).RunAsync(context.Object, input);

        var locationOne = await dbContext.ProductInventories.SingleAsync(pi => pi.ProductId == 1 && pi.LocationId == 1);
        var locationTwo = await dbContext.ProductInventories.SingleAsync(pi => pi.ProductId == 1 && pi.LocationId == 2);
        Assert.Equal(0, locationOne.Quantity);
        Assert.Equal(50, locationTwo.Quantity);
    }

    [Fact]
    public async Task RunAsync_Throws_WhenTrackedStockCannotCoverOrderQty()
    {
        using var dbContext = SalesOrderSagaDbContextFactory.Create();
        dbContext.ProductInventories.Add(new ProductInventory { ProductId = 853, LocationId = 1, Shelf = "A", Bin = 1, Quantity = 0 });
        await dbContext.SaveChangesAsync();

        var context = new Mock<TaskActivityContext>(MockBehavior.Strict);
        var input = Input(new SalesOrderSagaLineItem { ProductId = 853, OrderQty = 1, UnitPrice = 10m });

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => new ReserveStockActivityCore(dbContext).RunAsync(context.Object, input));
    }

    [Fact]
    public async Task RunAsync_Throws_WhenAnotherInstanceConcurrentlyReservedTheSameRow()
    {
        // Simulates two saga instances racing to reserve the same ProductId: dbContextA and
        // dbContextB are separate SalesOrderSagaDbContext instances (as two concurrent activity
        // invocations would each get their own) sharing the same underlying store. dbContextA
        // reserves first and commits; dbContextB — which read the row before dbContextA's
        // commit — must fail loudly on its own commit rather than silently overwrite dbContextA's
        // decrement (the lost-update scenario ProductInventory.Rowguid-as-concurrency-token
        // exists to prevent).
        var databaseName = Guid.NewGuid().ToString();
        using var dbContextA = SalesOrderSagaDbContextFactory.Create(databaseName);
        dbContextA.ProductInventories.Add(new ProductInventory { ProductId = 1, LocationId = 1, Shelf = "A", Bin = 1, Quantity = 100 });
        await dbContextA.SaveChangesAsync();

        using var dbContextB = SalesOrderSagaDbContextFactory.Create(databaseName);
        // Force dbContextB to read the pre-reservation row into its own change tracker before
        // dbContextA reserves, mirroring two concurrent activity invocations that both read the
        // row before either commits a decrement.
        _ = await dbContextB.ProductInventories.SingleAsync(pi => pi.ProductId == 1);

        var context = new Mock<TaskActivityContext>(MockBehavior.Strict);
        var line = new SalesOrderSagaLineItem { ProductId = 1, OrderQty = 40, UnitPrice = 10m };
        // Two distinct SalesOrderIds — this is two different orders racing on the same
        // ProductId, not a replay of the same order (which the Rule-01/idempotency
        // short-circuit in RunAsync would otherwise intercept before the decrement runs).
        var inputA = new SalesOrderSagaInput { SalesOrderId = 71774, CustomerId = 29825, OrderDate = DateTimeOffset.UtcNow, Lines = [line] };
        var inputB = new SalesOrderSagaInput { SalesOrderId = 71775, CustomerId = 29825, OrderDate = DateTimeOffset.UtcNow, Lines = [line] };

        await new ReserveStockActivityCore(dbContextA).RunAsync(context.Object, inputA);

        await Assert.ThrowsAsync<DbUpdateConcurrencyException>(
            () => new ReserveStockActivityCore(dbContextB).RunAsync(context.Object, inputB));
    }

    [Fact]
    public async Task RunAsync_ShortCircuitsOnReplay_WhenTransactionHistoryAlreadyExistsForOrder()
    {
        using var dbContext = SalesOrderSagaDbContextFactory.Create();
        dbContext.ProductInventories.Add(
            new ProductInventory { ProductId = 1, LocationId = 1, Shelf = "A", Bin = 1, Quantity = 400 });
        await dbContext.SaveChangesAsync();

        var context = new Mock<TaskActivityContext>(MockBehavior.Strict);
        var input = Input(new SalesOrderSagaLineItem { ProductId = 1, OrderQty = 50, UnitPrice = 10m });

        var firstReceipt = await new ReserveStockActivityCore(dbContext).RunAsync(context.Object, input);

        var inventoryAfterFirstRun = await dbContext.ProductInventories
            .Where(pi => pi.ProductId == 1)
            .SumAsync(pi => (int)pi.Quantity);
        var transactionCountAfterFirstRun = await dbContext.TransactionHistories.CountAsync(th => th.ProductId == 1);

        var replayReceipt = await new ReserveStockActivityCore(dbContext).RunAsync(context.Object, input);

        var inventoryAfterReplay = await dbContext.ProductInventories
            .Where(pi => pi.ProductId == 1)
            .SumAsync(pi => (int)pi.Quantity);
        var transactionCountAfterReplay = await dbContext.TransactionHistories.CountAsync(th => th.ProductId == 1);

        Assert.Equal(inventoryAfterFirstRun, inventoryAfterReplay);
        Assert.Equal(transactionCountAfterFirstRun, transactionCountAfterReplay);
        Assert.Equal(1, transactionCountAfterReplay);
        Assert.Equal(firstReceipt.SalesOrderId, replayReceipt.SalesOrderId);
        Assert.Equal(firstReceipt.ReservedProductIds, replayReceipt.ReservedProductIds);
    }

    [Fact]
    public async Task ActivityAdapter_DelegatesTo_Core()
    {
        using var dbContext = SalesOrderSagaDbContextFactory.Create();
        dbContext.ProductInventories.Add(new ProductInventory { ProductId = 316, LocationId = 1, Shelf = "A", Bin = 1, Quantity = 1361 });
        await dbContext.SaveChangesAsync();

        var input = Input(new SalesOrderSagaLineItem { ProductId = 316, OrderQty = 5, UnitPrice = 10m });

        var receipt = await new ReserveStockActivity(dbContext).RunAsync(input);

        Assert.Equal([316], receipt.ReservedProductIds);
    }
}
