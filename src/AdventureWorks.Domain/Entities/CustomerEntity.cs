namespace AdventureWorks.Domain.Entities;

public sealed class CustomerEntity : BaseEntity
{
    public int CustomerId { get; set; }
    
    public int? PersonId { get; set; }
    
    public int? StoreId { get; set; }
    
    public int? TerritoryId { get; set; }
    
    public string AccountNumber { get; set; }
    
    public Guid Rowguid { get; set; }
    
    public DateTime ModifiedDate { get; set; }

    public ICollection<SalesOrderHeader> SalesOrderHeaders { get; set; }
    
    public Person Person { get; set; }
    
    public StoreEntity StoreEntity { get; set; }
    
    public SalesTerritoryEntity TerritoryEntity { get; set; }
}