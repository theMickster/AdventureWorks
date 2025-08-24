namespace AdventureWorks.Models.Features.ProductReview;

/// <summary>Represents the input model for updating an existing product review.</summary>
public sealed class ProductReviewUpdateModel : ProductReviewBaseModel
{
    public int Id { get; set; }
}
