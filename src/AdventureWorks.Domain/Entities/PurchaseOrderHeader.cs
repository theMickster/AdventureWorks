﻿using System;
using System.Collections.Generic;
using AdventureWorks.Core.Interfaces;

namespace AdventureWorks.Core.Entities
{
    public class PurchaseOrderHeader : BaseEntity, IAggregateRoot
    {

        public int PurchaseOrderId { get; set; }
        public byte RevisionNumber { get; set; }
        public byte Status { get; set; }
        public int EmployeeId { get; set; }
        public int VendorId { get; set; }
        public int ShipMethodId { get; set; }
        public DateTime OrderDate { get; set; }
        public DateTime? ShipDate { get; set; }
        public decimal SubTotal { get; set; }
        public decimal TaxAmt { get; set; }
        public decimal Freight { get; set; }
        public decimal TotalDue { get; set; }
        public DateTime ModifiedDate { get; set; }

        public ICollection<PurchaseOrderDetail> PurchaseOrderDetails { get; set; }
        public Employee Employee { get; set; }
        public ShipMethod ShipMethod { get; set; }
        public Vendor Vendor { get; set; }
    }
}
