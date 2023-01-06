using System;
using System.Collections.Generic;

namespace AdventureWorks.Domain.Entities
{
    public class Employee : BaseEntity
    {
        public int BusinessEntityId { get; set; }
        public string NationalIdnumber { get; set; }
        public string LoginId { get; set; }
        public short? OrganizationLevel { get; set; }
        public string JobTitle { get; set; }
        public DateTime BirthDate { get; set; }
        public string MaritalStatus { get; set; }
        public string Gender { get; set; }
        public DateTime HireDate { get; set; }
        public bool SalariedFlag { get; set; }
        public short VacationHours { get; set; }
        public short SickLeaveHours { get; set; }
        public bool CurrentFlag { get; set; }
        public Guid Rowguid { get; set; }
        public DateTime ModifiedDate { get; set; }

        public ICollection<EmployeeDepartmentHistory> EmployeeDepartmentHistory { get; set; }
        public ICollection<EmployeePayHistory> EmployeePayHistory { get; set; }
        public ICollection<JobCandidate> JobCandidates { get; set; }
        public ICollection<PurchaseOrderHeader> PurchaseOrderHeaders { get; set; }
        public ICollection<SalesPerson> SalesPersons { get; set; }
        
        public Person PersonBusinessEntity { get; set; }
    }
}
