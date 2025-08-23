using AdventureWorks.Application.PersistenceContracts.Repositories.Production;
using AdventureWorks.Models.Features.Production;
using AutoMapper;
using MediatR;

namespace AdventureWorks.Application.Features.Production.Queries;

public sealed class SearchProductsQueryHandler(
    IMapper mapper,
    IProductRepository productRepository)
        : IRequestHandler<SearchProductsQuery, ProductSearchResultModel>
{
    private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    private readonly IProductRepository _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));

    public async Task<ProductSearchResultModel> Handle(SearchProductsQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var result = new ProductSearchResultModel
        {
            PageNumber = request.Parameters.PageNumber,
            PageSize = request.Parameters.PageSize,
            TotalRecords = 0
        };

        var (productEntities, totalRecords) = await _productRepository.SearchProductsAsync(request.Parameters, request.SearchModel, cancellationToken);

        if (productEntities is null or { Count: 0 })
        {
            return result;
        }

        result.Results = _mapper.Map<List<ProductListModel>>(productEntities);
        result.TotalRecords = totalRecords;

        return result;
    }
}
