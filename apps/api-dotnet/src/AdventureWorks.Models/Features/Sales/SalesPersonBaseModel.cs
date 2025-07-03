namespace AdventureWorks.Models.Features.Sales;

public abstract class SalesPersonBaseModel
{
    public int? TerritoryId { get; set; }

    public decimal? SalesQuota { get; set; }

    public decimal Bonus { get; set; }

    public decimal CommissionPct { get; set; }
}
