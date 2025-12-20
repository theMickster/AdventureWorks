using AdventureWorks.Common.Filtering;
using AdventureWorks.Domain.Entities.Purchasing;
using AdventureWorks.Models.Features.Purchasing;

namespace AdventureWorks.Application.PersistenceContracts.Repositories.Purchasing;

/// <summary>
/// Repository interface for vendor persistence operations.
/// </summary>
public interface IVendorRepository : IAsyncRepository<Vendor>
{
    /// <summary>
    /// Retrieves a paginated, risk-ranked list of vendors and the total count of vendors matching
    /// the optional <paramref name="parameters"/> filters.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Critical invariant — rank is computed before filters:</b> Each vendor's spend-rank
    /// (true SQL <c>RANK()</c> semantics: ties share a rank, computed as 1 + the count of vendors
    /// with strictly greater total spend) is always computed over the <i>full</i> vendor
    /// population first. The <c>creditRating</c>, <c>preferredVendorStatus</c>, and
    /// <c>activeFlag</c> filters in <paramref name="parameters"/> are applied only after ranking —
    /// they narrow which already-ranked rows are returned, and never change what a vendor's rank
    /// is. <see cref="VendorModel.IsHighRisk"/> is derived from that pre-filter rank, so a vendor's
    /// <c>IsHighRisk</c> value is identical whether or not a filter happens to exclude other
    /// vendors from the result page. Do not "optimize" this by ranking after filtering — that
    /// would silently change <c>IsHighRisk</c> results whenever a filter is applied.
    /// </para>
    /// <para>
    /// <b>IsHighRisk:</b> True only when <c>CreditRating &gt;= 4</c> and the vendor's rank is
    /// <c>&lt;= 52</c> (the top half of the full vendor population).
    /// </para>
    /// <para>
    /// <b>Zero-PO vendors:</b> Vendors with no purchase orders are still included, with
    /// <c>TotalSpend = 0</c> and <c>PoCount = 0</c>.
    /// </para>
    /// </remarks>
    /// <param name="parameters">the input paging and filtering parameters</param>
    /// <param name="cancellationToken">token to cancel the operation</param>
    /// <returns>A tuple containing the paginated, risk-ranked list of vendors and the total count</returns>
    Task<(IReadOnlyList<VendorModel>, int)> GetVendorsAsync(
        VendorParameter parameters,
        CancellationToken cancellationToken = default);
}
