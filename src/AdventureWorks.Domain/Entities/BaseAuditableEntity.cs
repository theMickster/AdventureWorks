namespace AdventureWorks.Domain.Entities;

public abstract class BaseAuditableEntity : BaseEntity
{
    public int CreatedBy { get; set; }

    public DateTime CreatedOn { get; set; }

    public int ModifiedBy { get; set; }

    public DateTime ModifiedOn { get; set; }
}
