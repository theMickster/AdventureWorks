using AdventureWorks.Application.Interfaces.Repositories;
using AdventureWorks.Domain.Entities;
using AdventureWorks.Infrastructure.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using AdventureWorks.Domain.Entities.Sales;

namespace AdventureWorks.Infrastructure.Persistence.Repositories;

/// <summary>
/// "There's some repetition here - couldn't we have some the sync methods call the async?"
/// https://blogs.msdn.microsoft.com/pfxteam/2012/04/13/should-i-expose-synchronous-wrappers-for-asynchronous-methods/
/// </summary>
/// <typeparam name="T"></typeparam>
public class EfRepository<T> : ReadOnlyEfRepository<T>,  IAsyncRepository<T> where T : BaseEntity
{
    public EfRepository(AdventureWorksDbContext dbContext)
        : base(dbContext)
    {
    }
    
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

    /// <summary>
    /// Retrieve a store by id along with its related entities
    /// </summary>
    /// <param name="storeId">the unique address identifier</param>
    /// <returns></returns>
    public async Task<StoreEntity> GetStoreEntityById(int storeId)
    {
        return await DbContext.Stores
            .Include(x => x.StoreBusinessEntity)
            .ThenInclude(y => y.BusinessEntityAddresses)
            .ThenInclude(y => y.AddressType)
            .Include(x => x.StoreBusinessEntity)
            .ThenInclude(y => y.BusinessEntityAddresses)
            .ThenInclude(z => z.Address)
            .FirstOrDefaultAsync(x => x.BusinessEntityId == storeId);
    }
}