using AdventureWorks.Application.PersistenceContracts.Repositories;
using AdventureWorks.Models.Features.Sales;
using AutoMapper;
using MediatR;

namespace AdventureWorks.Application.Features.Sales.Queries;

public sealed class ReadSpecialOfferQueryHandler(
    IMapper mapper,
    ISpecialOfferRepository specialOfferRepository)
        : IRequestHandler<ReadSpecialOfferQuery, SpecialOfferModel?>
{
    private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    private readonly ISpecialOfferRepository _repository = specialOfferRepository ?? throw new ArgumentNullException(nameof(specialOfferRepository));

    public async Task<SpecialOfferModel?> Handle(ReadSpecialOfferQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return _mapper.Map<SpecialOfferModel>(await _repository.GetByIdAsync(request.Id, cancellationToken));
    }
}
