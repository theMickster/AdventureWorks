using AdventureWorks.Application.PersistenceContracts.Repositories.HumanResources;
using AdventureWorks.Models.Features.HumanResources;
using AutoMapper;
using MediatR;

namespace AdventureWorks.Application.Features.HumanResources.Queries;

public sealed class ReadShiftQueryHandler(
    IMapper mapper,
    IShiftRepository shiftRepository)
        : IRequestHandler<ReadShiftQuery, ShiftModel>
{
    private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    private readonly IShiftRepository _repository = shiftRepository ?? throw new ArgumentNullException(nameof(shiftRepository));

    public async Task<ShiftModel> Handle(ReadShiftQuery request, CancellationToken cancellationToken)
    {
        return _mapper.Map<ShiftModel>(await _repository.GetByIdAsync(request.Id));
    }
}
