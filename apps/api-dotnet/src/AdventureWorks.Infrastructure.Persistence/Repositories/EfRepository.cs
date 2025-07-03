using AdventureWorks.Application.PersistenceContracts.Repositories;
using AdventureWorks.Domain.Entities;
using AdventureWorks.Infrastructure.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace AdventureWorks.Infrastructure.Persistence.Repositories;

/// <summary>
/// "There's some repetition here - couldn't we have some the sync methods call the async?"
/// https://blogs.msdn.microsoft.com/pfxteam/2012/04/13/should-i-expose-synchronous-wrappers-for-asynchronous-methods/
/// </summary>
/// <typeparam name="T"></typeparam>
public class EfRepository<T>(AdventureWorksDbContext dbContext)
    : ReadOnlyEfRepository<T>(dbContext), IAsyncRepository<T>
    where T : BaseEntity
{
    public async Task<T> AddAsync(T entity)
    {
        DbContext.Set<T>().Add(entity);
        await DbContext.SaveChangesAsync();

        return entity;
    }

    public async Task UpdateAsync(T entity)
    {
        DbContext.Entry(entity).State = EntityState.Modified;
        await DbContext.SaveChangesAsync();
    }

    public async Task DeleteAsync(T entity)
    {
        DbContext.Set<T>().Remove(entity);
        await DbContext.SaveChangesAsync();
    }
}