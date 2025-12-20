using AdventureWorks.Application.Features.Sales.Saga.Models;
using AdventureWorks.Domain.Entities.Production;
using AdventureWorks.Functions.SalesOrderSaga.Activities;
using AdventureWorks.Functions.SalesOrderSaga.UnitTests.Persistence;
using Microsoft.DurableTask;
using Moq;

namespace AdventureWorks.Functions.SalesOrderSaga.UnitTests.Activities;

/// <summary>
/// ProductId 1's seeded rows/totals mirror the real AdventureWorks database (3 locations
/// summing to 1085 units), verified via the <c>querying-adventureworks-database</c> skill.
/// ProductId 853 mirrors a real AdventureWorks product with zero units on hand.
/// </summary>
public class CheckInventoryActivityTests
{
    [Fact]
    public async Task RunAsync_ReturnsAvailable_WhenSummedQuantityAcrossLocationsMeetsOrderQty()
    {
        using var dbContext = SalesOrderSagaDbContextFactory.Create();
        dbContext.ProductInventories.AddRange(
            new ProductInventory { ProductId = 1, LocationId = 1, Shelf = "A", Bin = 1, Quantity = 400 },
            new ProductInventory { ProductId = 1, LocationId = 2, Shelf = "B", Bin = 2, Quantity = 485 },
            new ProductInventory { ProductId = 1, LocationId = 3, Shelf = "C", Bin = 3, Quantity = 200 });
        await dbContext.SaveChangesAsync();

        var context = new Mock<TaskActivityContext>(MockBehavior.Strict);
        var line = new SalesOrderSagaLineItem { ProductId = 1, OrderQty = 1000, UnitPrice = 10m };

        var result = await new CheckInventoryActivityCore(dbContext).RunAsync(context.Object, line);

        Assert.Equal(1, result.ProductId);
        Assert.Equal(1085, result.AvailableQuantity);
        Assert.True(result.IsAvailable);
    }

    [Fact]
    public async Task RunAsync_ReturnsUnavailable_WhenProductHasNoStock()
    {
        using var dbContext = SalesOrderSagaDbContextFactory.Create();
        var context = new Mock<TaskActivityContext>(MockBehavior.Strict);
        var line = new SalesOrderSagaLineItem { ProductId = 853, OrderQty = 1, UnitPrice = 10m };

        var result = await new CheckInventoryActivityCore(dbContext).RunAsync(context.Object, line);

        Assert.Equal(0, result.AvailableQuantity);
        Assert.False(result.IsAvailable);
    }

    [Fact]
    public async Task ActivityAdapter_DelegatesTo_Core()
    {
        using var dbContext = SalesOrderSagaDbContextFactory.Create();
        dbContext.ProductInventories.Add(new ProductInventory { ProductId = 316, LocationId = 1, Shelf = "A", Bin = 1, Quantity = 1361 });
        await dbContext.SaveChangesAsync();

        var line = new SalesOrderSagaLineItem { ProductId = 316, OrderQty = 10, UnitPrice = 10m };

        var result = await new CheckInventoryActivity(dbContext).RunAsync(line);

        Assert.True(result.IsAvailable);
    }
}
