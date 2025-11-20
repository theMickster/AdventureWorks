namespace AdventureWorks.Models.Features.Sales;

/// <summary>
/// Request payload for a partial update to a sales person's sales configuration (territory, quota, bonus, commission).
/// </summary>
public sealed class SalesPersonSalesConfigUpdateModel : SalesPersonBaseModel
{
    public int Id { get; set; }
}
