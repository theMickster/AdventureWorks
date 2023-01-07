using System;
using System.Collections.Generic;

namespace AdventureWorks.Domain.Entities;

public class Store : BaseEntity
{

    public int BusinessEntityId { get; set; }
    public string Name { get; set; }
    public int? SalesPersonId { get; set; }
    public string Demographics { get; set; }
    public Guid Rowguid { get; set; }
    public DateTime ModifiedDate { get; set; }

    public ICollection<Customer> Customers { get; set; }
    public BusinessEntity BusinessEntity { get; set; }
    public SalesPerson SalesPerson { get; set; }
}