using AdventureWorks.Application.Interfaces.Repositories;
using AdventureWorks.Application.Interfaces.Services.AddressType;
using AdventureWorks.Common.Attributes;
using AdventureWorks.Domain.Models;
using AutoMapper;

namespace AdventureWorks.Application.Services.AddressType;

[ServiceLifetimeScoped]
public sealed class ReadAddressTypeService : IReadAddressTypeService
{
    private readonly IMapper _mapper;
    private readonly IAddressTypeRepository _repository;

    public ReadAddressTypeService (
        IMapper mapper,
        IAddressTypeRepository addressTypeRepository)
    {
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _repository = addressTypeRepository ?? throw new ArgumentNullException(nameof(addressTypeRepository));
    }

    /// <summary>
    /// Retrieve an address type using its identifier.
    /// </summary>
    /// <returns>A <see cref="AddressTypeModel"/> </returns>
    public async Task<AddressTypeModel?> GetByIdAsync(int id)
    {
        var entity = await _repository.GetByIdAsync(id).ConfigureAwait(false);

        return _mapper.Map<AddressTypeModel>(entity);
    }

    /// <summary>
    /// Retrieve the list of address types. 
    /// </summary>
    /// <returns></returns>
    public async Task<List<AddressTypeModel>> GetListAsync()
    {
        var entities = await _repository.ListAllAsync().ConfigureAwait(false);

        if (entities == null || !entities.Any())
        {
            return new List<AddressTypeModel>();
        }

        return _mapper.Map<List<AddressTypeModel>>(entities);
    }

}
