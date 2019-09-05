using System;
using AdventureWorks.Core.Interfaces;

namespace AdventureWorks.Core.Entities
{
    public class EmailAddress : BaseEntity, IAggregateRoot
    {
        public int BusinessEntityId { get; set; }
        public int EmailAddressId { get; set; }
        public string EmailAddressName { get; set; }
        public Guid Rowguid { get; set; }
        public DateTime ModifiedDate { get; set; }

        public virtual Person BusinessEntity { get; set; }
    }
}
