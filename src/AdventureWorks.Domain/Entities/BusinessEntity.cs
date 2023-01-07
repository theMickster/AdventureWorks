using System;
using System.Collections.Generic;

namespace AdventureWorks.Domain.Entities;

public class BusinessEntity : BaseEntity
{
    public int BusinessEntityId { get; set; }
    public Guid Rowguid { get; set; }
    public DateTime ModifiedDate { get; set; }

    public ICollection<Person> Persons { get; set; }
    public ICollection<Vendor> Vendors { get; set; }
    public ICollection<Store> Stores { get; set; }
    public ICollection<BusinessEntityAddress> BusinessEntityAddresses { get; set; }
    public ICollection<BusinessEntityContact> BusinessEntityContacts { get; set; }
}