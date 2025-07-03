namespace AdventureWorks.Models.Base;

public abstract class LookupTypeBaseModel
{
    public int Id { get; set; }

    public required string Name { get; set; }

    public required string Code { get; set; }

    public required string Description { get; set; }
}
