namespace AdventureWorks.Models.Features.HumanResources;

/// <summary>Represents a request to transfer an employee to a new department and/or shift.</summary>
public sealed class EmployeeTransferModel
{
    /// <summary>The target department identifier.</summary>
    public short DepartmentId { get; set; }

    /// <summary>The target shift identifier (1=Day, 2=Evening, 3=Night).</summary>
    public byte ShiftId { get; set; }
}
