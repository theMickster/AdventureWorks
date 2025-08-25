using AdventureWorks.Common.Filtering;
using AdventureWorks.Domain.Entities.Person;
using AdventureWorks.Domain.Entities.Sales;

namespace AdventureWorks.Application.PersistenceContracts.Repositories.Sales;

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
    /// Retrieves the list of addresses for a given store (business entity) id
    /// </summary>
    /// <param name="storeId">the unique store identifier</param>
    /// <param name="cancellationToken">token to cancel the operation</param>
    /// <returns>List of business entity address entities for the store, or an empty list if none exist.</returns>
    Task<List<BusinessEntityAddressEntity>> GetAddressesByStoreIdAsync(int storeId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns true if a store with the given id exists.
    /// </summary>
    /// <param name="id">the store business entity id</param>
    /// <param name="cancellationToken">token to cancel the operation</param>
    Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default);

}
