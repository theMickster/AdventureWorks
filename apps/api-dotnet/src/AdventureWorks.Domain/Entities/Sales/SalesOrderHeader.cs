using AdventureWorks.Domain.Entities.Person;
using AdventureWorks.Domain.Entities.Purchasing;

namespace AdventureWorks.Domain.Entities.Sales;

public class SalesOrderHeader : BaseEntity
{
    public int SalesOrderId { get; set; }
    public byte RevisionNumber { get; set; }
    public DateTime OrderDate { get; set; }
    public DateTime DueDate { get; set; }
    public DateTime? ShipDate { get; set; }
    public byte Status { get; set; }
    public bool OnlineOrderFlag { get; set; }
    public string SalesOrderNumber { get; set; }
    public string PurchaseOrderNumber { get; set; }
    public string AccountNumber { get; set; }
    public int CustomerId { get; set; }
    public int? SalesPersonId { get; set; }
    public int? TerritoryId { get; set; }
    public int BillToAddressId { get; set; }
    public int ShipToAddressId { get; set; }
    public int ShipMethodId { get; set; }
    public int? CreditCardId { get; set; }
    public string CreditCardApprovalCode { get; set; }
    public int? CurrencyRateId { get; set; }
    public decimal SubTotal { get; set; }
    public decimal TaxAmt { get; set; }
    public decimal Freight { get; set; }
    public decimal TotalDue { get; set; }
    public string Comment { get; set; }
    public Guid Rowguid { get; set; }
    public DateTime ModifiedDate { get; set; }

    public ICollection<SalesOrderDetail> SalesOrderDetails { get; set; }
    public ICollection<SalesOrderHeaderSalesReason> SalesOrderHeaderSalesReasons { get; set; }

    public AddressEntity BillToAddressEntity { get; set; }
    public CreditCard CreditCard { get; set; }
    public CurrencyRate CurrencyRate { get; set; }
    public CustomerEntity CustomerEntity { get; set; }
    public SalesPersonEntity SalesPerson { get; set; }
    public ShipMethod ShipMethod { get; set; }
    public AddressEntity ShipToAddressEntity { get; set; }
    public SalesTerritoryEntity TerritoryEntity { get; set; }
}