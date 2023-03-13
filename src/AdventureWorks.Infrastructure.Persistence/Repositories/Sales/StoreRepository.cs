using AdventureWorks.Application.Interfaces.Repositories.Sales;
using AdventureWorks.Common.Attributes;
using AdventureWorks.Domain.Entities.Sales;
using AdventureWorks.Infrastructure.Persistence.DbContexts;

namespace AdventureWorks.Infrastructure.Persistence.Repositories.Sales;

[ServiceLifetimeScoped]
public sealed class StoreRepository : EfRepository<StoreEntity>, IStoreRepository
{
    public StoreRepository(AdventureWorksDbContext dbContext) : base(dbContext)
    {
        
    }


}
