using AdventureWorks.Domain.Entities;

namespace AdventureWorks.Application.Interfaces.Repositories;
public interface IAddressRepository : IAsyncRepository<AddressEntity>
{
    /// <summary>
    /// Retrieve an address from the DbContext by its unique id
    /// </summary>
    /// <param name="addressId"></param>
    /// <returns></returns>
    Task<AddressEntity?> GetAddressByIdAsync(int addressId);
}
