using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AdventureWorks.Application.PersistenceContracts.Repositories;
using AdventureWorks.Application.Specifications;
using AdventureWorks.Domain.Entities;
using AdventureWorks.Infrastructure.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace AdventureWorks.Infrastructure.Persistence.Repositories;

public class ReadOnlyEfRepository<T> : IReadOnlyAsyncRepository<T> where T : BaseEntity
{
    protected readonly AdventureWorksDbContext DbContext;

    public ReadOnlyEfRepository(AdventureWorksDbContext dbContext)
    {
        DbContext = dbContext;
    }

    public virtual async Task<int> CountAsync(ISpecification<T> spec)
    {
        return await ApplySpecification(spec).CountAsync();
    }

    public virtual async Task<T> GetByIdAsync(int id)
    {
        return await DbContext.Set<T>().FindAsync(id);
    }

    public virtual async Task<IReadOnlyList<T>> ListAllAsync()
    {
        return await DbContext.Set<T>().ToListAsync();
    }

    public virtual async Task<IReadOnlyList<T>> ListAsync(ISpecification<T> spec)
    {
        return await ApplySpecification(spec).ToListAsync();
    }

    private IQueryable<T> ApplySpecification(ISpecification<T> spec)
    {
        return SpecificationEvaluator<T>.GetQuery(DbContext.Set<T>().AsQueryable(), spec);
    }
}