using AdventureWorks.Application.Features.Sales.Saga.Models;
using AdventureWorks.Domain.Entities.Production;
using AdventureWorks.Functions.SalesOrderSaga.Models;
using AdventureWorks.Functions.SalesOrderSaga.Persistence;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask;
using Microsoft.EntityFrameworkCore;

namespace AdventureWorks.Functions.SalesOrderSaga.Activities;

/// <summary>
/// Core stock-reservation logic — the sales order saga's final US-807 step. Only called by
/// <c>SalesOrderSagaOrchestrator</c> once <c>CheckInventorySubOrchestrator</c> reports every
/// line as available. Within a single database transaction: decrements
/// <c>Production.ProductInventory.Quantity</c> per line item (spread across that product's
/// location rows, oldest <c>LocationId</c> first, since a product's stock is rarely all in one
/// location) and inserts one <c>Production.TransactionHistory</c> row per line item with
/// <see cref="TransactionHistoryConstants.TransactionTypeSalesOrder"/>. Compensation/rollback of
/// a committed reservation is out of scope for this story (stories 808-810).
/// </summary>
/// <remarks>
/// <para>
/// <b>Replay idempotency.</b> Durable Functions guarantees at-least-once activity execution —
/// this activity can be replayed (e.g. after a crash between <c>transaction.CommitAsync</c> and
/// the runtime checkpointing that fact). The orchestrator calls this activity exactly once per
/// order (not once per line), so <see cref="RunAsync"/> checks for an existing
/// <c>Production.TransactionHistory</c> row for this <c>SalesOrderId</c> before starting the
/// transaction; if one exists, the reservation already committed and it short-circuits with a
/// reconstructed <see cref="ReservationReceipt"/> instead of double-decrementing inventory.
/// </para>
/// <para>
/// <b>Concurrency.</b> <see cref="Microsoft.EntityFrameworkCore.DatabaseFacade.BeginTransactionAsync(System.Threading.CancellationToken)"/>
/// only makes this activity's own batch of statements atomic — it does not, by itself,
/// serialize this activity against a second saga instance concurrently reserving the same
/// <c>ProductId</c>. Two concurrent instances could otherwise both read the same
/// <c>ProductInventory</c> row, both compute a decrement from the same starting
/// <c>Quantity</c>, and the second <c>SaveChangesAsync</c> would silently overwrite the
/// first's decrement (a lost update). <see cref="Persistence.ProductInventoryConfiguration"/>
/// maps <c>rowguid</c> as an EF Core concurrency token, and <see cref="DecrementInventoryAsync"/>
/// assigns each decremented row a new <see cref="Guid"/> — so the losing writer's
/// <c>SaveChangesAsync</c> throws <see cref="Microsoft.EntityFrameworkCore.DbUpdateConcurrencyException"/>
/// instead of corrupting the row. There is no retry-on-conflict here: a thrown
/// <see cref="Microsoft.EntityFrameworkCore.DbUpdateConcurrencyException"/> fails this activity,
/// which fails the orchestration (no automatic re-check-and-retry of inventory is implemented —
/// that policy, along with compensation, is deferred to stories 808-810).
/// </para>
/// <para>
/// <b>CancellationToken.</b> <see cref="TaskActivity{TInput,TResult}.RunAsync"/> has no
/// <see cref="CancellationToken"/> parameter — the Durable Task SDK does not thread one through
/// to activities — so this method intentionally passes <see cref="CancellationToken.None"/> to
/// its EF Core calls rather than a caller-supplied token, unlike the repo-wide async convention.
/// </para>
/// </remarks>
public sealed class ReserveStockActivityCore(SalesOrderSagaDbContext dbContext) : TaskActivity<SalesOrderSagaInput, ReservationReceipt>
{
    public override async Task<ReservationReceipt> RunAsync(TaskActivityContext context, SalesOrderSagaInput? input)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(input);

        var existingTransactions = await dbContext.TransactionHistories
            .Where(th => th.ReferenceOrderId == input.SalesOrderId
                && th.TransactionType == TransactionHistoryConstants.TransactionTypeSalesOrder)
            .ToListAsync();

        if (existingTransactions.Count > 0)
        {
            // Durable Functions replayed this activity — the orchestrator only calls
            // ReserveStockActivity once per order (never once per line), so any existing
            // TransactionHistory rows for this SalesOrderId mean a prior run already committed
            // the reservation. Reconstruct the receipt instead of re-decrementing inventory.
            var reservedAt = existingTransactions.Min(th => th.TransactionDate);

            return new ReservationReceipt(
                input.SalesOrderId,
                existingTransactions.Select(th => th.ProductId).Distinct().ToList(),
                new DateTimeOffset(DateTime.SpecifyKind(reservedAt, DateTimeKind.Utc)));
        }

        var reservedProductIds = new List<int>(input.Lines.Count);

        await using var transaction = await dbContext.Database.BeginTransactionAsync(CancellationToken.None);

        for (var lineIndex = 0; lineIndex < input.Lines.Count; lineIndex++)
        {
            var line = input.Lines[lineIndex];

            await DecrementInventoryAsync(line);

            dbContext.TransactionHistories.Add(new TransactionHistory
            {
                ProductId = line.ProductId,
                ReferenceOrderId = input.SalesOrderId,
                // SalesOrderSagaLineItem (the shared saga DTO) does not carry the real
                // Sales.SalesOrderDetail.SalesOrderDetailID, so this is a 1-based position
                // within the order, not the actual detail row ID. Acceptable placeholder for
                // US 807 — revisit if a later story needs to correlate back to a specific
                // SalesOrderDetail row.
                ReferenceOrderLineId = lineIndex + 1,
                TransactionDate = DateTime.UtcNow,
                TransactionType = TransactionHistoryConstants.TransactionTypeSalesOrder,
                Quantity = line.OrderQty,
                ActualCost = line.UnitPrice,
                ModifiedDate = DateTime.UtcNow
            });

            reservedProductIds.Add(line.ProductId);
        }

        await dbContext.SaveChangesAsync(CancellationToken.None);
        await transaction.CommitAsync(CancellationToken.None);

        return new ReservationReceipt(input.SalesOrderId, reservedProductIds, DateTimeOffset.UtcNow);
    }

    /// <summary>
    /// Decrements <paramref name="line"/>'s <c>OrderQty</c> across its product's tracked
    /// <c>ProductInventory</c> rows, lowest <c>LocationId</c> first, stopping once satisfied.
    /// Bumps <see cref="ProductInventory.Rowguid"/> on every row it touches — the concurrency
    /// token <see cref="Persistence.ProductInventoryConfiguration"/> maps, so a concurrently
    /// racing saga instance that read the same row before this decrement commits gets a
    /// <see cref="Microsoft.EntityFrameworkCore.DbUpdateConcurrencyException"/> from
    /// <c>SaveChangesAsync</c> instead of silently losing this update. Throws
    /// <see cref="InvalidOperationException"/> if the tracked rows can't cover the requested
    /// quantity — the orchestrator only calls this activity after
    /// <c>CheckInventorySubOrchestrator</c> confirms availability, so this is a defensive check
    /// against a check-then-act race (stock consumed by another saga between the check and this
    /// reservation), not the primary insufficient-stock path.
    /// </summary>
    private async Task DecrementInventoryAsync(SalesOrderSagaLineItem line)
    {
        var remaining = (int)line.OrderQty;

        var inventoryRows = await dbContext.ProductInventories
            .Where(pi => pi.ProductId == line.ProductId && pi.Quantity > 0)
            .OrderBy(pi => pi.LocationId)
            .ToListAsync();

        foreach (var row in inventoryRows)
        {
            if (remaining <= 0)
            {
                break;
            }

            var decrement = (short)Math.Min(remaining, row.Quantity);
            row.Quantity -= decrement;
            row.Rowguid = Guid.NewGuid();
            remaining -= decrement;
        }

        if (remaining > 0)
        {
            throw new InvalidOperationException(
                $"Insufficient tracked stock for ProductId {line.ProductId} while reserving {line.OrderQty} unit(s).");
        }
    }
}

/// <summary>
/// Thin <c>[Function]</c> adapter for <see cref="ReserveStockActivityCore"/>.
/// </summary>
public sealed class ReserveStockActivity(SalesOrderSagaDbContext dbContext)
{
    [Function(nameof(ReserveStockActivity))]
    public Task<ReservationReceipt> RunAsync(
        [ActivityTrigger] SalesOrderSagaInput input, TaskActivityContext context) =>
        new ReserveStockActivityCore(dbContext).RunAsync(context, input);
}
