namespace AdventureWorks.Domain.Entities.Person;

public sealed class AddressTypeEntity : BaseEntity
{
    public int AddressTypeId { get; set; }

    public string Name { get; set; }

    public Guid Rowguid { get; set; }

    public DateTime ModifiedDate { get; set; }

    public ICollection<BusinessEntityAddressEntity> BusinessEntityAddresses { get; set; }
}