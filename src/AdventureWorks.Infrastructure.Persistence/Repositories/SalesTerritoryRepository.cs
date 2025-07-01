using AdventureWorks.Common.Attributes;
using AdventureWorks.Domain.Entities;
using AdventureWorks.Infrastructure.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using AdventureWorks.Application.PersistenceContracts.Repositories;
using AdventureWorks.Domain.Entities.Sales;

namespace AdventureWorks.Infrastructure.Persistence.Repositories;

[ServiceLifetimeScoped]
public sealed class SalesTerritoryRepository : ReadOnlyEfRepository<SalesTerritoryEntity>, ISalesTerritoryRepository
{
    public SalesTerritoryRepository(AdventureWorksDbContext dbContext) : base(dbContext)
    {
    }

    public override async Task<IReadOnlyList<SalesTerritoryEntity>> ListAllAsync()
    {
        return await DbContext.SalesTerritories
            .Include(x => x.CountryRegion)
            .ToListAsync();
    }
    
    /// <summary>
    /// Retrieve a state-province entity by its unique identifier
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public override async Task<SalesTerritoryEntity?> GetByIdAsync(int id)
    {
        return await DbContext.SalesTerritories
            .Include(x => x.CountryRegion)
            .FirstOrDefaultAsync(s => s.TerritoryId == id);
    }
}
