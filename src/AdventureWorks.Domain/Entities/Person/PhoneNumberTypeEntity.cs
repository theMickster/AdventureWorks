namespace AdventureWorks.Domain.Entities.Person;

public sealed class PhoneNumberTypeEntity : BaseEntity
{
    public int PhoneNumberTypeId { get; set; }

    public string Name { get; set; }

    public DateTime ModifiedDate { get; set; }

    public ICollection<PersonPhone> PersonPhones { get; set; }
}