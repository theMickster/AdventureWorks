using AdventureWorks.Domain.Models;

namespace AdventureWorks.Application.Interfaces.Services.Address;
public interface IReadAddressService
{
    /// <summary>
    /// Retrieve a address using its identifier.
    /// </summary>
    /// <returns>A <see cref="AddressModel"/> </returns>
    Task<AddressModel> GetByIdAsync(int addressId);
}
