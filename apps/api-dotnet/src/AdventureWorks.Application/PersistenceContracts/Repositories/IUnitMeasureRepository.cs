using AdventureWorks.Domain.Entities.Production;

namespace AdventureWorks.Application.PersistenceContracts.Repositories;

public interface IUnitMeasureRepository
{
    Task<UnitMeasure?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<UnitMeasure>> ListAllAsync(CancellationToken cancellationToken = default);
}
