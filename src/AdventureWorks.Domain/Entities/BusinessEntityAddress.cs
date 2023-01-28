namespace AdventureWorks.Domain.Entities;

public class BusinessEntityAddress : BaseEntity
{
    public int BusinessEntityId { get; set; }
    public int AddressId { get; set; }
    public int AddressTypeId { get; set; }
    public Guid Rowguid { get; set; }
    public DateTime ModifiedDate { get; set; }

    public virtual AddressEntity AddressEntity { get; set; }
    public virtual AddressTypeEntity AddressTypeEntity { get; set; }
    public virtual BusinessEntity BusinessEntity { get; set; }
}