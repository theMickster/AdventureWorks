using System;
using System.Collections.Generic;
using AdventureWorks.Core.Interfaces;

namespace AdventureWorks.Core.Entities
{
    public class AddressType : BaseEntity, IAggregateRoot
    {
        public int AddressTypeId { get; set; }
        public string Name { get; set; }
        public Guid Rowguid { get; set; }
        public DateTime ModifiedDate { get; set; }

        public virtual ICollection<BusinessEntityAddress> BusinessEntityAddress { get; set; }
    }
}
