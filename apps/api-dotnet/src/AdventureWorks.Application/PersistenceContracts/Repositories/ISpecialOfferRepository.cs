using AdventureWorks.Domain.Entities.Sales;

namespace AdventureWorks.Application.PersistenceContracts.Repositories;

public interface ISpecialOfferRepository
{
    Task<IReadOnlyList<SpecialOffer>> ListAllAsync(CancellationToken cancellationToken = default);

    Task<SpecialOffer?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
}
