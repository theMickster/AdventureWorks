using AdventureWorks.Application.PersistenceContracts.Repositories;
using AdventureWorks.Models.Features.Production;
using AutoMapper;
using MediatR;

namespace AdventureWorks.Application.Features.Production.Queries;

public sealed class ReadUnitMeasureListQueryHandler(
    IMapper mapper,
    IUnitMeasureRepository unitMeasureRepository)
        : IRequestHandler<ReadUnitMeasureListQuery, List<UnitMeasureModel>>
{
    private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    private readonly IUnitMeasureRepository _repository = unitMeasureRepository ?? throw new ArgumentNullException(nameof(unitMeasureRepository));

    public async Task<List<UnitMeasureModel>> Handle(ReadUnitMeasureListQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var entities = await _repository.ListAllAsync(cancellationToken);
        return entities is not { Count: > 0 } ? [] : _mapper.Map<List<UnitMeasureModel>>(entities);
    }
}
