namespace AdventureWorks.Domain.Entities;

public sealed class StateProvinceEntity : BaseEntity
{
    public int StateProvinceId { get; set; }

    public string StateProvinceCode { get; set; }
    
    public string CountryRegionCode { get; set; }
    
    public bool IsOnlyStateProvinceFlag { get; set; }
    
    public string Name { get; set; }
    
    public int TerritoryId { get; set; }
    
    public Guid Rowguid { get; set; }
    
    public DateTime ModifiedDate { get; set; }

    public ICollection<AddressEntity> Addresses { get; set; }
    
    public ICollection<SalesTaxRateEntity> SalesTaxRates { get; set; }
    
    public CountryRegionEntity CountryRegion { get; set; }
    
    public SalesTerritoryEntity SalesTerritory { get; set; }
}