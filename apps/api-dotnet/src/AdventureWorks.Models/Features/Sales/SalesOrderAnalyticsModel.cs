namespace AdventureWorks.Models.Features.Sales;

/// <summary>Aggregated analytics for a filtered slice of sales orders.</summary>
public sealed class SalesOrderAnalyticsModel
{
    /// <summary>Sum of TotalDue for all matching orders.</summary>
    public decimal TotalRevenue { get; set; }

    /// <summary>Number of matching orders.</summary>
    public int OrderCount { get; set; }

    /// <summary>Filtered revenue as a percentage (0–100) of the all-time unfiltered revenue.</summary>
    public decimal PercentageOfTotal { get; set; }

    /// <summary>
    /// Monthly revenue trend for the matching orders, ordered oldest-first.
    /// Capped at 24 entries; date ranges spanning more than 24 months drop the oldest months.
    /// </summary>
    public IReadOnlyList<SalesOrderMonthlyTrendModel> MonthlyTrend { get; set; } = [];
}
