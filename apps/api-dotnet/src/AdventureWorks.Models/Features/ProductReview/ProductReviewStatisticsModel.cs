namespace AdventureWorks.Models.Features.ProductReview;

public sealed class ProductReviewStatisticsModel
{
    public int ProductId { get; set; }

    public int TotalReviews { get; set; }

    public double AverageRating { get; set; }

    public IReadOnlyDictionary<int, int> RatingDistribution { get; set; } = new Dictionary<int, int>();
}
