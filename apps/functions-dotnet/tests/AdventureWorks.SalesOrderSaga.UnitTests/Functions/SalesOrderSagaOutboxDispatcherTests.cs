using AdventureWorks.SalesOrderSaga.Functions;
using AdventureWorks.SalesOrderSaga.Infrastructure;
using AdventureWorks.SalesOrderSaga.Persistence;
using AdventureWorks.SalesOrderSaga.UnitTests.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace AdventureWorks.SalesOrderSaga.UnitTests.Functions;

/// <summary>Coverage for outbox delivery, retry bookkeeping and failure isolation.</summary>
public sealed class SalesOrderSagaOutboxDispatcherTests
{
    [Fact]
    public async Task RunAsync_PublishesPendingMessages_AndMarksThemDispatched()
    {
        using var dbContext = SalesOrderSagaDbContextFactory.Create();
        dbContext.OutboxMessages.Add(Pending("m1", DateTime.UtcNow));
        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        var publisher = new Mock<ISalesOrderSagaEventPublisher>();

        await Dispatcher(dbContext, publisher).RunAsync(null!, TestContext.Current.CancellationToken);

        publisher.Verify(p => p.PublishSerializedAsync("OrderApproved", "m1", "{}", It.IsAny<CancellationToken>()), Times.Once);
        var message = await dbContext.OutboxMessages.SingleAsync(TestContext.Current.CancellationToken);
        Assert.NotNull(message.DispatchedAt);
        Assert.Equal(1, message.DispatchAttemptCount);
        Assert.Null(message.LastDispatchError);
    }

    [Fact]
    public async Task RunAsync_WhenPublishFails_RecordsErrorLeavesPending_AndContinuesWithNext()
    {
        using var dbContext = SalesOrderSagaDbContextFactory.Create();
        dbContext.OutboxMessages.AddRange(Pending("bad", DateTime.UtcNow.AddMinutes(-2)), Pending("good", DateTime.UtcNow.AddMinutes(-1)));
        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        var publisher = new Mock<ISalesOrderSagaEventPublisher>();
        publisher.Setup(p => p.PublishSerializedAsync(It.IsAny<string>(), "bad", It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("bus down"));

        await Dispatcher(dbContext, publisher).RunAsync(null!, TestContext.Current.CancellationToken);

        var bad = await dbContext.OutboxMessages.SingleAsync(m => m.MessageId == "bad", TestContext.Current.CancellationToken);
        var good = await dbContext.OutboxMessages.SingleAsync(m => m.MessageId == "good", TestContext.Current.CancellationToken);
        Assert.Null(bad.DispatchedAt);
        Assert.Equal("bus down", bad.LastDispatchError);
        Assert.Equal(1, bad.DispatchAttemptCount);
        Assert.NotNull(good.DispatchedAt);
    }

    [Fact]
    public async Task RunAsync_TruncatesLongErrorMessages()
    {
        using var dbContext = SalesOrderSagaDbContextFactory.Create();
        dbContext.OutboxMessages.Add(Pending("m1", DateTime.UtcNow));
        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        var publisher = new Mock<ISalesOrderSagaEventPublisher>();
        publisher.Setup(p => p.PublishSerializedAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException(new string('x', 5000)));

        await Dispatcher(dbContext, publisher).RunAsync(null!, TestContext.Current.CancellationToken);

        Assert.Equal(2048, (await dbContext.OutboxMessages.SingleAsync(TestContext.Current.CancellationToken)).LastDispatchError!.Length);
    }

    [Fact]
    public async Task RunAsync_SkipsAlreadyDispatchedMessages()
    {
        using var dbContext = SalesOrderSagaDbContextFactory.Create();
        var sent = Pending("done", DateTime.UtcNow);
        sent.DispatchedAt = DateTime.UtcNow;
        dbContext.OutboxMessages.Add(sent);
        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        var publisher = new Mock<ISalesOrderSagaEventPublisher>(MockBehavior.Strict);

        await Dispatcher(dbContext, publisher).RunAsync(null!, TestContext.Current.CancellationToken);

        publisher.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task RunAsync_DispatchesAtMost25PerRun_OldestFirst()
    {
        using var dbContext = SalesOrderSagaDbContextFactory.Create();
        var start = DateTime.UtcNow.AddHours(-1);
        for (var i = 0; i < 30; i++)
        {
            dbContext.OutboxMessages.Add(Pending($"m{i:D2}", start.AddSeconds(i)));
        }

        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        var publisher = new Mock<ISalesOrderSagaEventPublisher>();

        await Dispatcher(dbContext, publisher).RunAsync(null!, TestContext.Current.CancellationToken);

        Assert.Equal(25, await dbContext.OutboxMessages.CountAsync(m => m.DispatchedAt != null, TestContext.Current.CancellationToken));
        Assert.All(
            await dbContext.OutboxMessages.Where(m => m.DispatchedAt == null).ToListAsync(TestContext.Current.CancellationToken),
            m => Assert.True(string.CompareOrdinal(m.MessageId, "m25") >= 0));
    }

    private static SalesOrderSagaOutboxDispatcher Dispatcher(SalesOrderSagaDbContext dbContext, Mock<ISalesOrderSagaEventPublisher> publisher) =>
        new(dbContext, publisher.Object, NullLogger<SalesOrderSagaOutboxDispatcher>.Instance);

    private static SalesOrderSagaOutboxMessage Pending(string id, DateTime occurredAt) =>
        new() { MessageId = id, EventName = "OrderApproved", Payload = "{}", OccurredAt = occurredAt };
}
