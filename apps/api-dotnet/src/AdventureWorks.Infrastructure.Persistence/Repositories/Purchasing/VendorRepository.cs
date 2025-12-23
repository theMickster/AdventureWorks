using AdventureWorks.Application.PersistenceContracts.Repositories.Purchasing;
using AdventureWorks.Common.Attributes;
using AdventureWorks.Common.Filtering;
using AdventureWorks.Domain.Entities.Purchasing;
using AdventureWorks.Infrastructure.Persistence.DbContexts;
using AdventureWorks.Models.Features.Purchasing;
using Microsoft.EntityFrameworkCore;

namespace AdventureWorks.Infrastructure.Persistence.Repositories.Purchasing;

/// <summary>
/// Repository for vendor persistence operations.
/// </summary>
[ServiceLifetimeScoped]
public sealed class VendorRepository(AdventureWorksDbContext dbContext)
    : EfRepository<Vendor>(dbContext), IVendorRepository
{
    /// <summary>
    /// The top-half rank threshold used by <see cref="VendorModel.IsHighRisk"/>. The vendor
    /// population is 104, so the top half is the 52 highest-spending vendors.
    /// </summary>
    private const int HighRiskRankThreshold = 52;

    /// <summary>
    /// The minimum credit rating (Average/Below Average) that qualifies a vendor as high-risk.
    /// </summary>
    private const byte HighRiskCreditRatingThreshold = 4;

    /// <summary>
    /// A vendor joined in-memory with its aggregated spend, purchase order count, and
    /// pre-filter high-risk flag. Intentionally a strongly-typed record (not an anonymous type)
    /// so it can flow through filtering as a named type rather than <c>dynamic</c>.
    /// </summary>
    private sealed record VendorSpendRank(Vendor Vendor, decimal TotalSpend, int PoCount, bool IsHighRisk);

    /// <summary>
    /// Retrieves a paginated, risk-ranked list of vendors and the total count of vendors matching
    /// the optional filters in <paramref name="parameters"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Query shape:</b> Aggregation and vendor scalars are fetched as two separate,
    /// join-free queries — <c>PurchaseOrderHeaders.GroupBy(...)</c> and a bare
    /// <c>Vendors.AsNoTracking().ToListAsync()</c> — then joined in memory. This mirrors the
    /// Sales Order Analytics repository's GroupBy-without-join precedent: EF Core evaluates
    /// <c>GroupBy</c> aggregations client-side once a join is layered into the same query,
    /// which silently defeats server-side aggregation. The vendor table is small (104 rows),
    /// so materializing it and joining client-side is cheap and predictable.
    /// </para>
    /// <para>
    /// <b>Rank-before-filter invariant:</b> Every vendor's spend-rank and <c>IsHighRisk</c> flag
    /// are computed against the <i>full</i> vendor population before <c>creditRating</c>,
    /// <c>preferredVendorStatus</c>, or <c>activeFlag</c> filters are applied. Filters only
    /// narrow which already-ranked rows are returned in the final page. See
    /// <see cref="IVendorRepository.GetVendorsAsync"/> for the full rationale — do not move the
    /// filter predicates before the rank computation.
    /// </para>
    /// <para>
    /// <b>Rank semantics:</b> Uses true SQL <c>RANK()</c> semantics (ties share a rank), computed
    /// in C# as <c>1 + count of vendors with strictly greater total spend</c> — not the
    /// <c>ROW_NUMBER()</c>-equivalent <c>OrderByDescending().Select((x, i) => i + 1)</c>, which
    /// would give tied vendors distinct, and therefore incorrect, ranks.
    /// </para>
    /// </remarks>
    /// <param name="parameters">the input paging and filtering parameters</param>
    /// <param name="cancellationToken">token to cancel the operation</param>
    /// <returns>A tuple containing the paginated, risk-ranked list of vendors and the total count</returns>
    public async Task<(IReadOnlyList<VendorModel>, int)> GetVendorsAsync(
        VendorParameter parameters,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(parameters);

        // Step 1 — aggregate-only, no join.
        var spendByVendor = await DbContext.PurchaseOrderHeaders
            .AsNoTracking()
            .GroupBy(p => p.VendorId)
            .Select(g => new { VendorId = g.Key, TotalSpend = g.Sum(p => p.TotalDue), PoCount = g.Count() })
            .ToDictionaryAsync(x => x.VendorId, cancellationToken);

        // Step 2 — vendor scalars, separate query, no join. The vendor table is small (104 rows).
        var vendors = await DbContext.Vendors
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        // Step 3 — join in memory. Vendors with no purchase orders default to TotalSpend/PoCount = 0.
        var vendorSpend = vendors
            .Select(v =>
            {
                var hasSpend = spendByVendor.TryGetValue(v.BusinessEntityId, out var spend);
                return (Vendor: v, TotalSpend: hasSpend ? spend!.TotalSpend : 0m, PoCount: hasSpend ? spend!.PoCount : 0);
            })
            .ToList();

        // Step 4 — true RANK() semantics: rank = 1 + count of vendors with strictly greater spend.
        // Step 5 — IsHighRisk is computed here, against the full, unfiltered population.
        var rankedVendors = vendorSpend
            .Select(x => new VendorSpendRank(
                x.Vendor,
                x.TotalSpend,
                x.PoCount,
                IsHighRisk: x.Vendor.CreditRating >= HighRiskCreditRatingThreshold
                    && 1 + vendorSpend.Count(other => other.TotalSpend > x.TotalSpend) <= HighRiskRankThreshold))
            .ToList();

        // Step 6 — apply filters after rank (IsHighRisk) is computed, then page.
        IEnumerable<VendorSpendRank> filteredVendors = rankedVendors;

        if (parameters.CreditRating.HasValue)
        {
            filteredVendors = filteredVendors.Where(x => x.Vendor.CreditRating == parameters.CreditRating.Value);
        }

        if (parameters.PreferredVendorStatus.HasValue)
        {
            filteredVendors = filteredVendors.Where(x => x.Vendor.PreferredVendorStatus == parameters.PreferredVendorStatus.Value);
        }

        if (parameters.ActiveFlag.HasValue)
        {
            filteredVendors = filteredVendors.Where(x => x.Vendor.ActiveFlag == parameters.ActiveFlag.Value);
        }

        var orderedVendors = filteredVendors
            .OrderByDescending(x => x.TotalSpend)
            .ToList();

        var totalCount = orderedVendors.Count;

        var pagedVendors = orderedVendors
            .Skip(parameters.GetRecordsToSkip())
            .Take(parameters.PageSize)
            .Select(x => new VendorModel
            {
                VendorId = x.Vendor.BusinessEntityId,
                Name = x.Vendor.Name,
                AccountNumber = x.Vendor.AccountNumber,
                CreditRatingLabel = CreditRatingLabels.GetLabel(x.Vendor.CreditRating),
                PreferredVendorStatus = x.Vendor.PreferredVendorStatus,
                ActiveFlag = x.Vendor.ActiveFlag,
                TotalSpend = x.TotalSpend,
                PoCount = x.PoCount,
                IsHighRisk = x.IsHighRisk
            })
            .ToList();

        return (pagedVendors.AsReadOnly(), totalCount);
    }

    /// <summary>
    /// Retrieves a single vendor's profile and spend metrics.
    /// </summary>
    /// <remarks>
    /// Two queries, both scoped to the single <paramref name="vendorId"/> (unlike <see cref="GetVendorsAsync"/>'s
    /// full-table join-free pattern, which exists to avoid a client-side join across all 104 vendors) — a
    /// single-vendor aggregate is cheap to run as a real SQL <c>GROUP BY</c> plus a scalar vendor lookup.
    /// </remarks>
    /// <param name="vendorId">the vendor primary key</param>
    /// <param name="cancellationToken">token to cancel the operation</param>
    /// <returns>The vendor detail, or <c>null</c> if no vendor with that id exists</returns>
    public async Task<VendorDetailModel?> GetVendorDetailAsync(int vendorId, CancellationToken cancellationToken = default)
    {
        var vendor = await DbContext.Vendors
            .AsNoTracking()
            .FirstOrDefaultAsync(v => v.BusinessEntityId == vendorId, cancellationToken);

        if (vendor is null)
        {
            return null;
        }

        var spend = await DbContext.PurchaseOrderHeaders
            .AsNoTracking()
            .Where(p => p.VendorId == vendorId)
            .GroupBy(p => p.VendorId)
            .Select(g => new { TotalSpend = g.Sum(p => p.TotalDue), PoCount = g.Count() })
            .SingleOrDefaultAsync(cancellationToken);

        var totalSpend = spend?.TotalSpend ?? 0m;
        var poCount = spend?.PoCount ?? 0;

        return new VendorDetailModel
        {
            VendorId = vendor.BusinessEntityId,
            Name = vendor.Name,
            AccountNumber = vendor.AccountNumber,
            CreditRatingLabel = CreditRatingLabels.GetLabel(vendor.CreditRating),
            PreferredVendorStatus = vendor.PreferredVendorStatus,
            ActiveFlag = vendor.ActiveFlag,
            TotalSpend = totalSpend,
            PoCount = poCount,
            // Guard divide-by-zero: a vendor with no purchase orders has an average of 0, not NaN/exception.
            AvgPoValue = poCount == 0 ? 0m : totalSpend / poCount
        };
    }

    /// <summary>
    /// Retrieves a paginated, filterable page of a single vendor's purchase order history.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Vendor-existence short-circuit:</b> An <c>AnyAsync</c> existence check runs before the paged
    /// query. This lets the handler translate "no such vendor" into a 404 without running the (more
    /// expensive) paged/filtered query first, and lets a real "vendor exists, zero purchase orders
    /// matched the filter" case return an empty page rather than a false 404.
    /// </para>
    /// <para>
    /// <b>DueDate is <c>MIN</c> of line-item due dates, computed per row:</b> <c>PurchaseOrderHeader</c>
    /// has no <c>DueDate</c> column — <c>p.PurchaseOrderDetails.Min(d => (DateTime?)d.DueDate)</c> inside
    /// the paged <c>Select</c> projection is a correlated subquery, scoped to that single PO's own detail
    /// rows. This is intentionally a real SQL join/subquery, unlike <see cref="GetVendorsAsync"/>'s
    /// join-free, in-memory-joined pattern: that pattern exists specifically to avoid EF Core's
    /// GroupBy-forces-client-eval trap when aggregating across the *entire* (104-row) vendor table. Here
    /// the query is already scoped to one vendor's purchase orders (bounded, typically a few dozen rows),
    /// so a real per-row correlated subquery is cheap and translates cleanly to SQL — do not "fix" this to
    /// match the full-table pattern.
    /// </para>
    /// </remarks>
    /// <param name="vendorId">the vendor primary key</param>
    /// <param name="parameters">pagination and filter parameters (status, order date range)</param>
    /// <param name="cancellationToken">token to cancel the operation</param>
    /// <returns>A tuple containing the paginated purchase orders, the total matching count, and whether the vendor exists at all</returns>
    public async Task<(IReadOnlyList<PurchaseOrderSummaryModel> Results, int TotalCount, bool VendorExists)> GetVendorPurchaseOrdersAsync(
        int vendorId,
        VendorPurchaseOrderParameter parameters,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(parameters);

        var vendorExists = await DbContext.Vendors
            .AsNoTracking()
            .AnyAsync(v => v.BusinessEntityId == vendorId, cancellationToken);

        if (!vendorExists)
        {
            return (Array.Empty<PurchaseOrderSummaryModel>(), 0, false);
        }

        IQueryable<PurchaseOrderHeader> query = DbContext.PurchaseOrderHeaders
            .AsNoTracking()
            .Where(p => p.VendorId == vendorId);

        if (parameters.Status.HasValue)
        {
            query = query.Where(p => p.Status == parameters.Status.Value);
        }

        if (parameters.StartDate.HasValue)
        {
            query = query.Where(p => p.OrderDate >= parameters.StartDate.Value);
        }

        if (parameters.EndDate.HasValue)
        {
            query = query.Where(p => p.OrderDate <= parameters.EndDate.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        // Project raw, SQL-translatable scalars first (including the correlated DueDate-MIN subquery);
        // StatusLabel is a C# switch that cannot translate to SQL, so it is mapped after materialization.
        var pagedRows = await query
            .OrderByDescending(p => p.OrderDate)
            .ThenByDescending(p => p.PurchaseOrderId)
            .Skip(parameters.GetRecordsToSkip())
            .Take(parameters.PageSize)
            .Select(p => new
            {
                p.PurchaseOrderId,
                p.OrderDate,
                DueDate = p.PurchaseOrderDetails.Min(d => (DateTime?)d.DueDate),
                p.Status,
                p.TotalDue
            })
            .ToListAsync(cancellationToken);

        var results = pagedRows
            .Select(p => new PurchaseOrderSummaryModel
            {
                PurchaseOrderId = p.PurchaseOrderId,
                OrderDate = p.OrderDate,
                DueDate = p.DueDate,
                Status = p.Status,
                StatusLabel = PurchaseOrderStatusLabels.GetLabel(p.Status),
                TotalDue = p.TotalDue
            })
            .ToList();

        return (results.AsReadOnly(), totalCount, true);
    }
}
