using AdventureWorks.Domain.Entities.Sales;

namespace AdventureWorks.Domain.Entities.Purchasing;

public class ShipMethod : BaseEntity
{

    public int ShipMethodId { get; set; }
    public string Name { get; set; }
    public decimal ShipBase { get; set; }
    public decimal ShipRate { get; set; }
    public Guid Rowguid { get; set; }
    public DateTime ModifiedDate { get; set; }

    public ICollection<PurchaseOrderHeader> PurchaseOrderHeaders { get; set; }
    public ICollection<SalesOrderHeader> SalesOrderHeaders { get; set; }
}