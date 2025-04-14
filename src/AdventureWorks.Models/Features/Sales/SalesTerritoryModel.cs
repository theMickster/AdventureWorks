using AdventureWorks.Models.Features.AddressManagement;

namespace AdventureWorks.Models.Features.Sales;

public sealed class SalesTerritoryModel
{
    public int Id { get; set; }

    public string Name { get; set; }

    public string Group { get; set; }

    public decimal SalesYtd { get; set; }

    public decimal SalesLastYear { get; set; }

    public decimal CostYtd { get; set; }

    public decimal CostLastYear { get; set; }

    public CountryRegionModel CountryRegion { get; set; }
}
