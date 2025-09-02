using AdventureWorks.Application.PersistenceContracts.Repositories.Sales;
using AdventureWorks.Common.Attributes;
using AdventureWorks.Domain.Entities.Sales;
using AdventureWorks.Infrastructure.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace AdventureWorks.Infrastructure.Persistence.Repositories.Sales;

[ServiceLifetimeScoped]
public sealed class StoreSalesPersonHistoryRepository(AdventureWorksDbContext dbContext)
    : EfRepository<StoreSalesPersonHistoryEntity>(dbContext), IStoreSalesPersonHistoryRepository
{
    /// <summary>
    /// Retrieves all assignment history rows for the given store, ordered by StartDate descending.
    /// </summary>
    public async Task<IReadOnlyList<StoreSalesPersonHistoryEntity>> GetAssignmentsByStoreIdAsync(int storeId, CancellationToken cancellationToken = default)
    {
        var results = await BuildBaseHistoryQuery()
            .Where(x => x.BusinessEntityId == storeId)
            .OrderByDescending(x => x.StartDate)
            .ToListAsync(cancellationToken);

        return results.AsReadOnly();
    }

    private IQueryable<StoreSalesPersonHistoryEntity> BuildBaseHistoryQuery() =>
        DbContext.StoreSalesPersonHistories
            .AsNoTracking()
            .Include(x => x.SalesPerson)
                .ThenInclude(sp => sp.Employee)
                .ThenInclude(e => e.PersonBusinessEntity)
            .Include(x => x.SalesPerson)
                .ThenInclude(sp => sp.SalesTerritory);
}
