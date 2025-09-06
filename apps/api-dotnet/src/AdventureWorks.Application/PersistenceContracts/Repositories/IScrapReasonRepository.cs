using AdventureWorks.Domain.Entities.Production;

namespace AdventureWorks.Application.PersistenceContracts.Repositories;

public interface IScrapReasonRepository
{
    Task<ScrapReason?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ScrapReason>> ListAllAsync(CancellationToken cancellationToken = default);
}
