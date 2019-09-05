using System;
using System.Collections.Generic;
using AdventureWorks.Core.Interfaces;

namespace AdventureWorks.Core.Entities
{
    public class Culture : BaseEntity, IAggregateRoot
    {
        public string CultureId { get; set; }
        public string Name { get; set; }
        public DateTime ModifiedDate { get; set; }

        public virtual ICollection<ProductModelProductDescriptionCulture> ProductModelProductDescriptionCulture { get; set; }
    }
}
