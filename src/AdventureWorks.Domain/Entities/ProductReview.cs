namespace AdventureWorks.Domain.Entities;

public class ProductReview : BaseEntity
{
    public int ProductReviewId { get; set; }
    public int ProductId { get; set; }
    public string ReviewerName { get; set; }
    public DateTime ReviewDate { get; set; }
    public string EmailAddress { get; set; }
    public int Rating { get; set; }
    public string Comments { get; set; }
    public DateTime ModifiedDate { get; set; }

    public virtual Product Product { get; set; }
}