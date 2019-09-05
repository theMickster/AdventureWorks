using System;
using System.Collections.Generic;
using AdventureWorks.Core.Interfaces;

namespace AdventureWorks.Core.Entities
{    
    public class PhoneNumberType : BaseEntity, IAggregateRoot
    {
        public int PhoneNumberTypeId { get; set; }
        public string Name { get; set; }
        public DateTime ModifiedDate { get; set; }

        public ICollection<PersonPhone> PersonPhones { get; set; }
    }
}
