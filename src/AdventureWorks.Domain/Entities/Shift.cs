﻿namespace AdventureWorks.Domain.Entities;

public class Shift : BaseEntity
{

    public byte ShiftId { get; set; }
    public string Name { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public DateTime ModifiedDate { get; set; }

    public ICollection<EmployeeDepartmentHistory> EmployeeDepartmentHistory { get; set; }
}