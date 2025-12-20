namespace AdventureWorks.Functions.SalesOrderSaga.Models;

/// <summary>
/// Deserialized payload of an <c>OrderCreated</c> event published by the .NET API after a
/// <c>Sales.SalesOrderHeader</c> insert. This is the input to the sales order saga orchestrator;
/// it carries only what the saga needs to validate, reserve stock, and authorize payment —
/// not a full entity projection.
/// </summary>
public sealed class SalesOrderSagaInput
{
    public int SalesOrderId { get; init; }

    public int CustomerId { get; init; }

    public DateTimeOffset OrderDate { get; init; }

    public IReadOnlyList<SalesOrderSagaLineItem> Lines { get; init; } = [];
}
