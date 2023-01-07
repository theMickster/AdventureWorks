namespace AdventureWorks.Domain.Entities;

public class SalesTerritoryHistory : BaseEntity
{
    public int BusinessEntityId { get; set; }
    public int TerritoryId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public Guid Rowguid { get; set; }
    public DateTime ModifiedDate { get; set; }

    public virtual SalesPerson BusinessEntity { get; set; }
    public virtual SalesTerritory Territory { get; set; }
}