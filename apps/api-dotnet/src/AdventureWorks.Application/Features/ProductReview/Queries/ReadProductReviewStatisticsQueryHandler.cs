using AdventureWorks.Application.PersistenceContracts.Repositories;
using AdventureWorks.Models.Features.ProductReview;
using MediatR;

namespace AdventureWorks.Application.Features.ProductReview.Queries;

public sealed class ReadProductReviewStatisticsQueryHandler(
    IProductReviewRepository productReviewRepository)
    : IRequestHandler<ReadProductReviewStatisticsQuery, ProductReviewStatisticsModel>
{
    private readonly IProductReviewRepository _productReviewRepository =
        productReviewRepository ?? throw new ArgumentNullException(nameof(productReviewRepository));

    public async Task<ProductReviewStatisticsModel> Handle(
        ReadProductReviewStatisticsQuery request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var ratings = await _productReviewRepository.GetRatingsByProductIdAsync(request.ProductId, cancellationToken);

        var distribution = Enumerable.Range(1, 5)
            .ToDictionary(r => r, r => ratings.Count(x => x == r));

        return new ProductReviewStatisticsModel
        {
            ProductId = request.ProductId,
            TotalReviews = ratings.Count,
            AverageRating = ratings.Count == 0 ? 0.0 : Math.Round(ratings.Average(), 2),
            RatingDistribution = distribution
        };
    }
}
