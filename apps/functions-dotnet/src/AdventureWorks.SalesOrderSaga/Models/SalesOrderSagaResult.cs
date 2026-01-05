namespace AdventureWorks.SalesOrderSaga.Models;

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
    ReservationReceipt? Receipt = null,
    string? FailureReason = null)
{
    public static SalesOrderSagaResult ValidationFailed(int salesOrderId, IReadOnlyList<string> errors) =>
        new(salesOrderId, SalesOrderSagaStatus.ValidationFailed, ValidationErrors: errors);

    public static SalesOrderSagaResult InsufficientStock(int salesOrderId, IReadOnlyList<LineItemAvailability> lines) =>
        new(salesOrderId, SalesOrderSagaStatus.InsufficientStock, UnavailableLines: lines.Where(l => !l.IsAvailable).ToList());

    public static SalesOrderSagaResult Reserved(int salesOrderId, ReservationReceipt receipt) =>
        new(salesOrderId, SalesOrderSagaStatus.Reserved, Receipt: receipt);

    public static SalesOrderSagaResult Approved(int salesOrderId, ReservationReceipt receipt) =>
        new(salesOrderId, SalesOrderSagaStatus.Approved, Receipt: receipt);

    public static SalesOrderSagaResult PaymentDeclined(int salesOrderId, ReservationReceipt receipt, string? reason) =>
        new(salesOrderId, SalesOrderSagaStatus.PaymentDeclined, Receipt: receipt, FailureReason: reason);

    public static SalesOrderSagaResult PaymentTimedOut(int salesOrderId, ReservationReceipt receipt) =>
        new(salesOrderId, SalesOrderSagaStatus.PaymentTimedOut, Receipt: receipt, FailureReason: "Payment result was not received within 30 minutes.");

    public static SalesOrderSagaResult PaymentAuthorizationFailed(int salesOrderId, ReservationReceipt receipt, string reason) =>
        new(salesOrderId, SalesOrderSagaStatus.PaymentAuthorizationFailed, Receipt: receipt, FailureReason: reason);

    public static SalesOrderSagaResult ConfirmationFailed(int salesOrderId, ReservationReceipt receipt, string reason) =>
        new(salesOrderId, SalesOrderSagaStatus.ConfirmationFailed, Receipt: receipt, FailureReason: reason);

    public static SalesOrderSagaResult CompensationFailed(int salesOrderId, ReservationReceipt receipt, string reason) =>
        new(salesOrderId, SalesOrderSagaStatus.CompensationFailed, Receipt: receipt, FailureReason: reason);
}

/// <summary>
/// Where a sales order saga orchestration landed. Compensation/payment states are added by
/// stories 808-810.
/// </summary>
public enum SalesOrderSagaStatus
{
    ValidationFailed,
    InsufficientStock,
    Reserved,
    Approved,
    PaymentDeclined,
    PaymentTimedOut,
    PaymentAuthorizationFailed,
    ConfirmationFailed,
    CompensationFailed
}
