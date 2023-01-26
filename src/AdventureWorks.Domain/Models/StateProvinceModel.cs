using AdventureWorks.Domain.Models.Slim;

namespace AdventureWorks.Domain.Models;

public sealed class StateProvinceModel
{
    public int Id { get; set; }

    public string Code { get; set; }

    public string Name { get; set; }

    public bool IsStateProvinceCodeUnavailable { get; set; }
    
    public CountryRegionModel CountryRegion { get; set; }

    public GenericSlimModel Territory { get; set; }
}
