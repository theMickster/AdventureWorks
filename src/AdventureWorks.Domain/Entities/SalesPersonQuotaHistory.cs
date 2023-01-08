namespace AdventureWorks.Domain.Entities;

public class SalesPersonQuotaHistory : BaseEntity
{
    public int BusinessEntityId { get; set; }
    public DateTime QuotaDate { get; set; }
    public decimal SalesQuota { get; set; }
    public Guid Rowguid { get; set; }
    public DateTime ModifiedDate { get; set; }

    public virtual SalesPerson BusinessEntity { get; set; }
}