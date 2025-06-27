using AdventureWorks.Application.PersistenceContracts.Repositories.Sales;
using AdventureWorks.Common.Attributes;
using AdventureWorks.Domain.Entities.Sales;
using AdventureWorks.Infrastructure.Persistence.DbContexts;

namespace AdventureWorks.Infrastructure.Persistence.Repositories.Sales;

[ServiceLifetimeScoped]
public sealed class SalesPersonRepository(AdventureWorksDbContext dbContext)
    : EfRepository<SalesPersonEntity>(dbContext), ISalesPersonRepository
{

}
