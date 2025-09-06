using AdventureWorks.Application.PersistenceContracts.Repositories;
using AdventureWorks.Common.Attributes;
using AdventureWorks.Domain.Entities.Production;
using AdventureWorks.Infrastructure.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace AdventureWorks.Infrastructure.Persistence.Repositories;

[ServiceLifetimeScoped]
public sealed class UnitMeasureRepository(AdventureWorksDbContext dbContext)
    : IUnitMeasureRepository
{
    private readonly AdventureWorksDbContext _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));

    public async Task<UnitMeasure?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Set<UnitMeasure>()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.UnitMeasureCode == code, cancellationToken);
    }

    public async Task<IReadOnlyList<UnitMeasure>> ListAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Set<UnitMeasure>()
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }
}
