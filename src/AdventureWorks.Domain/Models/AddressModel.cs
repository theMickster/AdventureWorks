namespace AdventureWorks.Domain.Models;

public sealed class AddressModel
{
    public int Id { get; set; }

    public string AddressLine1 { get; set; }
    
    public string AddressLine2 { get; set; }
    
    public string City { get; set; }

    public int StateProvinceId { get; set; }

    public string StateProvinceCode { get; set; }

    public string CountryRegionCode { get; set; }

    public string CountryRegionName { get; set; }

    public string PostalCode { get; set; }

    public DateTime ModifiedDate { get; set; }
}