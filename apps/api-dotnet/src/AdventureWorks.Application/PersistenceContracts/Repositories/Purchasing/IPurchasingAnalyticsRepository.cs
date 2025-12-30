using AdventureWorks.Models.Features.Purchasing;

namespace AdventureWorks.Application.PersistenceContracts.Repositories.Purchasing;

/// <summary>
/// Repository interface for aggregate purchasing analytics.
/// </summary>
public interface IPurchasingAnalyticsRepository
{
    /// <summary>
    /// Retrieves the Pareto vendor-spend distribution and the purchase order pipeline summary.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Every vendor always appears:</b> <see cref="PurchasingAnalyticsModel.ParetoData"/>
    /// contains one entry per vendor in the entire vendor population — a vendor with no purchase
    /// orders is included with <c>TotalSpend = 0</c>, never silently dropped by the spend join.
    /// This endpoint takes no parameters, so no <c>ActiveFlag</c> (or any other) filter is applied:
    /// inactive vendors are part of the population too. Do not add an implicit active-only filter.
    /// </para>
    /// <para>
    /// <b>Ordering is deterministic:</b> entries are ordered by <c>TotalSpend</c> descending, then
    /// by <c>VendorId</c> ascending. The <c>VendorId</c> tie-break matters — vendors with identical
    /// spend (including the whole block of zero-spend vendors) would otherwise come back in an
    /// arbitrary order, making the cumulative percentages unstable between calls.
    /// </para>
    /// <para>
    /// <b>CumulativePercent invariants:</b> the running percentage is computed over the ordered
    /// list, and the last entry is always exactly <c>100</c>. When total spend across all vendors
    /// is zero, every entry's <c>CumulativePercent</c> is <c>0</c> rather than a divide-by-zero.
    /// </para>
    /// <para>
    /// <b>Pipeline summary is always four rows:</b> all four known status codes (1=Pending,
    /// 2=Approved, 3=Rejected, 4=Complete) are present in that order. A status with no purchase
    /// orders is backfilled as <c>PoCount = 0, TotalValue = 0</c> so consumers never have to
    /// special-case a missing status.
    /// </para>
    /// </remarks>
    /// <param name="cancellationToken">token to cancel the operation</param>
    /// <returns>A <see cref="PurchasingAnalyticsModel"/> containing the Pareto vendor-spend data and the pipeline summary</returns>
    Task<PurchasingAnalyticsModel> GetPurchasingAnalyticsAsync(CancellationToken cancellationToken = default);
}
