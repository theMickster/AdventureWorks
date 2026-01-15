using System.Text.Json;
using AdventureWorks.Domain.Entities.Person;
using AdventureWorks.Domain.Entities.Purchasing;
using AdventureWorks.Domain.Entities.Sales;
using AdventureWorks.SalesOrderSaga.Activities;
using AdventureWorks.SalesOrderSaga.Contracts;
using AdventureWorks.SalesOrderSaga.Models;
using AdventureWorks.SalesOrderSaga.Persistence;
using AdventureWorks.SalesOrderSaga.UnitTests.Persistence;
using Microsoft.DurableTask;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace AdventureWorks.SalesOrderSaga.UnitTests.Activities;

/// <summary>Coverage for order approval and its transactional outbox record.</summary>
public sealed class ConfirmOrderActivityTests
{
    private const int SalesOrderId = 71774;
    private const string InstanceId = "sales-order-saga-71774";
    private const byte ApprovedStatus = 5;

    private static readonly Mock<TaskActivityContext> Context = new(MockBehavior.Strict);

    [Fact]
    public async Task RunAsync_ApprovesOrderAndEnqueuesOrderApprovedEvent()
    {
        using var dbContext = SalesOrderSagaDbContextFactory.Create();
        await SeedOrderAsync(dbContext, status: 1, revision: 0, includeShipTo: true);
        var activity = new ConfirmOrderActivityCore(dbContext);

        var approval = await activity.RunAsync(Context.Object, new ConfirmOrderRequest(SalesOrderId, InstanceId));

        var order = await dbContext.SalesOrderHeaders.SingleAsync(TestContext.Current.CancellationToken);
        Assert.Equal(ApprovedStatus, order.Status);
        Assert.Equal(1, order.RevisionNumber);
        Assert.NotNull(order.ShipDate);
        Assert.Equal(SalesOrderId, approval.SalesOrderId);
        Assert.Equal(new OrderApprovedLine(707, 2, 34.99m), Assert.Single(approval.Lines));
        Assert.Equal("1 Main St", approval.ShipTo.AddressLine1);
        Assert.Equal("Overnight", approval.ShipMethod.Name);

        var outbox = await dbContext.OutboxMessages.SingleAsync(TestContext.Current.CancellationToken);
        Assert.Equal($"{SagaEventNames.OrderApproved}:{InstanceId}", outbox.MessageId);
        Assert.Equal(SagaEventNames.OrderApproved, outbox.EventName);
        Assert.Null(outbox.DispatchedAt);
        var published = JsonSerializer.Deserialize<OrderApprovedEvent>(outbox.Payload);
        Assert.Equal(SalesOrderId, published!.SalesOrderId);
    }

    [Fact]
    public async Task RunAsync_OnReplay_DoesNotBumpRevisionOrDuplicateOutbox()
    {
        using var dbContext = SalesOrderSagaDbContextFactory.Create();
        await SeedOrderAsync(dbContext, status: 1, revision: 0, includeShipTo: true);
        var activity = new ConfirmOrderActivityCore(dbContext);
        var request = new ConfirmOrderRequest(SalesOrderId, InstanceId);

        await activity.RunAsync(Context.Object, request);
        var firstShipDate = (await dbContext.SalesOrderHeaders.SingleAsync(TestContext.Current.CancellationToken)).ShipDate;
        await activity.RunAsync(Context.Object, request);

        var order = await dbContext.SalesOrderHeaders.SingleAsync(TestContext.Current.CancellationToken);
        Assert.Equal(ApprovedStatus, order.Status);
        Assert.Equal(1, order.RevisionNumber);
        Assert.Equal(firstShipDate, order.ShipDate);
        Assert.Single(await dbContext.OutboxMessages.ToListAsync(TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task RunAsync_WhenOrderDoesNotExist_Throws()
    {
        using var dbContext = SalesOrderSagaDbContextFactory.Create();
        var activity = new ConfirmOrderActivityCore(dbContext);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => activity.RunAsync(Context.Object, new ConfirmOrderRequest(SalesOrderId, InstanceId)));
    }

    [Fact]
    public async Task RunAsync_WhenShipToAddressIsMissing_ThrowsAndWritesNothing()
    {
        using var dbContext = SalesOrderSagaDbContextFactory.Create();
        await SeedOrderAsync(dbContext, status: 1, revision: 0, includeShipTo: false);
        var activity = new ConfirmOrderActivityCore(dbContext);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => activity.RunAsync(Context.Object, new ConfirmOrderRequest(SalesOrderId, InstanceId)));

        Assert.Empty(await dbContext.OutboxMessages.ToListAsync(TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task RunAsync_WhenRequestIsNull_Throws()
    {
        using var dbContext = SalesOrderSagaDbContextFactory.Create();
        var activity = new ConfirmOrderActivityCore(dbContext);

        await Assert.ThrowsAsync<ArgumentNullException>(() => activity.RunAsync(Context.Object, null));
    }

    private static async Task SeedOrderAsync(SalesOrderSagaDbContext dbContext, byte status, byte revision, bool includeShipTo)
    {
        dbContext.ShipMethods.Add(new ShipMethod { ShipMethodId = 5, Name = "Overnight" });
        if (includeShipTo)
        {
            dbContext.Addresses.Add(new AddressEntity { AddressId = 9, AddressLine1 = "1 Main St", City = "Reno", PostalCode = "89501" });
        }

        dbContext.SalesOrderHeaders.Add(new SalesOrderHeader
        {
            SalesOrderId = SalesOrderId,
            Status = status,
            RevisionNumber = revision,
            ShipToAddressId = 9,
            ShipMethodId = 5,
            SalesOrderDetails =
            [
                new SalesOrderDetail { SalesOrderId = SalesOrderId, SalesOrderDetailId = 1, ProductId = 707, OrderQty = 2, UnitPrice = 34.99m }
            ]
        });
        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
    }
}
