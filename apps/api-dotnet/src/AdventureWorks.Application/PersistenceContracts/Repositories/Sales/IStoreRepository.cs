using AdventureWorks.Common.Filtering;
using AdventureWorks.Domain.Entities.Sales;

namespace AdventureWorks.Application.PersistenceContracts.Repositories.Sales;

// Projection records for repository read methods are co-located with the interface for now.
// If more accumulate (Story #883/#885 may add StorePerformanceProjection / StoreCustomerProjection),
// move all to a dedicated folder before then.
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

}
