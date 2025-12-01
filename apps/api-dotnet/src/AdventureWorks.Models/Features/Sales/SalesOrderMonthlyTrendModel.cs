namespace AdventureWorks.Models.Features.Sales;

/// <summary>Revenue total for a single calendar month within an analytics trend series.</summary>
public sealed class SalesOrderMonthlyTrendModel
{
    /// <summary>Four-digit calendar year (e.g. 2013).</summary>
    public int Year { get; set; }

    /// <summary>Calendar month, 1–12.</summary>
    public int Month { get; set; }

    /// <summary>Sum of TotalDue for all orders in this month.</summary>
    public decimal Revenue { get; set; }

    /// <summary>
    /// True when the maximum order date in the filtered dataset falls before the last calendar day
    /// of this month — indicating the data collection period ended mid-month and the revenue total
    /// is not representative of a full month.
    /// </summary>
    public bool IsPartialMonth { get; set; }
}
