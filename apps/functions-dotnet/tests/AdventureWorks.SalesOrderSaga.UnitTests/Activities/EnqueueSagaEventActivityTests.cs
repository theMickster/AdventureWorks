using AdventureWorks.SalesOrderSaga.Activities;
using AdventureWorks.SalesOrderSaga.Models;
using AdventureWorks.SalesOrderSaga.UnitTests.Persistence;
using Microsoft.DurableTask;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace AdventureWorks.SalesOrderSaga.UnitTests.Activities;

/// <summary>Coverage for durable, idempotent outbox enqueueing.</summary>
public sealed class EnqueueSagaEventActivityTests
{
    private static readonly SagaEventPublication Publication = new("PaymentTimedOut", "PaymentTimedOut:sales-order-saga-1", "{\"SalesOrderId\":1}");

    [Fact]
    public async Task RunAsync_RecordsUndispatchedOutboxMessage()
    {
        using var dbContext = SalesOrderSagaDbContextFactory.Create();
        var activity = new EnqueueSagaEventActivityCore(dbContext);

        var result = await activity.RunAsync(new Mock<TaskActivityContext>().Object, Publication);

        Assert.True(result);
        var message = await dbContext.OutboxMessages.SingleAsync(TestContext.Current.CancellationToken);
        Assert.Equal(Publication.MessageId, message.MessageId);
        Assert.Equal(Publication.EventName, message.EventName);
        Assert.Equal(Publication.Payload, message.Payload);
        Assert.Null(message.DispatchedAt);
    }

    [Fact]
    public async Task RunAsync_OnReplay_DoesNotDuplicate()
    {
        using var dbContext = SalesOrderSagaDbContextFactory.Create();
        var activity = new EnqueueSagaEventActivityCore(dbContext);
        var context = new Mock<TaskActivityContext>().Object;

        await activity.RunAsync(context, Publication);
        var replay = await activity.RunAsync(context, Publication);

        Assert.True(replay);
        Assert.Single(await dbContext.OutboxMessages.ToListAsync(TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task RunAsync_WhenInputIsNull_Throws()
    {
        using var dbContext = SalesOrderSagaDbContextFactory.Create();
        var activity = new EnqueueSagaEventActivityCore(dbContext);

        await Assert.ThrowsAsync<ArgumentNullException>(() => activity.RunAsync(new Mock<TaskActivityContext>().Object, null));
    }
}
