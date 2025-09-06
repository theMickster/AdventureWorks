using AdventureWorks.Application.PersistenceContracts.Repositories;
using AdventureWorks.Models.Features.Production;
using AutoMapper;
using MediatR;

namespace AdventureWorks.Application.Features.Production.Queries;

public sealed class ReadLocationQueryHandler(
    IMapper mapper,
    ILocationRepository locationRepository)
        : IRequestHandler<ReadLocationQuery, LocationModel?>
{
    private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    private readonly ILocationRepository _repository = locationRepository ?? throw new ArgumentNullException(nameof(locationRepository));

    public async Task<LocationModel?> Handle(ReadLocationQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return _mapper.Map<LocationModel>(await _repository.GetByIdAsync(request.Id, cancellationToken));
    }
}
