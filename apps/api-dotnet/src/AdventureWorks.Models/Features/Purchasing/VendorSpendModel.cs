namespace AdventureWorks.Models.Features.Purchasing;

/// <summary>
/// A single vendor's total purchase order spend and its running cumulative share of total
/// spend, used to build the Pareto (80/20) view of vendor concentration.
/// </summary>
public sealed class VendorSpendModel
{
    /// <summary>
    /// The vendor's primary key (<c>Purchasing.Vendor.BusinessEntityId</c>).
    /// </summary>
    public int VendorId { get; set; }

    /// <summary>
    /// The vendor's name.
    /// </summary>
    public string VendorName { get; set; } = string.Empty;

    /// <summary>
    /// The sum of <c>TotalDue</c> across every purchase order placed with this vendor.
    /// Zero for a vendor that has no purchase orders — such vendors are still included.
    /// </summary>
    public decimal TotalSpend { get; set; }

    /// <summary>
    /// The running share of total spend, as a percentage, accumulated across all vendors ordered
    /// by <see cref="TotalSpend"/> descending. The final entry is always exactly <c>100</c>, or
    /// <c>0</c> for every entry when total spend across all vendors is zero.
    /// </summary>
    public decimal CumulativePercent { get; set; }
}
