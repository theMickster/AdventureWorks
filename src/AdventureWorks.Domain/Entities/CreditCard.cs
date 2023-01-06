using System;
using System.Collections.Generic;

namespace AdventureWorks.Domain.Entities
{
    public class CreditCard : BaseEntity
    {
        public int CreditCardId { get; set; }
        public string CardType { get; set; }
        public string CardNumber { get; set; }
        public byte ExpMonth { get; set; }
        public short ExpYear { get; set; }
        public DateTime ModifiedDate { get; set; }

        public ICollection<PersonCreditCard> PersonCreditCards { get; set; }
        public ICollection<SalesOrderHeader> SalesOrderHeaders { get; set; }
    }
}
