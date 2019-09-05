using System;
using System.Collections.Generic;
using AdventureWorks.Core.Interfaces;

namespace AdventureWorks.Core.Entities
{
    public class SpecialOfferProduct : BaseEntity, IAggregateRoot
    {
        public int SpecialOfferId { get; set; }
        public int ProductId { get; set; }
        public Guid Rowguid { get; set; }
        public DateTime ModifiedDate { get; set; }

        public ICollection<SalesOrderDetail> SalesOrderDetail { get; set; }
        public Product Product { get; set; }
        public SpecialOffer SpecialOffer { get; set; }
    }
}
