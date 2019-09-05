using System;
using System.Collections.Generic;
using AdventureWorks.Core.Interfaces;

namespace AdventureWorks.Core.Entities
{
    public class Department : BaseEntity, IAggregateRoot
    {

        public short DepartmentId { get; set; }
        public string Name { get; set; }
        public string GroupName { get; set; }
        public DateTime ModifiedDate { get; set; }

        public ICollection<EmployeeDepartmentHistory> EmployeeDepartmentHistory { get; set; }
    }
}
