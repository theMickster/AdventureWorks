namespace AdventureWorks.Models.Features.Sales;

/// <summary>
/// Aggregate performance metrics for a single sales person.
/// </summary>
public sealed class SalesPersonPerformanceModel
{
    /// <summary>Gets or sets the sales year-to-date total.</summary>
    /// <value>Sourced from <c>SalesPerson.SalesYTD</c>.</value>
    public decimal SalesYtd { get; set; }

    /// <summary>Gets or sets the sales total for the prior year.</summary>
    /// <value>Sourced from <c>SalesPerson.SalesLastYear</c>.</value>
    public decimal SalesLastYear { get; set; }

    /// <summary>Gets or sets the current sales quota; <see langword="null"/> if unassigned.</summary>
    /// <value>Sourced from <c>SalesPerson.SalesQuota</c>.</value>
    public decimal? SalesQuota { get; set; }

    /// <summary>Gets or sets the bonus amount paid for the most recent sales period.</summary>
    /// <value>Sourced from <c>SalesPerson.Bonus</c>.</value>
    public decimal Bonus { get; set; }

    /// <summary>Gets or sets the commission rate applied to orders.</summary>
    /// <value>Sourced from <c>SalesPerson.CommissionPct</c>.</value>
    public decimal CommissionPct { get; set; }

    /// <summary>Gets or sets the total number of sales orders attributed to this sales person.</summary>
    /// <value>Computed via <c>GetSalesPersonOrderAggregatesAsync</c>.</value>
    public int OrderCount { get; set; }

    /// <summary>Gets or sets the total revenue across all sales orders attributed to this sales person.</summary>
    /// <value>Computed via <c>GetSalesPersonOrderAggregatesAsync</c>.</value>
    public decimal TotalRevenue { get; set; }

    /// <summary>Gets or sets the time-ordered list of quota history records.</summary>
    /// <value>Sourced from <c>SalesPersonQuotaHistory</c>.</value>
    public List<SalesPersonQuotaHistoryModel> QuotaHistory { get; set; } = [];

    /// <summary>Gets or sets the time-ordered list of territory assignment records.</summary>
    /// <value>Sourced from <c>SalesTerritoryHistory</c>.</value>
    public List<SalesPersonTerritoryHistoryModel> TerritoryHistory { get; set; } = [];
}
