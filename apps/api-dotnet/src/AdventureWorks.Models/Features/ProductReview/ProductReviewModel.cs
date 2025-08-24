namespace AdventureWorks.Models.Features.ProductReview;

/// <summary>
/// Represents a product review data transfer object.
/// </summary>
public sealed class ProductReviewModel
{
    public int ProductReviewId { get; set; }

    public int ProductId { get; set; }

    public string ReviewerName { get; set; }

    public DateTime ReviewDate { get; set; }

    public string EmailAddress { get; set; }

    public int Rating { get; set; }

    public string Comments { get; set; }

    public DateTime ModifiedDate { get; set; }
}
