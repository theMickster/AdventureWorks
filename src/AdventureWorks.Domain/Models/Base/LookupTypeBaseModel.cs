namespace AdventureWorks.Domain.Models.Base;

public abstract class LookupTypeBaseModel
{
    public int Id { get; set; }

    public string Name { get; set; }

    public string Code { get; set; }

    public string Description { get; set; }
}
