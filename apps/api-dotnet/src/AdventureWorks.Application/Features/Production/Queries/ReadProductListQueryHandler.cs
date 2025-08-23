using AdventureWorks.Application.PersistenceContracts.Repositories.Production;
using AdventureWorks.Models.Features.Production;
using AutoMapper;
using MediatR;

namespace AdventureWorks.Application.Features.Production.Queries;

public sealed class ReadProductListQueryHandler(
    IMapper mapper,
    IProductRepository productRepository)
        : IRequestHandler<ReadProductListQuery, ProductSearchResultModel>
{
    private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    private readonly IProductRepository _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));

    public async Task<ProductSearchResultModel> Handle(ReadProductListQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var result = new ProductSearchResultModel
        {
            PageNumber = request.Parameters.PageNumber,
            PageSize = request.Parameters.PageSize,
            TotalRecords = 0
        };

        var (productEntities, totalRecords) = await _productRepository.GetProductsAsync(request.Parameters, cancellationToken);

        if (productEntities is null or { Count: 0 })
        {
            return result;
        }

        result.Results = _mapper.Map<List<ProductListModel>>(productEntities);
        result.TotalRecords = totalRecords;

        return result;
    }
}
