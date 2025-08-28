using AdventureWorks.Domain.Entities.Purchasing;

namespace AdventureWorks.Application.PersistenceContracts.Repositories;

public interface IShipMethodRepository
{
    Task<IReadOnlyList<ShipMethod>> ListAllAsync(CancellationToken cancellationToken = default);

    Task<ShipMethod?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
}
