using AdventureWorks.Application.PersistenceContracts.Repositories;
using AdventureWorks.Models.Features.ProductReview;
using MediatR;

namespace AdventureWorks.Application.Features.ProductReview.Queries;

/// <summary>
/// Handles the retrieval of rating statistics for a given product's reviews.
/// </summary>
public sealed class ReadProductReviewStatisticsQueryHandler(
    IProductReviewRepository productReviewRepository)
    : IRequestHandler<ReadProductReviewStatisticsQuery, ProductReviewStatisticsModel>
{
    private readonly IProductReviewRepository _productReviewRepository =
        productReviewRepository ?? throw new ArgumentNullException(nameof(productReviewRepository));

    /// <summary>
    /// Handles the query to retrieve rating statistics for a product's reviews.
    /// </summary>
    /// <param name="request">the query request</param>
    /// <param name="cancellationToken">token to cancel the operation</param>
    public async Task<ProductReviewStatisticsModel> Handle(
        ReadProductReviewStatisticsQuery request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var rawDistribution = await _productReviewRepository.GetRatingDistributionByProductIdAsync(request.ProductId, cancellationToken);

        var distribution = Enumerable.Range(1, 5)
            .ToDictionary(r => r, r => rawDistribution.GetValueOrDefault(r, 0));

        var totalReviews = distribution.Values.Sum();
        var averageRating = totalReviews == 0 ? 0.0 : Math.Round(
            distribution.Sum(kv => kv.Key * kv.Value) / (double)totalReviews, 2);

        return new ProductReviewStatisticsModel
        {
            ProductId = request.ProductId,
            TotalReviews = totalReviews,
            AverageRating = averageRating,
            RatingDistribution = distribution
        };
    }
}
