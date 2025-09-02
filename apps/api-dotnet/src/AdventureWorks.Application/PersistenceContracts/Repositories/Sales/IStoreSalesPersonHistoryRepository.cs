using AdventureWorks.Domain.Entities.Sales;

namespace AdventureWorks.Application.PersistenceContracts.Repositories.Sales;

public interface IStoreSalesPersonHistoryRepository : IAsyncRepository<StoreSalesPersonHistoryEntity>
{
    /// <summary>
    /// Retrieves all assignment history rows for the given store, ordered by StartDate descending.
    /// </summary>
    /// <param name="storeId">The store's BusinessEntityId.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    Task<IReadOnlyList<StoreSalesPersonHistoryEntity>> GetAssignmentsByStoreIdAsync(int storeId, CancellationToken cancellationToken = default);
}
