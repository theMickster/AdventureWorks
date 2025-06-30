using AdventureWorks.Domain.Entities.Person;

namespace AdventureWorks.Domain.Entities.Sales;

public sealed class SalesTerritoryEntity : BaseEntity
{
    public int TerritoryId { get; set; }

    public string Name { get; set; }

    public string CountryRegionCode { get; set; }

    public string Group { get; set; }
    
    public decimal SalesYtd { get; set; }
    
    public decimal SalesLastYear { get; set; }
    
    public decimal CostYtd { get; set; }
    
    public decimal CostLastYear { get; set; }
    
    public Guid Rowguid { get; set; }
    
    public DateTime ModifiedDate { get; set; }

    public ICollection<CustomerEntity> Customers { get; set; }
    
    public ICollection<SalesOrderHeader> SalesOrderHeaders { get; set; }
    
    public ICollection<SalesPersonEntity> SalesPeople { get; set; }
    
    public ICollection<SalesTerritoryHistory> SalesTerritoryHistory { get; set; }
    
    public ICollection<StateProvinceEntity> StateProvinces { get; set; }
    
    public CountryRegionEntity CountryRegion { get; set; }
}