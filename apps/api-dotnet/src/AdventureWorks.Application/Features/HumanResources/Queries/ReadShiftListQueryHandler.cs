using AdventureWorks.Application.PersistenceContracts.Repositories.HumanResources;
using AdventureWorks.Models.Features.HumanResources;
using AutoMapper;
using MediatR;

namespace AdventureWorks.Application.Features.HumanResources.Queries;

public sealed class ReadShiftListQueryHandler(
    IMapper mapper,
    IShiftRepository shiftRepository)
    : IRequestHandler<ReadShiftListQuery, List<ShiftModel>>
{
    private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    private readonly IShiftRepository _repository = shiftRepository ?? throw new ArgumentNullException(nameof(shiftRepository));

    public async Task<List<ShiftModel>> Handle(ReadShiftListQuery request, CancellationToken cancellationToken)
    {
        var entities = await _repository.ListAllAsync();
        return entities is not { Count: > 0 } ? [] : _mapper.Map<List<ShiftModel>>(entities);
    }
}
