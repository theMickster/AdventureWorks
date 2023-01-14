using AdventureWorks.Domain.Entities;

namespace AdventureWorks.Application.Interfaces.Repositories;

public interface IAsyncRepository<T>: IReadOnlyAsyncRepository<T>  where T : BaseEntity
{
    
    Task<T> AddAsync(T entity);

    Task UpdateAsync(T entity);

    Task DeleteAsync(T entity);

}