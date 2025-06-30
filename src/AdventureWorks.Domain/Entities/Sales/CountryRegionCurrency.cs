using AdventureWorks.Domain.Entities.Person;

namespace AdventureWorks.Domain.Entities.Sales;

public class CountryRegionCurrency : BaseEntity
{
    public string CountryRegionCode { get; set; }
    public string CurrencyCode { get; set; }
    public DateTime ModifiedDate { get; set; }

    public virtual CountryRegionEntity CountryRegionEntityCodeNavigation { get; set; }
    public virtual Currency CurrencyCodeNavigation { get; set; }
}