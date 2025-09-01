namespace AdventureWorks.Domain.Entities.Sales;

public class StoreSalesPersonHistoryEntity : BaseEntity
{
    public int BusinessEntityId { get; set; }

    public int SalesPersonId { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public DateTime ModifiedDate { get; set; }

    public Guid Rowguid { get; set; }

    public virtual StoreEntity Store { get; set; }

    public virtual SalesPersonEntity SalesPerson { get; set; }
}
