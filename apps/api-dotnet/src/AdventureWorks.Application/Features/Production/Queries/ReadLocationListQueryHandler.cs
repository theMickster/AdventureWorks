using AdventureWorks.Application.PersistenceContracts.Repositories;
using AdventureWorks.Models.Features.Production;
using AutoMapper;
using MediatR;

namespace AdventureWorks.Application.Features.Production.Queries;

public sealed class ReadLocationListQueryHandler(
    IMapper mapper,
    ILocationRepository locationRepository)
        : IRequestHandler<ReadLocationListQuery, List<LocationModel>>
{
    private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    private readonly ILocationRepository _repository = locationRepository ?? throw new ArgumentNullException(nameof(locationRepository));

    public async Task<List<LocationModel>> Handle(ReadLocationListQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var entities = await _repository.ListAllAsync(cancellationToken);
        return entities is not { Count: > 0 } ? [] : _mapper.Map<List<LocationModel>>(entities);
    }
}
