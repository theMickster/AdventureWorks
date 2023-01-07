﻿namespace AdventureWorks.Domain.Entities;

public class SalesOrderDetail : BaseEntity
{
    public int SalesOrderId { get; set; }
    public int SalesOrderDetailId { get; set; }
    public string CarrierTrackingNumber { get; set; }
    public short OrderQty { get; set; }
    public int ProductId { get; set; }
    public int SpecialOfferId { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal UnitPriceDiscount { get; set; }
    public decimal LineTotal { get; set; }
    public Guid Rowguid { get; set; }
    public DateTime ModifiedDate { get; set; }

    public SalesOrderHeader SalesOrder { get; set; }
    public SpecialOfferProduct SpecialOfferProduct { get; set; }
    public Product Product{ get; set; }
}