using AdventureWorks.Domain.Entities.Production;

namespace AdventureWorks.Application.PersistenceContracts.Repositories;

public interface IProductModelRepository
{
    Task<ProductModel?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ProductModel>> ListAllAsync(CancellationToken cancellationToken = default);
}
