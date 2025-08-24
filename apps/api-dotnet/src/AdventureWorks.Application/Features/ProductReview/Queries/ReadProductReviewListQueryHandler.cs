using AdventureWorks.Application.PersistenceContracts.Repositories;
using AdventureWorks.Models.Features.ProductReview;
using AutoMapper;
using MediatR;

namespace AdventureWorks.Application.Features.ProductReview.Queries;

/// <summary>
/// Handles the retrieval of a paginated list of product reviews for a given product.
/// </summary>
public sealed class ReadProductReviewListQueryHandler(
    IMapper mapper,
    IProductReviewRepository productReviewRepository)
        : IRequestHandler<ReadProductReviewListQuery, ProductReviewSearchResultModel>
{
    private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    private readonly IProductReviewRepository _productReviewRepository = productReviewRepository ?? throw new ArgumentNullException(nameof(productReviewRepository));

    /// <summary>
    /// Handles the query to retrieve a paginated list of product reviews.
    /// </summary>
    /// <param name="request">the query request</param>
    /// <param name="cancellationToken">token to cancel the operation</param>
    public async Task<ProductReviewSearchResultModel> Handle(ReadProductReviewListQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var result = new ProductReviewSearchResultModel
        {
            PageNumber = request.Parameters.PageNumber,
            PageSize = request.Parameters.PageSize,
            TotalRecords = 0
        };

        var (reviewEntities, totalRecords) = await _productReviewRepository.GetProductReviewsByProductIdAsync(
            request.ProductId, request.Parameters, cancellationToken);

        if (reviewEntities is null or { Count: 0 })
        {
            return result;
        }

        result.Results = _mapper.Map<List<ProductReviewModel>>(reviewEntities);
        result.TotalRecords = totalRecords;

        return result;
    }
}
