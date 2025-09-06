using AdventureWorks.Domain.Entities.Production;

namespace AdventureWorks.Application.PersistenceContracts.Repositories;

public interface ILocationRepository
{
    Task<Location?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Location>> ListAllAsync(CancellationToken cancellationToken = default);
}
