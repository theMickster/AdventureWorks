using AdventureWorks.Application.PersistenceContracts.Repositories;
using AdventureWorks.Common.Attributes;
using AdventureWorks.Domain.Entities.Sales;
using AdventureWorks.Infrastructure.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace AdventureWorks.Infrastructure.Persistence.Repositories;

[ServiceLifetimeScoped]
public sealed class SpecialOfferRepository(AdventureWorksDbContext dbContext)
    : ISpecialOfferRepository
{
    private readonly AdventureWorksDbContext _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));

    public async Task<SpecialOffer?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Set<SpecialOffer>()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.SpecialOfferId == id, cancellationToken);
    }

    public async Task<IReadOnlyList<SpecialOffer>> ListAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Set<SpecialOffer>()
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }
}
