using AdventureWorks.Domain.Entities.Purchasing;
using AdventureWorks.Domain.Entities.Sales;

namespace AdventureWorks.Domain.Entities.Person;

public class BusinessEntity : BaseEntity
{
    public int BusinessEntityId { get; set; }
    
    public Guid Rowguid { get; set; }
    
    public bool IsEntraUser { get; set; }
    
    public DateTime ModifiedDate { get; set; }
    
    public ICollection<PersonEntity> Persons { get; set; }
    
    public ICollection<Vendor> Vendors { get; set; }
    
    public ICollection<StoreEntity> Stores { get; set; }
    
    public ICollection<BusinessEntityAddressEntity> BusinessEntityAddresses { get; set; }
    
    public ICollection<BusinessEntityContactEntity> BusinessEntityContacts { get; set; }
    
}