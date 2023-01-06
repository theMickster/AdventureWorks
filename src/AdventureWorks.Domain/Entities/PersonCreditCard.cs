using System;

namespace AdventureWorks.Domain.Entities
{
    public class PersonCreditCard : BaseEntity
    {
        public int BusinessEntityId { get; set; }
        public int CreditCardId { get; set; }
        public DateTime ModifiedDate { get; set; }

        public virtual Person BusinessEntity { get; set; }

        public virtual CreditCard CreditCard { get; set; }

    }
}
