using AdventureWorks.Application.PersistenceContracts.Repositories;
using AdventureWorks.Models.Features.ProductReview;
using AutoMapper;
using MediatR;

namespace AdventureWorks.Application.Features.ProductReview.Queries;

/// <summary>
/// Handles the retrieval of a single product review by its unique identifier.
/// </summary>
public sealed class ReadProductReviewQueryHandler(
    IMapper mapper,
    IProductReviewRepository productReviewRepository)
        : IRequestHandler<ReadProductReviewQuery, ProductReviewModel?>
{
    private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    private readonly IProductReviewRepository _productReviewRepository = productReviewRepository ?? throw new ArgumentNullException(nameof(productReviewRepository));

    /// <summary>
    /// Handles the query to retrieve a single product review.
    /// </summary>
    /// <param name="request">the query request</param>
    /// <param name="cancellationToken">token to cancel the operation</param>
    public async Task<ProductReviewModel?> Handle(ReadProductReviewQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var entity = await _productReviewRepository.GetByIdAsync(request.Id, cancellationToken);

        if (entity is null)
        {
            return null;
        }

        return _mapper.Map<ProductReviewModel>(entity);
    }
}
