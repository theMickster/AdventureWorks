namespace AdventureWorks.Domain.Entities;

public class BusinessEntityAddressEntity : BaseEntity
{
    public int BusinessEntityId { get; set; }

    public int AddressId { get; set; }

    public int AddressTypeId { get; set; }

    public Guid Rowguid { get; set; }

    public DateTime ModifiedDate { get; set; }

    public AddressEntity Address { get; set; }

    public AddressTypeEntity AddressType { get; set; }

    public BusinessEntity BusinessEntity { get; set; }
}