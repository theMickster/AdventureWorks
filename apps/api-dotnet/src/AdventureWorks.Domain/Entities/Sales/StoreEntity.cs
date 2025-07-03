using AdventureWorks.Domain.Entities.Person;

namespace AdventureWorks.Domain.Entities.Sales;

public sealed class StoreEntity : BaseEntity
{
    public int BusinessEntityId { get; set; }

    public string Name { get; set; }

    public int? SalesPersonId { get; set; }

    public string Demographics { get; set; }

    public Guid Rowguid { get; set; }

    public DateTime ModifiedDate { get; set; }

    public ICollection<CustomerEntity> Customers { get; set; }

    public BusinessEntity StoreBusinessEntity { get; set; }

    public SalesPersonEntity PrimarySalesPerson { get; set; }

}