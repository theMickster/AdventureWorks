using AdventureWorks.Domain.Entities.Person;

namespace AdventureWorks.Domain.Entities;

public class PersonPhone : BaseEntity
{
    public int BusinessEntityId { get; set; }

    public string PhoneNumber { get; set; }

    public int PhoneNumberTypeId { get; set; }

    public DateTime ModifiedDate { get; set; }

    public virtual PersonEntity BusinessEntity { get; set; }

    public virtual PhoneNumberTypeEntity PhoneNumberType { get; set; }
}