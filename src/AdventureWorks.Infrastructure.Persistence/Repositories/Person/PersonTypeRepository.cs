using AdventureWorks.Application.Interfaces.Repositories.Person;
using AdventureWorks.Common.Attributes;
using AdventureWorks.Domain.Entities.Person;
using AdventureWorks.Infrastructure.Persistence.DbContexts;

namespace AdventureWorks.Infrastructure.Persistence.Repositories.Person;

[ServiceLifetimeScoped]
public sealed class PersonTypeRepository : ReadOnlyEfRepository<PersonTypeEntity>, IPersonTypeRepository
{
    public PersonTypeRepository(AdventureWorksDbContext dbContext) : base(dbContext)
    {
        
    }
}
