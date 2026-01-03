namespace AdventureWorks.Models.Features.Production;

/// <summary>
/// Aggregate manufacturing performance metrics across all production work orders.
/// </summary>
public sealed class ManufacturingKpisModel
{
    /// <summary>The total number of production work orders.</summary>
    public int TotalWorkOrders { get; set; }

    /// <summary>The total quantity ordered across all production work orders.</summary>
    public int TotalOrdered { get; set; }

    /// <summary>The total quantity stocked after manufacturing.</summary>
    public int TotalStocked { get; set; }

    /// <summary>The total quantity scrapped during manufacturing.</summary>
    public int TotalScrapped { get; set; }

    /// <summary>The overall stocked quantity as a percentage of the ordered quantity, rounded to two decimal places.</summary>
    public decimal OverallYieldPct { get; set; }

    /// <summary>The overall scrapped quantity as a percentage of the ordered quantity, rounded to two decimal places.</summary>
    public decimal OverallScrapPct { get; set; }
}
