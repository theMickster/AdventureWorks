using AdventureWorks.Application.PersistenceContracts.Repositories;
using AdventureWorks.Models.Features.AddressManagement;
using AutoMapper;
using MediatR;

namespace AdventureWorks.Application.Features.AddressManagement.Queries;

public sealed class ReadAddressTypeListQueryHandler(
    IMapper mapper,
    IAddressTypeRepository addressTypeRepository)
    : IRequestHandler<ReadAddressTypeListQuery, List<AddressTypeModel>> 
{
    private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    private readonly IAddressTypeRepository _repository = addressTypeRepository ?? throw new ArgumentNullException(nameof(addressTypeRepository));


    public async Task<List<AddressTypeModel>> Handle(ReadAddressTypeListQuery request, CancellationToken cancellationToken)
    {
        var entities = await _repository.ListAllAsync();
        return entities is not { Count: > 0 } ? [] : _mapper.Map<List<AddressTypeModel>>(entities);
    }
}
