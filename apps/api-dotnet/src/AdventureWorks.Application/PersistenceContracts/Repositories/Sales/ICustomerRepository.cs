using AdventureWorks.Common.Filtering;
using AdventureWorks.Domain.Entities.Sales;

namespace AdventureWorks.Application.PersistenceContracts.Repositories.Sales;

/// <summary>
/// Lightweight projection of <see cref="CustomerEntity"/> enriched with lifetime-value (LTV)
/// aggregates. <see cref="LtvRank"/> and <see cref="IsInactive"/> are computed after the full
/// customer set is materialized (rank is global and must survive search filtering), so they are
/// mutable rather than <c>init</c>.
/// </summary>
public sealed record CustomerLtvProjection
{
    /// <summary>The customer's unique identifier.</summary>
    public required int CustomerId { get; init; }

    /// <summary>The customer's store id, or <c>null</c> for an individual customer.</summary>
    public int? StoreId { get; init; }

    /// <summary>Store name (store customers) or contact person's full name (individual customers).</summary>
    public required string DisplayName { get; init; }

    /// <summary>Either <c>"Store"</c> or <c>"Individual"</c>.</summary>
    public required string CustomerType { get; init; }

    /// <summary>Sum of <c>SalesOrderHeader.TotalDue</c> across every order placed by this customer.</summary>
    public required decimal TotalSpend { get; init; }

    /// <summary>Total number of orders placed by this customer.</summary>
    public required int OrderCount { get; init; }

    /// <summary>Date of the customer's most recent order, or <c>null</c> when the customer has no orders.</summary>
    public DateTime? LastOrderDate { get; init; }

    /// <summary>
    /// Sequential rank (1, 2, 3, ...) by <see cref="TotalSpend"/> descending, tie-broken by
    /// <see cref="CustomerId"/> ascending. ROW_NUMBER semantics — every customer gets a distinct
    /// rank, even among ties.
    /// </summary>
    public int LtvRank { get; set; }

    /// <summary>
    /// <c>true</c> when the customer has never ordered, or their most recent order predates the
    /// 12-month cutoff measured from the most recent order date across all customers.
    /// </summary>
    public bool IsInactive { get; set; }
}

public interface ICustomerRepository : IAsyncRepository<CustomerEntity>
{
    /// <summary>
    /// Retrieves a paged, LTV-ranked, optionally search-filtered list of customers along with the
    /// total count of customers matching the search filter. Rank is assigned across the full
    /// customer set before search filtering, so a customer's rank does not change when the list is
    /// filtered.
    /// </summary>
    /// <param name="parameters">paging and search parameters</param>
    /// <param name="cancellationToken">token to cancel the operation</param>
    Task<(IReadOnlyList<CustomerLtvProjection>, int)> GetCustomersAsync(CustomerParameter parameters, CancellationToken cancellationToken = default);
}
