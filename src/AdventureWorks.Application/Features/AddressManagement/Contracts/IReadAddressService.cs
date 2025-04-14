using AdventureWorks.Models.Features.AddressManagement;

namespace AdventureWorks.Application.Features.AddressManagement.Contracts;
public interface IReadAddressService
{
    /// <summary>
    /// Retrieve a address using its identifier.
    /// </summary>
    /// <returns>A <see cref="AddressModel"/> </returns>
    Task<AddressModel?> GetByIdAsync(int addressId);
}
