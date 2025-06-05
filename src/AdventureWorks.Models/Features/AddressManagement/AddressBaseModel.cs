using AdventureWorks.Models.Slim;

namespace AdventureWorks.Models.Features.AddressManagement;

public abstract class AddressBaseModel
{
    public string? AddressLine1 { get; set; }

    public string? AddressLine2 { get; set; }

    public string? City { get; set; }

    public GenericSlimModel? StateProvince { get; set; }

    public string? PostalCode { get; set; }

}