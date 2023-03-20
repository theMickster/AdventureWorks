using AdventureWorks.Domain.Entities.Person;
using AdventureWorks.Domain.Entities.Sales;
using AdventureWorks.Domain.Entities.Shield;

namespace AdventureWorks.Domain.Entities;

public class BusinessEntity : BaseEntity
{
    public int BusinessEntityId { get; set; }
    
    public Guid Rowguid { get; set; }
    
    public DateTime ModifiedDate { get; set; }

    public ICollection<PersonEntity> Persons { get; set; }
    
    public ICollection<Vendor> Vendors { get; set; }
    
    public ICollection<StoreEntity> Stores { get; set; }
    
    public ICollection<BusinessEntityAddressEntity> BusinessEntityAddresses { get; set; }
    
    public ICollection<BusinessEntityContactEntity> BusinessEntityContacts { get; set; }

    public ICollection<UserAccountEntity> UserAccounts { get; set; }
    
}