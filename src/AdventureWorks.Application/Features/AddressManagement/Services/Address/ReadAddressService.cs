using AdventureWorks.Application.Features.AddressManagement.Contracts;
using AdventureWorks.Application.PersistenceContracts.Repositories;
using AdventureWorks.Common.Attributes;
using AdventureWorks.Models.Features.AddressManagement;
using AutoMapper;

namespace AdventureWorks.Application.Features.AddressManagement.Services.Address;

[ServiceLifetimeScoped]
public sealed class ReadAddressService : IReadAddressService
{
    private readonly IMapper _mapper;
    private readonly IAddressRepository _addressRepository;

    public ReadAddressService(IMapper mapper, IAddressRepository addressRepository)
    {
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _addressRepository = addressRepository ?? throw new ArgumentNullException(nameof(addressRepository));
    }

    /// <summary>
    /// Retrieve a address using its identifier.
    /// </summary>
    /// <returns>A <see cref="AddressModel"/> </returns>
    public async Task<AddressModel?> GetByIdAsync(int addressId)
    {
        var addressEntity = await _addressRepository.GetAddressByIdAsync(addressId).ConfigureAwait(false);

        return addressEntity == null ? null : _mapper.Map<AddressModel>(addressEntity);
    }
}
