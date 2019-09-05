using System;
using AdventureWorks.Core.Interfaces;

namespace AdventureWorks.Core.Entities
{
    public class PersonPhone : BaseEntity, IAggregateRoot
    {
        public int BusinessEntityId { get; set; }
        public string PhoneNumber { get; set; }
        public int PhoneNumberTypeId { get; set; }
        public DateTime ModifiedDate { get; set; }

        public virtual Person BusinessEntity { get; set; }
        public virtual PhoneNumberType PhoneNumberType { get; set; }
    }
}
