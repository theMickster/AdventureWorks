namespace AdventureWorks.Domain.Entities.HumanResources;

public class EmployeePayHistory : BaseEntity
{
    public int BusinessEntityId { get; set; }
    
    public DateTime RateChangeDate { get; set; }
    
    public decimal Rate { get; set; }
    
    public byte PayFrequency { get; set; }
    
    public DateTime ModifiedDate { get; set; }

    public virtual EmployeeEntity BusinessEntity { get; set; }
}