using AdventureWorks.SalesOrderSaga.Models;
using AdventureWorks.SalesOrderSaga.Contracts;
using AdventureWorks.SalesOrderSaga.Persistence;
using System.Text.Json;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask;
using Microsoft.EntityFrameworkCore;

namespace AdventureWorks.SalesOrderSaga.Activities;

/// <summary>Marks an authorized AdventureWorks order approved and builds its approval event.</summary>
public sealed class ConfirmOrderActivityCore(SalesOrderSagaDbContext dbContext) : TaskActivity<ConfirmOrderRequest, OrderApprovedEvent>
{
    public override async Task<OrderApprovedEvent> RunAsync(TaskActivityContext context, ConfirmOrderRequest? input)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(input);
        var order = await dbContext.SalesOrderHeaders
            .Include(o => o.SalesOrderDetails)
            .Include(o => o.ShipToAddressEntity)
            .Include(o => o.ShipMethod)
            .SingleOrDefaultAsync(o => o.SalesOrderId == input.SalesOrderId, CancellationToken.None)
            ?? throw new InvalidOperationException($"SalesOrderId {input.SalesOrderId} does not exist.");

        // Status 5 is AdventureWorks' Approved state. A Durable retry can start after a
        // successful SaveChanges but before its checkpoint, so only transition a non-approved
        // order; incrementing RevisionNumber on every replay would corrupt the order history.
        if (order.Status != 5)
        {
            order.Status = 5;
            order.RevisionNumber++;
            order.ShipDate ??= DateTime.UtcNow;
            order.ModifiedDate = DateTime.UtcNow;
        }

        var shipTo = order.ShipToAddressEntity
            ?? throw new InvalidOperationException($"SalesOrderId {input.SalesOrderId} has no ship-to address.");
        var shipMethod = order.ShipMethod
            ?? throw new InvalidOperationException($"SalesOrderId {input.SalesOrderId} has no ship method.");
        var approval = new OrderApprovedEvent(
            input.SalesOrderId,
            order.SalesOrderDetails.Select(d => new OrderApprovedLine(d.ProductId, d.OrderQty, d.UnitPrice)).ToList(),
            new OrderShippingAddress(shipTo.AddressId, shipTo.AddressLine1, shipTo.AddressLine2, shipTo.City, shipTo.PostalCode),
            new OrderShipMethod(shipMethod.ShipMethodId, shipMethod.Name));
        var messageId = $"{SagaEventNames.OrderApproved}:{input.SagaInstanceId}";
        if (!await dbContext.OutboxMessages.AnyAsync(message => message.MessageId == messageId, CancellationToken.None))
        {
            dbContext.OutboxMessages.Add(new SalesOrderSagaOutboxMessage
            {
                MessageId = messageId,
                EventName = SagaEventNames.OrderApproved,
                Payload = JsonSerializer.Serialize(approval),
                OccurredAt = DateTime.UtcNow
            });
        }

        // The order update and its notification share the DbContext's single SaveChanges
        // transaction, so a committed Approved order always has a dispatchable outbox record.
        await dbContext.SaveChangesAsync(CancellationToken.None);

        return approval;
    }
}

/// <summary>Functions adapter for <see cref="ConfirmOrderActivityCore"/>.</summary>
public sealed class ConfirmOrderActivity(SalesOrderSagaDbContext dbContext)
{
    [Function(nameof(ConfirmOrderActivity))]
    public Task<OrderApprovedEvent> RunAsync([ActivityTrigger] ConfirmOrderRequest request) =>
        new ConfirmOrderActivityCore(dbContext).RunAsync(new FunctionsTaskActivityContext(nameof(ConfirmOrderActivity)), request);
}
