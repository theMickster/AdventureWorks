namespace AdventureWorks.Domain.Entities.HumanResources;

public sealed class ShiftEntity : BaseEntity
{
    public byte ShiftId { get; set; }

    public string Name { get; set; }

    public TimeSpan StartTime { get; set; }

    public TimeSpan EndTime { get; set; }

    public DateTime ModifiedDate { get; set; }

    public ICollection<EmployeeDepartmentHistory> EmployeeDepartmentHistory { get; set; }
}