namespace AdventureWorks.Models.Features.Sales;

public sealed class StoreAddressModel
{
    public int Id { get; set; }

    public int StoreId { get; set; }

    public int AddressTypeId { get; set; }

    public string AddressTypeName { get; set; } = string.Empty;

    public string AddressLine1 { get; set; } = string.Empty;

    public string? AddressLine2 { get; set; }

    public string City { get; set; } = string.Empty;

    public string StateProvinceCode { get; set; } = string.Empty;

    public string StateProvinceName { get; set; } = string.Empty;

    public string CountryRegionCode { get; set; } = string.Empty;

    public string CountryRegionName { get; set; } = string.Empty;

    public string PostalCode { get; set; } = string.Empty;

    public DateTime ModifiedDate { get; set; }
}
