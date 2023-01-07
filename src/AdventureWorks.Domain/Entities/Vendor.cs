﻿namespace AdventureWorks.Domain.Entities;

public class Vendor : BaseEntity
{

    public int BusinessEntityId { get; set; }
    public string AccountNumber { get; set; }
    public string Name { get; set; }
    public byte CreditRating { get; set; }
    public bool PreferredVendorStatus { get; set; }
    public bool ActiveFlag { get; set; }
    public string PurchasingWebServiceUrl { get; set; }
    public DateTime ModifiedDate { get; set; }

    public ICollection<ProductVendor> ProductVendors { get; set; }
    public ICollection<PurchaseOrderHeader> PurchaseOrderHeaders { get; set; }
    public BusinessEntity BusinessEntity { get; set; }
}