namespace AdventureWorks.Domain.Entities.Person;

public sealed class BusinessEntityContactEntity : BaseEntity
{
    public int BusinessEntityId { get; set; }

    public int PersonId { get; set; }

    public int ContactTypeId { get; set; }

    public Guid Rowguid { get; set; }

    public DateTime ModifiedDate { get; set; }

    public BusinessEntity BusinessEntity { get; set; }

    public ContactTypeEntity ContactType { get; set; }

    public PersonEntity Person { get; set; }
}