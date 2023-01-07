namespace AdventureWorks.Domain.Entities;

public class Culture : BaseEntity
{
    public string CultureId { get; set; }
    public string Name { get; set; }
    public DateTime ModifiedDate { get; set; }

    public virtual ICollection<ProductModelProductDescriptionCulture> ProductModelProductDescriptionCulture { get; set; }
}