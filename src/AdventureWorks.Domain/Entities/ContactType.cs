using System;
using System.Collections.Generic;

namespace AdventureWorks.Domain.Entities
{
    public class ContactType : BaseEntity
    {

        public int ContactTypeId { get; set; }
        public string Name { get; set; }
        public DateTime ModifiedDate { get; set; }

        public ICollection<BusinessEntityContact> BusinessEntityContacts { get; set; }
    }
}
