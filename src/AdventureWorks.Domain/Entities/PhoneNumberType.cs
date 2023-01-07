using System;
using System.Collections.Generic;

namespace AdventureWorks.Domain.Entities;

public class PhoneNumberType : BaseEntity
{
    public int PhoneNumberTypeId { get; set; }
    public string Name { get; set; }
    public DateTime ModifiedDate { get; set; }

    public ICollection<PersonPhone> PersonPhones { get; set; }
}