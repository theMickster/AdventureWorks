using AdventureWorks.Domain.Entities.Person;

namespace AdventureWorks.Domain.Entities;

public class BusinessEntityContact : BaseEntity
{
    public int BusinessEntityId { get; set; }

    public int PersonId { get; set; }

    public int ContactTypeId { get; set; }

    public Guid Rowguid { get; set; }

    public DateTime ModifiedDate { get; set; }

    public BusinessEntity BusinessEntity { get; set; }

    public ContactTypeEntity ContactTypeEntity { get; set; }

    public PersonEntity Person { get; set; }
}