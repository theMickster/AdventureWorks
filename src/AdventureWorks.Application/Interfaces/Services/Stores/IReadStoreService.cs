using AdventureWorks.Domain.Models.Sales;

namespace AdventureWorks.Application.Interfaces.Services.Stores;

public interface IReadStoreService
{
    /// <summary>
    /// Retrieve a store using its unique identifier.
    /// </summary>
    /// <returns>A <see cref="StoreModel"/> </returns>
    Task<StoreModel?> GetByIdAsync(int storeId);
}
