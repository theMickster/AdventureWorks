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
    /// and <see cref="CustomerLtvProjection.IsInactive"/> flag are then computed in memory, and
    /// the optional search filter and paging are also applied in memory — not in SQL.
    /// </summary>
    /// <remarks>
    /// The LTV rank must be computed across the full customer set before any search filter
    /// narrows it, and this codebase has no precedent for SQL <c>RANK()</c>/<c>ROW_NUMBER()</c>
    /// window functions, so in-memory ranking was a deliberate choice. This is acceptable at
    /// AdventureWorks' current bounded scale (~19,820 customers) and should be revisited (push
    /// ranking/paging into SQL via a <c>ROW_NUMBER()</c> projection or DB view) if the table
    /// grows substantially or this pattern is copied to another list endpoint.
    /// </remarks>
    public async Task<(IReadOnlyList<CustomerLtvProjection>, int)> GetCustomersAsync(
        CustomerParameter parameters, CancellationToken cancellationToken = default)
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
}
