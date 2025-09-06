using AdventureWorks.Application.PersistenceContracts.Repositories;
using AdventureWorks.Common.Attributes;
using AdventureWorks.Domain.Entities.Production;
using AdventureWorks.Infrastructure.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace AdventureWorks.Infrastructure.Persistence.Repositories;

[ServiceLifetimeScoped]
public sealed class ProductModelRepository(AdventureWorksDbContext dbContext)
    : IProductModelRepository
{
    private readonly AdventureWorksDbContext _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));

    public async Task<ProductModel?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Set<ProductModel>()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.ProductModelId == id, cancellationToken);
    }

    public async Task<IReadOnlyList<ProductModel>> ListAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Set<ProductModel>()
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }
}
