namespace AdventureWorks.Models.Features.Sales;

/// <summary>
/// A single territory assignment record for a sales person.
/// </summary>
public sealed class SalesPersonTerritoryHistoryModel
{
    /// <summary>Gets or sets the territory identifier.</summary>
    /// <value>Sourced from <c>SalesTerritoryHistory.TerritoryId</c>.</value>
    public int TerritoryId { get; set; }

    /// <summary>Gets or sets the territory name.</summary>
    /// <value>Sourced from <c>SalesTerritory.Name</c>.</value>
    public string TerritoryName { get; set; } = string.Empty;

    /// <summary>Gets or sets the start date of the territory assignment.</summary>
    /// <value>Sourced from <c>SalesTerritoryHistory.StartDate</c>.</value>
    public DateTime StartDate { get; set; }

    /// <summary>Gets or sets the end date of the territory assignment; <see langword="null"/> if currently active.</summary>
    /// <value>Sourced from <c>SalesTerritoryHistory.EndDate</c>.</value>
    public DateTime? EndDate { get; set; }
}
