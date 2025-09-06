using AdventureWorks.Application.PersistenceContracts.Repositories;
using AdventureWorks.Models.Features.Production;
using AutoMapper;
using MediatR;

namespace AdventureWorks.Application.Features.Production.Queries;

public sealed class ReadUnitMeasureQueryHandler(
    IMapper mapper,
    IUnitMeasureRepository unitMeasureRepository)
        : IRequestHandler<ReadUnitMeasureQuery, UnitMeasureModel?>
{
    private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    private readonly IUnitMeasureRepository _repository = unitMeasureRepository ?? throw new ArgumentNullException(nameof(unitMeasureRepository));

    public async Task<UnitMeasureModel?> Handle(ReadUnitMeasureQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return _mapper.Map<UnitMeasureModel>(await _repository.GetByCodeAsync(request.Code, cancellationToken));
    }
}
