using AdventureWorks.Application.PersistenceContracts.Repositories;
using AdventureWorks.Common.Attributes;
using AdventureWorks.Domain.Entities.Sales;
using AdventureWorks.Infrastructure.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace AdventureWorks.Infrastructure.Persistence.Repositories;

[ServiceLifetimeScoped]
public sealed class CurrencyRepository(AdventureWorksDbContext dbContext)
    : ICurrencyRepository
{
    private readonly AdventureWorksDbContext _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));

    public async Task<Currency?> GetByIdAsync(string code, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Set<Currency>()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.CurrencyCode == code, cancellationToken);
    }

    public async Task<IReadOnlyList<Currency>> ListAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Set<Currency>()
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }
}
