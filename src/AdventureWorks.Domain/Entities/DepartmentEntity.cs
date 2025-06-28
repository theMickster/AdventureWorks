namespace AdventureWorks.Domain.Entities;

public class DepartmentEntity : BaseEntity
{

    public short DepartmentId { get; set; }
    public string Name { get; set; }
    public string GroupName { get; set; }
    public DateTime ModifiedDate { get; set; }

    public ICollection<EmployeeDepartmentHistory> EmployeeDepartmentHistory { get; set; }
}