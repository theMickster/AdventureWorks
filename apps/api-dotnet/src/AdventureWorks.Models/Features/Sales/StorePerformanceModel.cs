namespace AdventureWorks.Models.Features.Sales;

/// <summary>
/// Read model for a store's year-to-date sales performance summary.
/// Aggregates are scoped to the calendar year reported in <see cref="Year"/>.
/// </summary>
public sealed class StorePerformanceModel
{
    /// <summary>The store's BusinessEntityId.</summary>
    public int StoreId { get; set; }

    /// <summary>The store's display name.</summary>
    public string StoreName { get; set; } = string.Empty;

    /// <summary>
    /// Sum of <c>TotalDue</c> across all sales orders dated in the current calendar year.
    /// Equivalent to year-to-date when called for the current year (no future orders exist),
    /// which is the only mode the API supports today.
    /// </summary>
    public decimal RevenueYtd { get; set; }

    /// <summary>Number of <c>SalesOrderHeader</c> rows that contributed to <see cref="RevenueYtd"/>.</summary>
    public int OrderCount { get; set; }

    /// <summary>
    /// Mean order value: <see cref="RevenueYtd"/> divided by <see cref="OrderCount"/>.
    /// Returns <c>0</c> when <see cref="OrderCount"/> is zero.
    /// </summary>
    public decimal AverageOrderValue { get; set; }

    /// <summary>Distinct customer count for the store, independent of order activity.</summary>
    public int CustomerCount { get; set; }

    /// <summary>Calendar year the aggregates cover.</summary>
    public int Year { get; set; }
}
