namespace AdventureWorks.Models.Features.Production;

public sealed class LocationModel
{
    public short LocationId { get; set; }

    public string Name { get; set; } = string.Empty;

    public decimal CostRate { get; set; }

    public decimal Availability { get; set; }

    public DateTime ModifiedDate { get; set; }
}
