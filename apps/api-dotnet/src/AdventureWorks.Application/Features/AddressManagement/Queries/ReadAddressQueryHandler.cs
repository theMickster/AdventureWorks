using AdventureWorks.Application.PersistenceContracts.Repositories;
using AdventureWorks.Models.Features.AddressManagement;
using AutoMapper;
using MediatR;

namespace AdventureWorks.Application.Features.AddressManagement.Queries;

public sealed class ReadAddressQueryHandler
    (IMapper mapper, IAddressRepository repository)
        : IRequestHandler<ReadAddressQuery, AddressModel>
{
    private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    private readonly IAddressRepository _repository = repository ?? throw new ArgumentNullException(nameof(repository));

    public async Task<AddressModel> Handle(ReadAddressQuery request, CancellationToken cancellationToken)
    {
        return _mapper.Map<AddressModel>(await _repository.GetAddressByIdAsync(request.Id));
    }
}