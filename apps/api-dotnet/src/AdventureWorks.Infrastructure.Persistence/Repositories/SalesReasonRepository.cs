using AdventureWorks.Application.PersistenceContracts.Repositories;
using AdventureWorks.Common.Attributes;
using AdventureWorks.Domain.Entities.Sales;
using AdventureWorks.Infrastructure.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace AdventureWorks.Infrastructure.Persistence.Repositories;

[ServiceLifetimeScoped]
public sealed class SalesReasonRepository(AdventureWorksDbContext dbContext)
    : ISalesReasonRepository
{
    private readonly AdventureWorksDbContext _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));

    public async Task<SalesReason?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Set<SalesReason>()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.SalesReasonId == id, cancellationToken);
    }

    public async Task<IReadOnlyList<SalesReason>> ListAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Set<SalesReason>()
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }
}
