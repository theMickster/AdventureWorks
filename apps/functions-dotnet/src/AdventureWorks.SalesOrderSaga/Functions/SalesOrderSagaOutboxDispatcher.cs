using AdventureWorks.SalesOrderSaga.Infrastructure;
using AdventureWorks.SalesOrderSaga.Persistence;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AdventureWorks.SalesOrderSaga.Functions;

/// <summary>Retries delivery of terminal saga notifications without re-running a completed saga.</summary>
public sealed class SalesOrderSagaOutboxDispatcher(
    SalesOrderSagaDbContext dbContext,
    ISalesOrderSagaEventPublisher publisher,
    ILogger<SalesOrderSagaOutboxDispatcher> logger)
{
    [Function(nameof(SalesOrderSagaOutboxDispatcher))]
    public async Task RunAsync([TimerTrigger("0 */1 * * * *")] TimerInfo timer, CancellationToken cancellationToken)
    {
        var pending = await dbContext.OutboxMessages
            .Where(message => message.DispatchedAt == null)
            .OrderBy(message => message.OccurredAt)
            .Take(25)
            .ToListAsync(cancellationToken);

        foreach (var message in pending)
        {
            try
            {
                await publisher.PublishSerializedAsync(message.EventName, message.MessageId, message.Payload, cancellationToken);
                message.DispatchedAt = DateTime.UtcNow;
                message.LastDispatchError = null;
            }
            catch (Exception exception)
            {
                message.LastDispatchError = exception.Message[..Math.Min(exception.Message.Length, 2048)];
                logger.LogWarning(exception, "Could not dispatch saga outbox message {MessageId}.", message.MessageId);
            }

            message.DispatchAttemptCount++;
            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
