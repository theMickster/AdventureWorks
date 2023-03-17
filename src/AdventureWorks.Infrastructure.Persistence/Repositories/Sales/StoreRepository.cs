using AdventureWorks.Application.Interfaces.Repositories.Sales;
using AdventureWorks.Common.Attributes;
using AdventureWorks.Domain.Entities.Sales;
using AdventureWorks.Infrastructure.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace AdventureWorks.Infrastructure.Persistence.Repositories.Sales;

[ServiceLifetimeScoped]
public sealed class StoreRepository : EfRepository<StoreEntity>, IStoreRepository
{
    public StoreRepository(AdventureWorksDbContext dbContext) : base(dbContext)
    {
        
    }

    /// <summary>
    /// Retrieve a store by id along with its related entities
    /// </summary>
    /// <param name="storeId">the unique address identifier</param>
    /// <returns></returns>
    public async Task<StoreEntity> GetStoreById(int storeId)
    {
        return await DbContext.Stores
            .Include(x => x.StoreBusinessEntity)
            .ThenInclude(y => y.BusinessEntityAddresses)
            .ThenInclude(y => y.AddressType)
            .Include(x => x.StoreBusinessEntity)
            .ThenInclude(y => y.BusinessEntityAddresses)
            .ThenInclude(z => z.Address)
            .FirstOrDefaultAsync(x => x.BusinessEntityId == storeId);
    }
}
