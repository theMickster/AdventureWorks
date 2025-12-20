namespace AdventureWorks.Functions.SalesOrderSaga.Models;

/// <summary>
/// Terminal outcome of a sales order saga orchestration, returned as the orchestrator's output
/// and surfaced by <c>SalesOrderSagaStatusFunction</c>. Exactly one of
/// <see cref="ValidationErrors"/>, <see cref="UnavailableLines"/>, or <see cref="Receipt"/> is
/// populated, matching <see cref="Status"/>.
/// </summary>
public sealed record SalesOrderSagaResult(
    int SalesOrderId,
    SalesOrderSagaStatus Status,
    IReadOnlyList<string>? ValidationErrors = null,
    IReadOnlyList<LineItemAvailability>? UnavailableLines = null,
    ReservationReceipt? Receipt = null)
{
    public static SalesOrderSagaResult ValidationFailed(int salesOrderId, IReadOnlyList<string> errors) =>
        new(salesOrderId, SalesOrderSagaStatus.ValidationFailed, ValidationErrors: errors);

    public static SalesOrderSagaResult InsufficientStock(int salesOrderId, IReadOnlyList<LineItemAvailability> lines) =>
        new(salesOrderId, SalesOrderSagaStatus.InsufficientStock, UnavailableLines: lines.Where(l => !l.IsAvailable).ToList());

    public static SalesOrderSagaResult Reserved(int salesOrderId, ReservationReceipt receipt) =>
        new(salesOrderId, SalesOrderSagaStatus.Reserved, Receipt: receipt);
}

/// <summary>
/// Where a sales order saga orchestration landed. Compensation/payment states are added by
/// stories 808-810.
/// </summary>
public enum SalesOrderSagaStatus
{
    ValidationFailed,
    InsufficientStock,
    Reserved
}
