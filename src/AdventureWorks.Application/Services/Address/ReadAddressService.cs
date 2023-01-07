using AdventureWorks.Application.Interfaces.Services.Address;
using AdventureWorks.Domain.Models;
using AutoMapper;

namespace AdventureWorks.Application.Services.Address;

public sealed class ReadAddressService: IReadAddressService
{
    private readonly IMapper _mapper;

    public ReadAddressService(IMapper mapper)
    {
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    /// <summary>
    /// Retrieve a address using its identifier.
    /// </summary>
    /// <returns>A <see cref="AddressModel"/> </returns>
    public Task<AddressModel> GetByIdAsync(int addressId)
    {
        throw new NotImplementedException();
    }
}
