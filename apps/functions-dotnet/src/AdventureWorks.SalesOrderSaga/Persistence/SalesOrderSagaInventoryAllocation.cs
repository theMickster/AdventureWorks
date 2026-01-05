namespace AdventureWorks.SalesOrderSaga.Persistence;

/// <summary>
/// Saga-owned ledger of the exact inventory rows a reservation changed. It is retained until
/// compensation completes so a retried activity can restore the same rows without guessing.
/// </summary>
public sealed class SalesOrderSagaInventoryAllocation
{
    public string SagaInstanceId { get; set; } = string.Empty;
    public int SalesOrderId { get; set; }
    public int LineNumber { get; set; }
    public int ProductId { get; set; }
    public short LocationId { get; set; }
    public short Quantity { get; set; }
    public DateTime ReservedAt { get; set; }
    public DateTime? ReversedAt { get; set; }
}
