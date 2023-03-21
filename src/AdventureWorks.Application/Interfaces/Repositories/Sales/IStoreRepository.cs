using AdventureWorks.Common.Filtering;
using AdventureWorks.Domain.Entities.Sales;

namespace AdventureWorks.Application.Interfaces.Repositories.Sales;

public interface IStoreRepository : IAsyncRepository<StoreEntity>
{
    /// <summary>
    /// Retrieve a store by id along with its related entities
    /// </summary>
    /// <param name="storeId">the unique store identifier</param>
    /// <returns></returns>
    Task<StoreEntity?> GetStoreByIdAsync(int storeId);

    /// <summary>
    /// Retrieves a paginated list of stores and the total count of stores in the database.
    /// </summary>
    /// <param name="parameters">the input paging parameters</param>
    Task< (IReadOnlyList<StoreEntity>, int)> GetStores(StoreParameter parameters);
}
