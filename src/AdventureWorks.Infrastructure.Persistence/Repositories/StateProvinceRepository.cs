using AdventureWorks.Common.Attributes;
using AdventureWorks.Domain.Entities;
using AdventureWorks.Infrastructure.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using AdventureWorks.Application.PersistenceContracts.Repositories;

namespace AdventureWorks.Infrastructure.Persistence.Repositories;

[ServiceLifetimeScoped]
public sealed class StateProvinceRepository : ReadOnlyEfRepository<StateProvinceEntity>, IStateProvinceRepository
{
    public StateProvinceRepository(AdventureWorksDbContext dbContext) : base(dbContext)
    {
    }

    public override async Task<IReadOnlyList<StateProvinceEntity>> ListAllAsync()
    {
        return await DbContext.StateProvinces
            .Include(x => x.CountryRegion)
            .Include(y => y.SalesTerritory)
            .ToListAsync();
    }


    /// <summary>
    /// Retrieve a state-province entity by its unique identifier
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public override async Task<StateProvinceEntity> GetByIdAsync(int id)
    {
        return await DbContext.StateProvinces
            .Include(x => x.CountryRegion)
            .Include(y => y.SalesTerritory)
            .FirstOrDefaultAsync(s => s.StateProvinceId == id);
    }
}