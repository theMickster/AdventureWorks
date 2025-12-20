namespace AdventureWorks.Functions.SalesOrderSaga.Models;

/// <summary>
/// Output of <c>ReserveStockActivity</c> — confirms every line item's
/// <c>Production.ProductInventory.Quantity</c> was decremented and a matching
/// <c>Production.TransactionHistory</c> row was inserted, within a single transaction.
/// </summary>
public sealed record ReservationReceipt(int SalesOrderId, IReadOnlyList<int> ReservedProductIds, DateTimeOffset ReservedAt);
