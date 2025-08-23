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

    public override async Task<IReadOnlyList<SalesTerritoryEntity>> ListAllAsync(CancellationToken cancellationToken = default)
    {
        return await DbContext.SalesTerritories
            .Include(x => x.CountryRegion)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Retrieve a sales territory entity by its unique identifier
    /// </summary>
    /// <param name="id"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public override async Task<SalesTerritoryEntity?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await DbContext.SalesTerritories
            .Include(x => x.CountryRegion)
            .FirstOrDefaultAsync(s => s.TerritoryId == id, cancellationToken);
    }
}
