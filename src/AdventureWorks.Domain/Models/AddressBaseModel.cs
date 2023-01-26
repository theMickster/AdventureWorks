using AdventureWorks.Domain.Models.Slim;

namespace AdventureWorks.Domain.Models;

public abstract class AddressBaseModel
{
    public string AddressLine1 { get; set; }

    public string AddressLine2 { get; set; }

    public string City { get; set; }

    public GenericSlimModel AddressStateProvince { get; set; }

    public string PostalCode { get; set; }

}