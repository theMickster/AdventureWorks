namespace AdventureWorks.Domain.Entities.Production;

public class ProductModelIllustration : BaseEntity
{
    public int ProductModelId { get; set; }
    public int IllustrationId { get; set; }
    public DateTime ModifiedDate { get; set; }

    public virtual Illustration Illustration { get; set; }
    public virtual ProductModel ProductModel { get; set; }
}