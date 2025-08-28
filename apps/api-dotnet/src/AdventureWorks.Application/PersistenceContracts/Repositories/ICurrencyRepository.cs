using AdventureWorks.Domain.Entities.Sales;

namespace AdventureWorks.Application.PersistenceContracts.Repositories;

public interface ICurrencyRepository
{
    Task<IReadOnlyList<Currency>> ListAllAsync(CancellationToken cancellationToken = default);

    Task<Currency?> GetByIdAsync(string code, CancellationToken cancellationToken = default);
}
