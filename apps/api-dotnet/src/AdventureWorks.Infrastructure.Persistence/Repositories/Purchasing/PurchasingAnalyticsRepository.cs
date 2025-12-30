using AdventureWorks.Application.PersistenceContracts.Repositories.Purchasing;
using AdventureWorks.Common.Attributes;
using AdventureWorks.Infrastructure.Persistence.DbContexts;
using AdventureWorks.Models.Features.Purchasing;
using Microsoft.EntityFrameworkCore;

namespace AdventureWorks.Infrastructure.Persistence.Repositories.Purchasing;

/// <summary>
/// EF Core implementation of <see cref="IPurchasingAnalyticsRepository"/> that executes the
/// aggregate purchasing queries and assembles the analytics response model.
/// </summary>
[ServiceLifetimeScoped]
public sealed class PurchasingAnalyticsRepository(AdventureWorksDbContext dbContext)
    : IPurchasingAnalyticsRepository
{
    private readonly AdventureWorksDbContext _dbContext = dbContext
        ?? throw new ArgumentNullException(nameof(dbContext));

    /// <summary>
    /// The complete set of <c>Purchasing.PurchaseOrderHeader.Status</c> codes, in the order the
    /// pipeline summary reports them: 1=Pending, 2=Approved, 3=Rejected, 4=Complete. Used to
    /// backfill statuses that have no purchase orders, so the summary is always four rows.
    /// </summary>
    private static readonly byte[] KnownPurchaseOrderStatuses = [1, 2, 3, 4];

    /// <summary>
    /// Retrieves the Pareto vendor-spend distribution and the purchase order pipeline summary.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Query shape — join-free, joined in memory:</b> per-vendor spend and the vendor scalars
    /// are fetched as two separate queries (<c>PurchaseOrderHeaders.GroupBy(p =&gt; p.VendorId)</c>
    /// and a bare <c>Vendors.AsNoTracking().ToListAsync()</c>), then joined client-side. This
    /// mirrors <see cref="VendorRepository.GetVendorsAsync"/>: EF Core evaluates a <c>GroupBy</c>
    /// aggregation client-side the moment a join is layered into the same query, silently defeating
    /// server-side aggregation. The vendor table is small (104 rows), so materializing it and
    /// joining in memory is cheap and predictable. No raw SQL is involved.
    /// </para>
    /// <para>
    /// <b>All vendors always appear:</b> the in-memory join drives off the vendor list, not the
    /// spend dictionary, so a vendor with zero purchase orders is emitted with
    /// <c>TotalSpend = 0</c>. No <c>ActiveFlag</c> or other filter is applied — this endpoint takes
    /// no parameters and the Pareto view is defined over the full vendor population.
    /// </para>
    /// <para>
    /// <b>Deterministic ordering:</b> ordered by <c>TotalSpend</c> descending, then <c>VendorId</c>
    /// ascending. The tie-break is required, not cosmetic: the zero-spend vendors all share a spend
    /// value, and without it their order (and therefore the cumulative percentages assigned to
    /// them) would vary between calls.
    /// </para>
    /// <para>
    /// <b>CumulativePercent is computed in C#, after ordering:</b> a running sum over the ordered
    /// list, divided by the grand total. It is deliberately not a SQL window function — the
    /// ordering already exists in memory and the population is tiny. Two invariants are enforced
    /// explicitly: when the grand total is zero, every row gets <c>0</c> instead of dividing by
    /// zero; otherwise the last row is assigned exactly <c>100</c> by direct assignment, because
    /// accumulated decimal division can otherwise leave it a hair off 100.
    /// </para>
    /// <para>
    /// <b>Pipeline summary:</b> a single <c>GroupBy(p =&gt; p.Status)</c> aggregate, then mapped
    /// through <see cref="PurchaseOrderStatusLabels.GetLabel(byte)"/> — the same internal helper
    /// shared with <see cref="VendorRepository"/> and <see cref="PurchaseOrderRepository"/>, rather
    /// than a duplicated status switch. Statuses in <see cref="KnownPurchaseOrderStatuses"/> that
    /// returned no rows are backfilled as zero-value entries.
    /// </para>
    /// </remarks>
    /// <param name="cancellationToken">token to cancel the operation</param>
    /// <returns>A <see cref="PurchasingAnalyticsModel"/> containing the Pareto vendor-spend data and the pipeline summary</returns>
    public async Task<PurchasingAnalyticsModel> GetPurchasingAnalyticsAsync(CancellationToken cancellationToken = default)
    {
        // Step 1 — aggregate-only, no join.
        var spendByVendor = await _dbContext.PurchaseOrderHeaders
            .AsNoTracking()
            .GroupBy(p => p.VendorId)
            .Select(g => new { VendorId = g.Key, TotalSpend = g.Sum(p => p.TotalDue) })
            .ToDictionaryAsync(x => x.VendorId, x => x.TotalSpend, cancellationToken);

        // Step 2 — vendor scalars, separate query, no join, no filters.
        var vendors = await _dbContext.Vendors
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        // Step 3 — join in memory; a vendor absent from the spend dictionary keeps TotalSpend = 0.
        var paretoData = vendors
            .Select(v => new VendorSpendModel
            {
                VendorId = v.BusinessEntityId,
                VendorName = v.Name,
                TotalSpend = spendByVendor.TryGetValue(v.BusinessEntityId, out var spend) ? spend : 0m
            })
            .OrderByDescending(x => x.TotalSpend)
            .ThenBy(x => x.VendorId)
            .ToList();

        // Step 4 — running cumulative percentage over the ordered list.
        var grandTotalSpend = paretoData.Sum(x => x.TotalSpend);

        if (grandTotalSpend > 0m)
        {
            var runningTotal = 0m;

            foreach (var vendorSpend in paretoData)
            {
                runningTotal += vendorSpend.TotalSpend;
                vendorSpend.CumulativePercent = runningTotal / grandTotalSpend * 100m;
            }

            // Pin the final entry to exactly 100 — accumulated division can leave it slightly off.
            paretoData[^1].CumulativePercent = 100m;
        }

        // Step 5 — pipeline aggregate by status, then label + backfill the missing statuses.
        var pipelineAggregates = await _dbContext.PurchaseOrderHeaders
            .AsNoTracking()
            .GroupBy(p => p.Status)
            .Select(g => new { Status = g.Key, Count = g.Count(), Total = g.Sum(p => p.TotalDue) })
            .ToListAsync(cancellationToken);

        var pipelineSummary = KnownPurchaseOrderStatuses
            .Select(status =>
            {
                var aggregate = pipelineAggregates.FirstOrDefault(x => x.Status == status);

                return new PurchaseOrderPipelineStatusModel
                {
                    StatusLabel = PurchaseOrderStatusLabels.GetLabel(status),
                    PoCount = aggregate?.Count ?? 0,
                    TotalValue = aggregate?.Total ?? 0m
                };
            })
            .ToList();

        return new PurchasingAnalyticsModel
        {
            ParetoData = paretoData,
            PipelineSummary = pipelineSummary
        };
    }
}
