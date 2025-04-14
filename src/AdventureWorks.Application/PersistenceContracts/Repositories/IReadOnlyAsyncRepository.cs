using AdventureWorks.Domain.Entities;

namespace AdventureWorks.Application.PersistenceContracts.Repositories;

public interface IReadOnlyAsyncRepository<T> where T : BaseEntity
{
    public Task<T> GetByIdAsync(int id);

    Task<IReadOnlyList<T>> ListAllAsync();

    Task<int> CountAsync(ISpecification<T> spec);

    Task<IReadOnlyList<T>> ListAsync(ISpecification<T> spec);

}