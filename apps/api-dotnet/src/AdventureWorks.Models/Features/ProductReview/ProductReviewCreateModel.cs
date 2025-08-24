namespace AdventureWorks.Models.Features.ProductReview;

/// <summary>
/// Represents the input model for creating a new product review.
/// </summary>
public sealed class ProductReviewCreateModel : ProductReviewBaseModel
{
    public int ProductId { get; set; }
}
