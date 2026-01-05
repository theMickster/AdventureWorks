using AdventureWorks.SalesOrderSaga.Persistence;
using AdventureWorks.SalesOrderSaga.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask;

namespace AdventureWorks.SalesOrderSaga.Activities;

/// <summary>Durably records a typed saga event for independent outbox delivery.</summary>
public sealed class EnqueueSagaEventActivityCore(SalesOrderSagaDbContext dbContext) : TaskActivity<SagaEventPublication, bool>
{
    public override async Task<bool> RunAsync(TaskActivityContext context, SagaEventPublication? input)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(input);
        if (await dbContext.OutboxMessages.FindAsync([input.MessageId], CancellationToken.None) is not null)
        {
            return true;
        }

        dbContext.OutboxMessages.Add(new SalesOrderSagaOutboxMessage
        {
            MessageId = input.MessageId,
            EventName = input.EventName,
            Payload = input.Payload,
            OccurredAt = DateTime.UtcNow
        });
        await dbContext.SaveChangesAsync(CancellationToken.None);
        return true;
    }
}

/// <summary>Functions adapter for <see cref="EnqueueSagaEventActivityCore"/>.</summary>
public sealed class EnqueueSagaEventActivity(SalesOrderSagaDbContext dbContext)
{
    [Function(nameof(EnqueueSagaEventActivity))]
    public Task<bool> RunAsync([ActivityTrigger] SagaEventPublication input) =>
        new EnqueueSagaEventActivityCore(dbContext).RunAsync(new FunctionsTaskActivityContext(nameof(EnqueueSagaEventActivity)), input);
}
