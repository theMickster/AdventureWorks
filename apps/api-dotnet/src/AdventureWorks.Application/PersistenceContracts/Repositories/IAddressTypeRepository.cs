using AdventureWorks.Domain.Entities;
using AdventureWorks.Domain.Entities.Person;

namespace AdventureWorks.Application.PersistenceContracts.Repositories;

public interface IAddressTypeRepository : IReadOnlyAsyncRepository<AddressTypeEntity>
{
    /// <summary>
    /// Returns true if an address type with the given id exists.
    /// </summary>
    /// <param name="addressTypeId">the unique address type identifier</param>
    /// <param name="cancellationToken">token to cancel the operation</param>
    Task<bool> ExistsAsync(int addressTypeId, CancellationToken cancellationToken = default);
}
