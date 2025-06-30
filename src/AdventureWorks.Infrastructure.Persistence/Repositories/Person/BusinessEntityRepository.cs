using AdventureWorks.Application.PersistenceContracts.Repositories.Person;
using AdventureWorks.Common.Attributes;
using AdventureWorks.Domain.Entities;
using AdventureWorks.Domain.Entities.Person;
using AdventureWorks.Infrastructure.Persistence.DbContexts;

namespace AdventureWorks.Infrastructure.Persistence.Repositories.Person;

[ServiceLifetimeScoped]
public sealed class BusinessEntityRepository : EfRepository<BusinessEntity>, IBusinessEntityRepository
{
    public BusinessEntityRepository(AdventureWorksDbContext dbContext) : base(dbContext)
    {
    }
}
