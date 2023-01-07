namespace AdventureWorks.Domain.Entities;

public sealed class CountryRegionEntity : BaseEntity
{
    public string CountryRegionCode { get; set; }

    public string Name { get; set; }

    public DateTime ModifiedDate { get; set; }

    //public ICollection<CountryRegionCurrency> CountryRegionCurrencies { get; set; }

    //public ICollection<SalesTerritoryEntity> SalesTerritories { get; set; }

    public ICollection<StateProvinceEntity> StateProvinces { get; set; }

}