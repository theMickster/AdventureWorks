namespace AdventureWorks.Domain.Entities;

public sealed class SalesPerson : BaseEntity
{
    public int BusinessEntityId { get; set; }
    
    public int? TerritoryId { get; set; }
    
    public decimal? SalesQuota { get; set; }
    
    public decimal Bonus { get; set; }
    
    public decimal CommissionPct { get; set; }
    
    public decimal SalesYtd { get; set; }
    
    public decimal SalesLastYear { get; set; }
    
    public Guid Rowguid { get; set; }
    
    public DateTime ModifiedDate { get; set; }

    public ICollection<SalesOrderHeader> SalesOrderHeaders { get; set; }
    
    public ICollection<SalesPersonQuotaHistory> SalesPersonQuotaHistory { get; set; }
    
    public ICollection<SalesTerritoryHistory> SalesTerritoryHistory { get; set; }
    
    public ICollection<StoreEntity> Stores { get; set; }
    
    public Employee BusinessEntity { get; set; }
    
    public SalesTerritoryEntity TerritoryEntity { get; set; }
}