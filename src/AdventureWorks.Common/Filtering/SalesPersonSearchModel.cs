using AdventureWorks.Common.Filtering.Base;

namespace AdventureWorks.Common.Filtering;

public sealed class SalesPersonSearchModel : SearchPersonModelBase
{
    /// <summary>
    /// The unique integer identifier of the sales territory
    /// </summary>
    public int? SalesTerritoryId { get; set; }

    /// <summary>
    /// The name of the sales territory
    /// </summary>
    public string? SalesTerritoryName { get; set; }

    /// <summary>
    /// The name of the sales territory group or region
    /// </summary>
    public string? SalesTerritoryGroupName { get; set; }

    /// <summary>
    /// The sales person's email address
    /// </summary>
    public string? EmailAddress { get; set; }
    
}
