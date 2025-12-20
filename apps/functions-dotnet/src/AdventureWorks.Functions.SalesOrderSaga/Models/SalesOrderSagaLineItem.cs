namespace AdventureWorks.Functions.SalesOrderSaga.Models;

/// <summary>
/// One order line carried on an <c>OrderCreated</c> event. Mirrors the subset of
/// <c>Sales.SalesOrderDetail</c> the saga needs to reserve stock — not a full row projection.
/// </summary>
public sealed class SalesOrderSagaLineItem
{
    public int ProductId { get; init; }

    public short OrderQty { get; init; }

    public decimal UnitPrice { get; init; }
}
