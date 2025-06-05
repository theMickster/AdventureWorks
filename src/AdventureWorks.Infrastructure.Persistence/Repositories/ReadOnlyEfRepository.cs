using AdventureWorks.Application.PersistenceContracts.Repositories;
using AdventureWorks.Domain.Entities;
using AdventureWorks.Infrastructure.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace AdventureWorks.Infrastructure.Persistence.Repositories;

public class ReadOnlyEfRepository<T>(AdventureWorksDbContext dbContext) : IReadOnlyAsyncRepository<T>
    where T : BaseEntity
{
    protected readonly AdventureWorksDbContext DbContext = dbContext;

    public virtual async Task<T> GetByIdAsync(int id)
    {
        return await DbContext.Set<T>().FindAsync(id);
    }

    public virtual async Task<IReadOnlyList<T>> ListAllAsync()
    {
        return await DbContext.Set<T>().ToListAsync();
    }
}