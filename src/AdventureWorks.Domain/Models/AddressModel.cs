namespace AdventureWorks.Domain.Models;

public sealed class AddressModel
{
    public int Id { get; set; }

    public string AddressLine1 { get; set; }
    
    public string AddressLine2 { get; set; }
    
    public string City { get; set; }

    public StateProvinceModel StateProvince { get; set; }

    public CountryRegionModel CountryRegion { get; set; }

    public string PostalCode { get; set; }

    public DateTime ModifiedDate { get; set; }
}