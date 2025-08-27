using AdventureWorks.Domain.Entities.Person;

namespace AdventureWorks.Application.PersistenceContracts.Repositories;
public interface IAddressRepository : IAsyncRepository<AddressEntity>
{
    /// <summary>
    /// Retrieve an address from the DbContext by its unique id
    /// </summary>
    /// <param name="addressId"></param>
    /// <returns></returns>
    Task<AddressEntity?> GetAddressByIdAsync(int addressId);

    /// <summary>
    /// Returns true if an address with the given id exists.
    /// </summary>
    /// <param name="addressId">the unique address identifier</param>
    /// <param name="cancellationToken">token to cancel the operation</param>
    Task<bool> ExistsAsync(int addressId, CancellationToken cancellationToken = default);
}
