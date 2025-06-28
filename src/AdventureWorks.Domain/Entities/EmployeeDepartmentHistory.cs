namespace AdventureWorks.Domain.Entities;

public class EmployeeDepartmentHistory : BaseEntity
{
    public int BusinessEntityId { get; set; }
    public short DepartmentId { get; set; }
    public byte ShiftId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public DateTime ModifiedDate { get; set; }

    public EmployeeEntity BusinessEntity { get; set; }
    public DepartmentEntity Department { get; set; }
    public Shift Shift { get; set; }
}