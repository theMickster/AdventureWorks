namespace AdventureWorks.Functions.SalesOrderSaga.Models;

/// <summary>
/// Consolidated output of <c>CheckInventorySubOrchestrator</c> after fanning out
/// <c>CheckInventoryActivity</c> across every order line and awaiting all results.
/// </summary>
public sealed record CheckInventoryResult(IReadOnlyList<LineItemAvailability> Lines)
{
    public bool AllAvailable => Lines.All(l => l.IsAvailable);
}
