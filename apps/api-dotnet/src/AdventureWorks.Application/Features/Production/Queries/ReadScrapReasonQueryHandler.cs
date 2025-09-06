using AdventureWorks.Application.PersistenceContracts.Repositories;
using AdventureWorks.Models.Features.Production;
using AutoMapper;
using MediatR;

namespace AdventureWorks.Application.Features.Production.Queries;

public sealed class ReadScrapReasonQueryHandler(
    IMapper mapper,
    IScrapReasonRepository scrapReasonRepository)
        : IRequestHandler<ReadScrapReasonQuery, ScrapReasonModel?>
{
    private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    private readonly IScrapReasonRepository _repository = scrapReasonRepository ?? throw new ArgumentNullException(nameof(scrapReasonRepository));

    public async Task<ScrapReasonModel?> Handle(ReadScrapReasonQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return _mapper.Map<ScrapReasonModel>(await _repository.GetByIdAsync(request.Id, cancellationToken));
    }
}
