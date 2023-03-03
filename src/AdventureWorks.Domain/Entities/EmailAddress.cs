using AdventureWorks.Domain.Entities.Person;

namespace AdventureWorks.Domain.Entities;

public class EmailAddress : BaseEntity
{
    public int BusinessEntityId { get; set; }
    
    public int EmailAddressId { get; set; }
    
    public string EmailAddressName { get; set; }
    
    public Guid Rowguid { get; set; }
    
    public DateTime ModifiedDate { get; set; }

    public virtual PersonEntity BusinessEntity { get; set; }
}