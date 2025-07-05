namespace AdventureWorks.Models.Features.HumanResources;

public sealed class ShiftModel
{
    public byte Id { get; set; }

    public required string Name { get; set; }

    public TimeSpan StartTime { get; set; }

    public TimeSpan EndTime { get; set; }

    public DateTime ModifiedDate { get; set; }
}
