using AdventureWorks.Domain.Entities.Sales;

namespace AdventureWorks.Application.PersistenceContracts.Repositories;

public interface ISalesReasonRepository
{
    Task<IReadOnlyList<SalesReason>> ListAllAsync(CancellationToken cancellationToken = default);

    Task<SalesReason?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
}
