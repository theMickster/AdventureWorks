namespace AdventureWorks.Models.Features.HumanResources;

public sealed class EmployeeOrgTreeItemModel
{
    public int EmployeeId { get; set; }
    public required string FullName { get; set; }
    public required string JobTitle { get; set; }
    public required string DepartmentName { get; set; }
    public short? OrganizationLevel { get; set; }
    public int? ParentEmployeeId { get; set; }
}
