namespace AdventureWorks.Models.Features.AddressManagement;

public sealed class BusinessEntityAddressModel
{
    public AddressModel Address { get; set; } = new();

    public AddressTypeModel AddressType { get; set; } = new() { Name = string.Empty };

}
