using AdventureWorks.Domain.Entities.Sales;

namespace AdventureWorks.Application.Interfaces.Repositories.Sales;

public interface IStoreRepository : IAsyncRepository<StoreEntity>
{

    /// <summary>
    /// Retrieve a store by id along with its related entities
    /// </summary>
    /// <param name="storeId">the unique address identifier</param>
    /// <returns></returns>
    Task<StoreEntity> GetStoreByIdAsync(int storeId);

}
