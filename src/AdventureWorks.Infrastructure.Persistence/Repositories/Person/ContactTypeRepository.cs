using AdventureWorks.Application.Interfaces.Repositories.Person;
using AdventureWorks.Common.Attributes;
using AdventureWorks.Domain.Entities.Person;
using AdventureWorks.Infrastructure.Persistence.DbContexts;

namespace AdventureWorks.Infrastructure.Persistence.Repositories.Person;

[ServiceLifetimeScoped]
public sealed class ContactTypeRepository : ReadOnlyEfRepository<ContactTypeEntity>, IContactTypeRepository
{
    public ContactTypeRepository(AdventureWorksDbContext dbContext) : base(dbContext)
    {
        
    }
}
