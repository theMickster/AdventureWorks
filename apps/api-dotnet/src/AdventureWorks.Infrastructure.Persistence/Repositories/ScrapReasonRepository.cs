using AdventureWorks.Application.PersistenceContracts.Repositories;
using AdventureWorks.Common.Attributes;
using AdventureWorks.Domain.Entities.Production;
using AdventureWorks.Infrastructure.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace AdventureWorks.Infrastructure.Persistence.Repositories;

[ServiceLifetimeScoped]
public sealed class ScrapReasonRepository(AdventureWorksDbContext dbContext)
    : IScrapReasonRepository
{
    private readonly AdventureWorksDbContext _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));

    public async Task<ScrapReason?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Set<ScrapReason>()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.ScrapReasonId == id, cancellationToken);
    }

    public async Task<IReadOnlyList<ScrapReason>> ListAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Set<ScrapReason>()
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }
}
