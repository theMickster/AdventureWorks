using AdventureWorks.Application.PersistenceContracts.Repositories.Sales;
using AdventureWorks.Models.Features.Sales;
using AutoMapper;
using MediatR;

namespace AdventureWorks.Application.Features.Sales.Queries;

/// <summary>
/// Handler for <see cref="ReadStoreDemographicsQuery"/>.
/// Loads the store's demographics XML via a narrow projection, then populates the
/// response model with parsed survey fields. Returns <c>null</c> when the store does not exist;
/// returns a model with only <c>StoreId</c>/<c>StoreName</c> populated when Demographics is null
/// or malformed.
/// </summary>
public sealed class ReadStoreDemographicsQueryHandler(IMapper mapper, IStoreRepository storeRepository)
    : IRequestHandler<ReadStoreDemographicsQuery, StoreDemographicsModel?>
{
    private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    private readonly IStoreRepository _storeRepository = storeRepository ?? throw new ArgumentNullException(nameof(storeRepository));

    public async Task<StoreDemographicsModel?> Handle(ReadStoreDemographicsQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var projection = await _storeRepository.GetDemographicsAsync(request.StoreId, cancellationToken);
        if (projection is null)
        {
            return null;
        }

        var model = _mapper.Map<StoreDemographicsModel>(projection);
        StoreDemographicsParser.Populate(model, projection.Demographics);

        return model;
    }
}
