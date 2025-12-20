using AdventureWorks.Application.Features.Sales.Saga.Models;
using AdventureWorks.Domain.Entities.Production;
using AdventureWorks.Functions.SalesOrderSaga.Activities;
using AdventureWorks.Functions.SalesOrderSaga.Functions;
using AdventureWorks.Functions.SalesOrderSaga.Models;
using AdventureWorks.Functions.SalesOrderSaga.Persistence;
using Microsoft.DurableTask;
using Microsoft.DurableTask.Client;
using Microsoft.DurableTask.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace AdventureWorks.Functions.SalesOrderSaga.OrchestrationTests;

/// <summary>
/// Drives <see cref="SalesOrderSagaOrchestratorCore"/> and <see cref="CheckInventorySubOrchestratorCore"/>
/// through the real in-memory Durable Task engine (<c>Microsoft.DurableTask.InProcessTestHost</c>)
/// rather than a mocked <see cref="TaskOrchestrationContext"/> — this is the higher-fidelity
/// layer called out in the US 807 plan; <c>SalesOrderSagaOrchestratorTests</c> and
/// <c>CheckInventorySubOrchestratorTests</c> (Moq-based, in the sibling
/// <c>AdventureWorks.Functions.SalesOrderSaga.UnitTests</c> project) remain the required,
/// primary coverage. <see cref="ValidateOrderActivityCore"/> and <see cref="ReserveStockActivityCore"/>
/// are registered as real activities here too so the full orchestration graph executes
/// end-to-end; only <see cref="ReserveStockActivityCore"/> touches a (test-only, EF InMemory)
/// database.
/// </summary>
public sealed class SalesOrderSagaOrchestratorInProcessTests
{
    private static SalesOrderSagaInput ValidInput(params SalesOrderSagaLineItem[] lines) => new()
    {
        SalesOrderId = 71774,
        CustomerId = 29825,
        OrderDate = new DateTimeOffset(2026, 7, 1, 0, 0, 0, TimeSpan.Zero),
        Lines = lines
    };

    private static SalesOrderSagaDbContext CreateInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<SalesOrderSagaDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        return new SalesOrderSagaDbContext(options);
    }

    private static async Task<OrchestrationMetadata> RunOrchestrationAsync(SalesOrderSagaInput input, SalesOrderSagaDbContext dbContext)
    {
        await using var host = await DurableTaskTestHost.StartAsync(registry =>
        {
            registry.AddOrchestrator(nameof(SalesOrderSagaOrchestrator), new SalesOrderSagaOrchestratorCore());
            registry.AddOrchestrator(nameof(CheckInventorySubOrchestrator), new CheckInventorySubOrchestratorCore());
            registry.AddActivity(nameof(ValidateOrderActivity), new ValidateOrderActivityCore());
            registry.AddActivity(nameof(CheckInventoryActivity), new CheckInventoryActivityCore(dbContext));
            registry.AddActivity(nameof(ReserveStockActivity), new ReserveStockActivityCore(dbContext));
        });

        var instanceId = await host.Client.ScheduleNewOrchestrationInstanceAsync(nameof(SalesOrderSagaOrchestrator), input);

        return await host.Client.WaitForInstanceCompletionAsync(instanceId, getInputsAndOutputs: true, TestContextTimeout());
    }

    private static CancellationToken TestContextTimeout() => new CancellationTokenSource(TimeSpan.FromSeconds(30)).Token;

    [Fact]
    public async Task Orchestration_ReturnsValidationFailed_WhenInputHasNoLineItems()
    {
        using var dbContext = CreateInMemoryDbContext();
        var input = ValidInput(); // no lines -> Rule-02

        var metadata = await RunOrchestrationAsync(input, dbContext);

        Assert.Equal(OrchestrationRuntimeStatus.Completed, metadata.RuntimeStatus);
        var result = metadata.ReadOutputAs<SalesOrderSagaResult>();
        Assert.NotNull(result);
        Assert.Equal(SalesOrderSagaStatus.ValidationFailed, result.Status);
    }

    [Fact]
    public async Task Orchestration_ReturnsInsufficientStock_WhenNoInventoryExistsForProduct()
    {
        using var dbContext = CreateInMemoryDbContext();
        var input = ValidInput(new SalesOrderSagaLineItem { ProductId = 853, OrderQty = 1, UnitPrice = 10m });

        var metadata = await RunOrchestrationAsync(input, dbContext);

        Assert.Equal(OrchestrationRuntimeStatus.Completed, metadata.RuntimeStatus);
        var result = metadata.ReadOutputAs<SalesOrderSagaResult>();
        Assert.NotNull(result);
        Assert.Equal(SalesOrderSagaStatus.InsufficientStock, result.Status);
    }

    [Fact]
    public async Task Orchestration_ReservesStock_WhenValidationAndInventoryBothSucceed()
    {
        using var dbContext = CreateInMemoryDbContext();
        dbContext.ProductInventories.Add(new ProductInventory
        {
            ProductId = 1, LocationId = 1, Shelf = "A", Bin = 1, Quantity = 1085
        });
        await dbContext.SaveChangesAsync();

        var input = ValidInput(new SalesOrderSagaLineItem { ProductId = 1, OrderQty = 5, UnitPrice = 34.99m });

        var metadata = await RunOrchestrationAsync(input, dbContext);

        Assert.Equal(OrchestrationRuntimeStatus.Completed, metadata.RuntimeStatus);
        var result = metadata.ReadOutputAs<SalesOrderSagaResult>();
        Assert.NotNull(result);
        Assert.Equal(SalesOrderSagaStatus.Reserved, result.Status);
        Assert.NotNull(result.Receipt);
        Assert.Equal([1], result.Receipt!.ReservedProductIds);
    }
}
