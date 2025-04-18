using AdventureWorks.Models.Slim;

namespace AdventureWorks.Models.Features.AddressManagement;

public sealed class StateProvinceModel
{
    public int Id { get; set; }

    public string Code { get; set; }

    public string Name { get; set; }

    public bool IsStateProvinceCodeUnavailable { get; set; }
    
    public CountryRegionModel CountryRegion { get; set; }

    public GenericSlimModel Territory { get; set; }
}
