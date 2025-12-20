namespace AdventureWorks.Functions.SalesOrderSaga.Persistence;

/// <summary>
/// <c>Production.TransactionHistory.TransactionType</c> is <c>nchar(1)</c>. The live
/// AdventureWorks data uses <c>W</c> for work orders, <c>P</c> for purchase orders, and
/// <c>S</c> for sales orders (verified by joining <c>TransactionType = 'S'</c> rows'
/// <c>ReferenceOrderID</c> back to <c>Sales.SalesOrderHeader.SalesOrderID</c>) — not the two
/// character "SR" Feature 610's tech notes mention, which cannot fit this column.
/// </summary>
public static class TransactionHistoryConstants
{
    public const string TransactionTypeSalesOrder = "S";
}
