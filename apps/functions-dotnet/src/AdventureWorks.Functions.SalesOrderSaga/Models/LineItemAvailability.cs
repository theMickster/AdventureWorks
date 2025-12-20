namespace AdventureWorks.Functions.SalesOrderSaga.Models;

/// <summary>
/// Output of <c>CheckInventoryActivity</c> for a single order line — the requested quantity
/// compared against <c>Production.ProductInventory.Quantity</c> summed across all locations
/// for that product.
/// </summary>
public sealed record LineItemAvailability(int ProductId, short OrderQty, int AvailableQuantity)
{
    public bool IsAvailable => AvailableQuantity >= OrderQty;
}
