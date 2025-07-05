namespace AdventureWorks.Models.Features.HumanResources;

public sealed class DepartmentModel
{
    public short Id { get; set; }

    public required string Name { get; set; }

    public required string GroupName { get; set; }

    public DateTime ModifiedDate { get; set; }
}
