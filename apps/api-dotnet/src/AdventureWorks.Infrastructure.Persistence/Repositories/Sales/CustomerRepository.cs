using AdventureWorks.Application.PersistenceContracts.Repositories.Sales;
using AdventureWorks.Common.Attributes;
using AdventureWorks.Common.Filtering;
using AdventureWorks.Domain.Entities.Sales;
using AdventureWorks.Infrastructure.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace AdventureWorks.Infrastructure.Persistence.Repositories.Sales;

[ServiceLifetimeScoped]
public sealed class CustomerRepository(AdventureWorksDbContext dbContext)
    : EfRepository<CustomerEntity>(dbContext), ICustomerRepository
{
    /// <summary>
    /// Performs a single SQL round trip that projects every customer into a narrow
    /// <see cref="CustomerLtvProjection"/>, using correlated aggregate subqueries for spend,
    /// order count, and last-order-date. The global <see cref="CustomerLtvProjection.LtvRank"/>
    /// and <see cref="CustomerLtvProjection.IsInactive"/> flag are then computed in memory.
    /// Shared by <see cref="GetCustomersAsync"/> and <see cref="GetCustomerDetailAsync"/> so both
    /// endpoints agree on rank — computed once, over the full (unfiltered, unpaged) customer set.
    /// </summary>
    /// <remarks>
    /// The LTV rank must be computed across the full customer set before any search filter
    /// narrows it, and this codebase has no precedent for SQL <c>RANK()</c>/<c>ROW_NUMBER()</c>
    /// window functions, so in-memory ranking was a deliberate choice. This is acceptable at
    /// AdventureWorks' current bounded scale (~19,820 customers) and should be revisited (push
    /// ranking/paging into SQL via a <c>ROW_NUMBER()</c> projection or DB view) if the table
    /// grows substantially. <see cref="GetCustomerDetailAsync"/> already shares this full-table
    /// pass and pays its full cost per single-record read — an accepted tradeoff at the current
    /// scale, not a future hypothetical.
    ///
    /// Ranking here is <c>ROW_NUMBER()</c> semantics over <c>(TotalSpend desc, CustomerId asc)</c>
    /// — every customer gets a distinct, sequential rank, even among ties on <c>TotalSpend</c> —
    /// NOT <c>RANK()</c> semantics, where tied rows would share a rank. Ties are the common case,
    /// not the exception: of the ~19,820 customers, 701 tie at $0 spend and another 15,302 share a
    /// non-zero spend value across 1,577 distinct tied amounts (roughly 81% of customers are
    /// tie-involved). <see cref="AdventureWorks.Infrastructure.Persistence.Repositories.Purchasing.VendorRepository"/>'s vendor-spend ranking
    /// uses true <c>RANK()</c> semantics (tied vendors share a rank); the two ranking approaches are
    /// NOT interchangeable, and porting logic between them without adjusting for this difference
    /// will silently change results for the majority of rows.
    /// </remarks>
    private async Task<List<CustomerLtvProjection>> GetRankedCustomerProjectionsAsync(CancellationToken cancellationToken)
    {
        var customers = await DbContext.Set<CustomerEntity>()
            .AsNoTracking()
            .Select(c => new CustomerLtvProjection
            {
                CustomerId = c.CustomerId,
                StoreId = c.StoreId,
                DisplayName = c.StoreId != null
                    ? (c.StoreEntity != null ? c.StoreEntity.Name : string.Empty)
                    : (c.Person != null ? c.Person.FirstName + " " + c.Person.LastName : string.Empty),
                CustomerType = c.StoreId != null ? "Store" : "Individual",
                StoreName = c.StoreEntity != null ? c.StoreEntity.Name : null,
                FirstName = c.Person != null ? c.Person.FirstName : null,
                LastName = c.Person != null ? c.Person.LastName : null,
                TotalSpend = c.SalesOrderHeaders.Sum(o => (decimal?)o.TotalDue) ?? 0m,
                OrderCount = c.SalesOrderHeaders.Count(),
                LastOrderDate = c.SalesOrderHeaders.Max(o => (DateTime?)o.OrderDate)
            })
            .ToListAsync(cancellationToken);

        var ranked = customers
            .OrderByDescending(c => c.TotalSpend)
            .ThenBy(c => c.CustomerId)
            .ToList();

        var hasAnyOrders = ranked.Any(c => c.LastOrderDate.HasValue);
        var cutoff = hasAnyOrders
            ? ranked.Where(c => c.LastOrderDate.HasValue).Max(c => c.LastOrderDate!.Value).AddMonths(-12)
            : DateTime.MinValue;

        for (var i = 0; i < ranked.Count; i++)
        {
            ranked[i].LtvRank = i + 1;
            ranked[i].IsInactive = !ranked[i].LastOrderDate.HasValue || ranked[i].LastOrderDate!.Value < cutoff;
        }

        return ranked;
    }

    /// <inheritdoc />
    public async Task<(IReadOnlyList<CustomerLtvProjection>, int)> GetCustomersAsync(
        CustomerParameter parameters, CancellationToken cancellationToken = default)
    {
        var ranked = await GetRankedCustomerProjectionsAsync(cancellationToken);

        IEnumerable<CustomerLtvProjection> filtered = ranked;
        if (!string.IsNullOrWhiteSpace(parameters.Search))
        {
            var search = parameters.Search.Trim();
            filtered = filtered.Where(c => c.DisplayName.Contains(search, StringComparison.OrdinalIgnoreCase));
        }

        var filteredList = filtered.ToList();
        var totalCount = filteredList.Count;

        var page = filteredList
            .Skip(parameters.GetRecordsToSkip())
            .Take(parameters.PageSize)
            .ToList();

        return (page.AsReadOnly(), totalCount);
    }

    /// <inheritdoc />
    public async Task<(CustomerLtvProjection? Customer, int TotalCustomerCount)> GetCustomerDetailAsync(
        int customerId, CancellationToken cancellationToken = default)
    {
        var ranked = await GetRankedCustomerProjectionsAsync(cancellationToken);

        var customer = ranked.FirstOrDefault(c => c.CustomerId == customerId);

        return (customer, ranked.Count);
    }
}
