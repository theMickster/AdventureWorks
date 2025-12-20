using AdventureWorks.Common.Filtering.Base;

namespace AdventureWorks.Common.Filtering;

/// <summary>
/// Used to support paging and filtering in the AdventureWorks vendor list feature.
/// </summary>
/// <remarks>
/// Results are always ordered by total spend descending — there is no client-facing sort
/// parameter, per the vendor list's fixed risk-ranked view.
/// </remarks>
public sealed class VendorParameter : QueryStringParamsBase
{
    private int _take = 25;

    /// <summary>
    /// The amount of records requested to be returned to a list endpoint's caller.
    /// Overrides the base class default of 10 — the vendor list defaults to 25 per page.
    /// </summary>
    /// <remarks>The page size cannot be greater than fifty (50).</remarks>
    public override int PageSize
    {
        get => _take;
        init => _take = value <= 0 ? 1 : (value > MaxTake ? MaxTake : value);
    }

    /// <summary>
    /// Filter by credit rating (1=Superior, 2=Excellent, 3=Above Average, 4=Average, 5=Below Average).
    /// </summary>
    public byte? CreditRating { get; set; }

    /// <summary>
    /// Filter by preferred vendor status.
    /// </summary>
    public bool? PreferredVendorStatus { get; set; }

    /// <summary>
    /// Filter by active flag.
    /// </summary>
    public bool? ActiveFlag { get; set; }
}
