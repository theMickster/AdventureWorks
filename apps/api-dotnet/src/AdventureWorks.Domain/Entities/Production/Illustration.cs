namespace AdventureWorks.Domain.Entities.Production;

public class Illustration : BaseEntity
{

    public int IllustrationId { get; set; }
    public string Diagram { get; set; }
    public DateTime ModifiedDate { get; set; }

    public virtual ICollection<ProductModelIllustration> ProductModelIllustration { get; set; }
}