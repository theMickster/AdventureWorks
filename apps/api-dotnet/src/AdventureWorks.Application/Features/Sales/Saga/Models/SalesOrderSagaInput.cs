namespace AdventureWorks.Application.Features.Sales.Saga.Models;

/// <summary>
/// Deserialized payload of an <c>OrderCreated</c> event published by the .NET API after a
/// <c>Sales.SalesOrderHeader</c> insert. This is the input to the sales order saga orchestrator
/// (<c>apps/functions-dotnet</c>); it carries only what the saga needs to validate, reserve
/// stock, and authorize payment — not a full entity projection. Lives in this shared package
/// (rather than the Functions app) so <see cref="Validators.SalesOrderSagaInputValidator"/> can
/// validate it without creating an Application -> Functions reference.
/// </summary>
public sealed class SalesOrderSagaInput
{
    public int SalesOrderId { get; init; }

    public int CustomerId { get; init; }

    public DateTimeOffset OrderDate { get; init; }

    public IReadOnlyList<SalesOrderSagaLineItem> Lines { get; init; } = [];
}
