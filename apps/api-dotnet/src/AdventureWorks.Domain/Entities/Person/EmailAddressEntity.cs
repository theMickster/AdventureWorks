namespace AdventureWorks.Domain.Entities.Person;

public class EmailAddressEntity : BaseEntity
{
    public int BusinessEntityId { get; set; }
    
    public int EmailAddressId { get; set; }
    
    public string EmailAddressName { get; set; }
    
    public Guid Rowguid { get; set; }
    
    public DateTime ModifiedDate { get; set; }

    public virtual PersonEntity BusinessEntity { get; set; }
}