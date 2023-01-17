using AdventureWorks.Application.Interfaces.Repositories;
using AdventureWorks.Common.Attributes;
using AdventureWorks.Domain.Entities;
using AdventureWorks.Infrastructure.Persistence.DbContexts;

namespace AdventureWorks.Infrastructure.Persistence.Repositories;

[ServiceLifetimeScoped]
public sealed class StateProvinceRepository : ReadOnlyEfRepository<StateProvinceEntity>, IStateProvinceRepository
{
    public StateProvinceRepository(AdventureWorksDbContext dbContext) : base(dbContext)
    {
    }
}