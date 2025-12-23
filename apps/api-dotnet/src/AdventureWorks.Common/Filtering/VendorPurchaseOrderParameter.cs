using AdventureWorks.Common.Filtering.Base;

namespace AdventureWorks.Common.Filtering;

/// <summary>
/// Used to support paging and filtering for a single vendor's purchase order history
/// (<c>GET /api/v1/vendors/{id}/purchase-orders</c>).
/// </summary>
/// <remarks>
/// Results are always ordered by order date descending — there is no client-facing sort parameter.
/// </remarks>
public sealed class VendorPurchaseOrderParameter : QueryStringParamsBase
{
    private int _take = 25;

    /// <summary>
    /// The amount of records requested to be returned to a list endpoint's caller.
    /// Overrides the base class default of 10 — this endpoint defaults to 25 per page.
    /// </summary>
    /// <remarks>
    /// Must be a true <c>override</c>, not a <c>new</c> hide: <see cref="QueryStringParamsBase.GetRecordsToSkip"/>
    /// calls <c>PageSize</c> polymorphically from the base class, so a <c>new</c> member would never be
    /// observed by that call and would silently break paging. The page size cannot be greater than fifty (50).
    /// </remarks>
    public override int PageSize
    {
        get => _take;
        init => _take = value <= 0 ? 1 : (value > MaxTake ? MaxTake : value);
    }

    /// <summary>
    /// Filter by purchase order status (1=Pending, 2=Approved, 3=Rejected, 4=Complete).
    /// </summary>
    public byte? Status { get; set; }

    /// <summary>
    /// Filter to purchase orders with <c>OrderDate</c> on or after this date (inclusive).
    /// </summary>
    public DateTime? StartDate { get; set; }

    /// <summary>
    /// Filter to purchase orders with <c>OrderDate</c> on or before this date (inclusive).
    /// </summary>
    public DateTime? EndDate { get; set; }
}
