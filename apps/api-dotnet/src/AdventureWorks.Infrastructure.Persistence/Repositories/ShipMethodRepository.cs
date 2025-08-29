using AdventureWorks.Application.PersistenceContracts.Repositories;
using AdventureWorks.Common.Attributes;
using AdventureWorks.Domain.Entities.Purchasing;
using AdventureWorks.Infrastructure.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace AdventureWorks.Infrastructure.Persistence.Repositories;

[ServiceLifetimeScoped]
public sealed class ShipMethodRepository(AdventureWorksDbContext dbContext)
    : IShipMethodRepository
{
    private readonly AdventureWorksDbContext _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));

    public async Task<ShipMethod?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Set<ShipMethod>()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.ShipMethodId == id, cancellationToken);
    }

    public async Task<IReadOnlyList<ShipMethod>> ListAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Set<ShipMethod>()
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }
}
