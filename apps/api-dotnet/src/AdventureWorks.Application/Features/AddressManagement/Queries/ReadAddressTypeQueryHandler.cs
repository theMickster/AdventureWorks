using AdventureWorks.Application.PersistenceContracts.Repositories;
using AdventureWorks.Models.Features.AddressManagement;
using AutoMapper;
using MediatR;

namespace AdventureWorks.Application.Features.AddressManagement.Queries;

public sealed class ReadAddressTypeQueryHandler(
    IMapper mapper,
    IAddressTypeRepository addressTypeRepository
    ) : IRequestHandler<ReadAddressTypeQuery, AddressTypeModel>
{
    private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    private readonly IAddressTypeRepository _repository = addressTypeRepository ?? throw new ArgumentNullException(nameof(addressTypeRepository));

    public async Task<AddressTypeModel> Handle(ReadAddressTypeQuery request, CancellationToken cancellationToken)
    {
        return _mapper.Map<AddressTypeModel>(await _repository.GetByIdAsync(request.Id));
    }
}
