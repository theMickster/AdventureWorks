namespace AdventureWorks.Models.Features.ProductReview;

/// <summary>
/// Shared fields for product review create and update operations.
/// </summary>
public class ProductReviewBaseModel
{
    public required string ReviewerName { get; set; }

    public required string EmailAddress { get; set; }

    public int Rating { get; set; }

    public string? Comments { get; set; }
}
