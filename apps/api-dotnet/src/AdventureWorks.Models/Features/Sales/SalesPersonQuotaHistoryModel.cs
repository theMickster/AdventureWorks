namespace AdventureWorks.Models.Features.Sales;

/// <summary>
/// A single quota history record for a sales person.
/// </summary>
public sealed class SalesPersonQuotaHistoryModel
{
    /// <summary>Gets or sets the date the quota went into effect.</summary>
    /// <value>Sourced from <c>SalesPersonQuotaHistory.QuotaDate</c>.</value>
    public DateTime QuotaDate { get; set; }

    /// <summary>Gets or sets the quota amount effective from <see cref="QuotaDate"/>.</summary>
    /// <value>Sourced from <c>SalesPersonQuotaHistory.SalesQuota</c>.</value>
    public decimal SalesQuota { get; set; }
}
