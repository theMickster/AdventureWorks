using System;
using System.Collections.Generic;
using AdventureWorks.Core.Interfaces;

namespace AdventureWorks.Core.Entities
{
    public class ContactType : BaseEntity, IAggregateRoot
    {

        public int ContactTypeId { get; set; }
        public string Name { get; set; }
        public DateTime ModifiedDate { get; set; }

        public ICollection<BusinessEntityContact> BusinessEntityContacts { get; set; }
    }
}
