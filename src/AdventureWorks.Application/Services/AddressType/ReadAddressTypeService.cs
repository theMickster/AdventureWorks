using AdventureWorks.Application.Interfaces.Repositories;
using AdventureWorks.Application.Interfaces.Services.AddressType;
using AdventureWorks.Common.Attributes;
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

}
