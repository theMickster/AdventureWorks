using AdventureWorks.Application.PersistenceContracts.Repositories;
using AdventureWorks.Common.Attributes;
using AdventureWorks.Domain.Entities;
using AdventureWorks.Infrastructure.Persistence.DbContexts;

namespace AdventureWorks.Infrastructure.Persistence.Repositories;

[ServiceLifetimeScoped]
public sealed class AddressTypeRepository : ReadOnlyEfRepository<AddressTypeEntity>, IAddressTypeRepository
{
    public AddressTypeRepository(AdventureWorksDbContext dbContext) : base(dbContext)
    {
    }
}
