namespace AdventureWorks.Models.Features.Purchasing;

/// <summary>
/// Full profile and spend-metric detail for a single vendor (<c>GET /api/v1/vendors/{id}</c>).
/// </summary>
public sealed class VendorDetailModel
{
    /// <summary>
    /// The vendor's primary key (BusinessEntityId).
    /// </summary>
    public int VendorId { get; set; }

    /// <summary>
    /// The vendor's name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// The vendor's account number.
    /// </summary>
    public string AccountNumber { get; set; } = string.Empty;

    /// <summary>
    /// The human-readable label for the vendor's credit rating: 1=Superior, 2=Excellent,
    /// 3=Above Average, 4=Average, 5=Below Average.
    /// </summary>
    public string CreditRatingLabel { get; set; } = string.Empty;

    /// <summary>
    /// Whether the vendor is flagged as a preferred vendor.
    /// </summary>
    public bool PreferredVendorStatus { get; set; }

    /// <summary>
    /// Whether the vendor is currently active.
    /// </summary>
    public bool ActiveFlag { get; set; }

    /// <summary>
    /// The vendor's total spend (sum of <c>TotalDue</c> across all purchase orders). Zero when
    /// the vendor has no purchase orders.
    /// </summary>
    public decimal TotalSpend { get; set; }

    /// <summary>
    /// The number of purchase orders placed with this vendor. Zero when the vendor has no
    /// purchase orders.
    /// </summary>
    public int PoCount { get; set; }

    /// <summary>
    /// Average purchase order value (<c>TotalSpend / PoCount</c>). Zero — not a divide-by-zero
    /// exception — when the vendor has no purchase orders.
    /// </summary>
    public decimal AvgPoValue { get; set; }
}
