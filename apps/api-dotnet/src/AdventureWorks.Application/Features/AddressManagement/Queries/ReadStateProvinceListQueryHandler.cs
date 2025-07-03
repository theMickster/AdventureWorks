using AdventureWorks.Application.PersistenceContracts.Repositories;
using AdventureWorks.Models.Features.AddressManagement;
using AutoMapper;
using MediatR;

namespace AdventureWorks.Application.Features.AddressManagement.Queries;

public sealed class ReadStateProvinceListQueryHandler(
    IMapper mapper,
    IStateProvinceRepository repository)
        : IRequestHandler<ReadStateProvinceListQuery, List<StateProvinceModel>>
{
    private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    private readonly IStateProvinceRepository _repository = repository ?? throw new ArgumentNullException(nameof(repository));

    public async Task<List<StateProvinceModel>> Handle(ReadStateProvinceListQuery request, CancellationToken cancellationToken)
    {
        var entities = await _repository.ListAllAsync();
        return entities is not { Count: > 0 } ? [] : _mapper.Map<List<StateProvinceModel>>(entities);
    }
}
