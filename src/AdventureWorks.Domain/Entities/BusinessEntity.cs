using System;
using System.Collections.Generic;
using AdventureWorks.Core.Interfaces;

namespace AdventureWorks.Core.Entities
{
    public class BusinessEntity : BaseEntity, IAggregateRoot
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
}
