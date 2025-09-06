using AdventureWorks.Application.PersistenceContracts.Repositories;
using AdventureWorks.Common.Attributes;
using AdventureWorks.Domain.Entities.Production;
using AdventureWorks.Infrastructure.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace AdventureWorks.Infrastructure.Persistence.Repositories;

[ServiceLifetimeScoped]
public sealed class LocationRepository(AdventureWorksDbContext dbContext)
    : ILocationRepository
{
    private readonly AdventureWorksDbContext _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));

    public async Task<Location?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Set<Location>()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.LocationId == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Location>> ListAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Set<Location>()
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }
}
