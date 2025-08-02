namespace AdventureWorks.Models.Features.AddressManagement;

public sealed class AddressModel : AddressBaseModel
{
    public int Id { get; set; }

    public CountryRegionModel CountryRegion { get; set; } = new();

    public DateTime ModifiedDate { get; set; }
}