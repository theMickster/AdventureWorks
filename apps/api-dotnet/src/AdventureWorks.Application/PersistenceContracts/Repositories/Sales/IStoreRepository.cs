using AdventureWorks.Common.Filtering;
using AdventureWorks.Domain.Entities.Sales;

namespace AdventureWorks.Application.PersistenceContracts.Repositories.Sales;

// Projection records are co-located with the interface. Promote them to a dedicated folder
// (Application/Features/Sales/Projections/ or similar) when adding a fourth — three is the limit.
/// <summary>
/// Lightweight projection of <see cref="StoreEntity"/> used by demographics reads.
/// Carries only the columns required to populate
/// <c>StoreDemographicsModel</c> so EF emits a narrow SELECT.
/// </summary>
public sealed record StoreDemographicsProjection
{
    /// <summary>The store's BusinessEntityId.</summary>
    public required int BusinessEntityId { get; init; }

    /// <summary>The store's display name.</summary>
    public required string Name { get; init; }

    /// <summary>Raw <c>Sales.Store.Demographics</c> XML payload, or <c>null</c>.</summary>
    public string? Demographics { get; init; }
}

/// <summary>
/// Lightweight projection used by store performance reads. Carries the aggregate columns required
/// to populate <c>StorePerformanceModel</c>; <c>AverageOrderValue</c> is computed in the handler,
/// not here.
/// </summary>
public sealed record StorePerformanceProjection
{
    /// <summary>The store's BusinessEntityId.</summary>
    public required int BusinessEntityId { get; init; }

    /// <summary>The store's display name.</summary>
    public required string Name { get; init; }

    /// <summary>Sum of <c>SalesOrderHeader.TotalDue</c> across the store's customers for the year.</summary>
    public required decimal RevenueYtd { get; init; }

    /// <summary>Number of <c>SalesOrderHeader</c> rows that contributed to <see cref="RevenueYtd"/>.</summary>
    public required int OrderCount { get; init; }

    /// <summary>Distinct customer count for the store, independent of order activity.</summary>
    public required int CustomerCount { get; init; }

    /// <summary>Calendar year the aggregates cover (supplied by the caller).</summary>
    public required int Year { get; init; }
}

/// <summary>
/// Lightweight projection of a store customer enriched with per-customer order aggregates
/// (lifetime spend, order count, last order date). Carries only the columns required to
/// populate <c>StoreCustomerModel</c> so EF emits a narrow SELECT.
/// </summary>
public sealed record StoreCustomerProjection
{
    /// <summary>The customer's unique identifier.</summary>
    public required int CustomerId { get; init; }

    /// <summary>The customer's account number.</summary>
    public required string AccountNumber { get; init; }

    /// <summary>Full name of the customer's contact person, or empty when the customer has no person record.</summary>
    public required string PersonName { get; init; }

    /// <summary>Sum of <c>SalesOrderHeader.TotalDue</c> across every order placed by this customer.</summary>
    public required decimal LifetimeSpend { get; init; }

    /// <summary>Total number of orders placed by this customer.</summary>
    public required int OrderCount { get; init; }

    /// <summary>Date of the customer's most recent order, or <c>null</c> when the customer has no orders.</summary>
    public DateTime? LastOrderDate { get; init; }
}

public interface IStoreRepository : IAsyncRepository<StoreEntity>
{
    /// <summary>
    /// Retrieve a store by id along with its related entities
    /// </summary>
    /// <param name="storeId">the unique store identifier</param>
    /// <param name="includeAddresses">when false, address navigation properties are not loaded</param>
    /// <param name="cancellationToken">token to cancel the operation</param>
    Task<StoreEntity?> GetStoreByIdAsync(int storeId, bool includeAddresses = true, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a paginated list of stores and the total count of stores in the database.
    /// </summary>
    /// <param name="parameters">the input paging parameters</param>
    /// <param name="includeAddresses">when false, address navigation properties are not loaded</param>
    /// <param name="cancellationToken">token to cancel the operation</param>
    Task<(IReadOnlyList<StoreEntity>, int)> GetStoresAsync(StoreParameter parameters, bool includeAddresses = true, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a paged list of stores that is filtered using the <paramref name="storeSearchModel"/> input parameter.
    /// </summary>
    /// <param name="parameters"></param>
    /// <param name="storeSearchModel"></param>
    /// <param name="includeAddresses">when false, address navigation properties are not loaded</param>
    /// <param name="cancellationToken">token to cancel the operation</param>
    Task<(IReadOnlyList<StoreEntity>, int)> SearchStoresAsync(StoreParameter parameters, StoreSearchModel storeSearchModel, bool includeAddresses = true, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns true if a store with the given id exists.
    /// </summary>
    /// <param name="id">the store business entity id</param>
    /// <param name="cancellationToken">token to cancel the operation</param>
    Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a narrow projection containing only the columns needed to render
    /// <c>StoreDemographicsModel</c>: BusinessEntityId, Name, and the raw Demographics XML.
    /// Returns <c>null</c> when the store does not exist. Does not load any navigation
    /// properties — the caller parses the XML payload itself.
    /// </summary>
    /// <param name="storeId">the store business entity id</param>
    /// <param name="cancellationToken">token to cancel the operation</param>
    Task<StoreDemographicsProjection?> GetDemographicsAsync(int storeId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a narrow performance aggregate (revenue, order count, customer count) for the
    /// given store and calendar year. Returns <c>null</c> when the store does not exist.
    /// When the store exists but has no customers/orders the projection is populated with zero
    /// values so the caller can still render a model. The year is supplied by the caller so the
    /// repository remains storage-only (no clock dependency).
    /// </summary>
    /// <param name="storeId">the store business entity id</param>
    /// <param name="year">the calendar year the YTD aggregates cover</param>
    /// <param name="cancellationToken">token to cancel the operation</param>
    Task<StorePerformanceProjection?> GetPerformanceAsync(int storeId, int year, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a paged list of <see cref="StoreCustomerProjection"/> rows for the given store
    /// along with the total customer count. Each row carries per-customer order aggregates
    /// (lifetime spend, order count, last order date) computed in-database via a narrow projection.
    /// The caller must verify the store exists separately (e.g. <see cref="ExistsAsync"/>);
    /// when the store has no customers this method returns an empty list and a total count of zero.
    /// </summary>
    /// <param name="storeId">the store business entity id</param>
    /// <param name="parameters">paging, sort column, and sort order parameters</param>
    /// <param name="cancellationToken">token to cancel the operation</param>
    Task<(IReadOnlyList<StoreCustomerProjection>, int)> GetCustomersByStoreIdAsync(
        int storeId, StoreCustomerParameter parameters, CancellationToken cancellationToken = default);
}
