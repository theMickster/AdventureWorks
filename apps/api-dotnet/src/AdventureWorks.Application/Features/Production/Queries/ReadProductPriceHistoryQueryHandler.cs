using AdventureWorks.Application.PersistenceContracts.Repositories.Production;
using AdventureWorks.Models.Features.Production;
using AutoMapper;
using MediatR;

namespace AdventureWorks.Application.Features.Production.Queries;

public sealed class ReadProductPriceHistoryQueryHandler(
    IMapper mapper,
    IProductRepository productRepository)
        : IRequestHandler<ReadProductPriceHistoryQuery, List<ProductPriceHistoryModel>>
{
    private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    private readonly IProductRepository _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));

    public async Task<List<ProductPriceHistoryModel>> Handle(ReadProductPriceHistoryQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var listPriceTask = _productRepository.GetListPriceHistoryByProductIdAsync(request.ProductId, cancellationToken);
        var costTask = _productRepository.GetCostHistoryByProductIdAsync(request.ProductId, cancellationToken);
        await Task.WhenAll(listPriceTask, costTask);

        var listPriceModels = _mapper.Map<List<ProductPriceHistoryModel>>(await listPriceTask);
        var costModels = _mapper.Map<List<ProductPriceHistoryModel>>(await costTask);

        var combined = listPriceModels.Concat(costModels)
            .OrderBy(h => h.StartDate)
            .ThenBy(h => h.Type)
            .ToList();

        return combined;
    }
}
