using AdventureWorks.Common.Filtering;
using AdventureWorks.Domain.Models.Sales;

namespace AdventureWorks.Application.Interfaces.Services.Stores;

public interface IReadStoreService
{
    /// <summary>
    /// Retrieve a store using its unique identifier.
    /// </summary>
    /// <returns>A <see cref="StoreModel"/> </returns>
    Task<StoreModel?> GetByIdAsync(int storeId);

    /// <summary>
    /// Retrieves a paginated list of stores
    /// </summary>
    /// <param name="parameters">the input paging parameters</param>
    /// <returns>a <seealso cref="StoreSearchResultModel"/> object</returns>
    Task<StoreSearchResultModel> GetStoresAsync(StoreParameter parameters);

    /// <summary>
    /// Retrieves a paged list of stores that is filtered using the <paramref name="storeSearchModel"/> input parameter.
    /// </summary>
    /// <param name="parameters">the input paging parameters</param>
    /// <param name="storeSearchModel">the input search parameters</param>
    /// <returns>a <seealso cref="StoreSearchResultModel"/> object</returns>
    Task<StoreSearchResultModel> SearchStoresAsync(StoreParameter parameters, StoreSearchModel storeSearchModel);
}
