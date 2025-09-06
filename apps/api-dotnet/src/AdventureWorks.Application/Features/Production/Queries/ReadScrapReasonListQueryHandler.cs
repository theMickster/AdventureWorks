using AdventureWorks.Application.PersistenceContracts.Repositories;
using AdventureWorks.Models.Features.Production;
using AutoMapper;
using MediatR;

namespace AdventureWorks.Application.Features.Production.Queries;

public sealed class ReadScrapReasonListQueryHandler(
    IMapper mapper,
    IScrapReasonRepository scrapReasonRepository)
        : IRequestHandler<ReadScrapReasonListQuery, List<ScrapReasonModel>>
{
    private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    private readonly IScrapReasonRepository _repository = scrapReasonRepository ?? throw new ArgumentNullException(nameof(scrapReasonRepository));

    public async Task<List<ScrapReasonModel>> Handle(ReadScrapReasonListQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var entities = await _repository.ListAllAsync(cancellationToken);
        return entities is not { Count: > 0 } ? [] : _mapper.Map<List<ScrapReasonModel>>(entities);
    }
}
