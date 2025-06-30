using AdventureWorks.Domain.Entities.HumanResources;
using AdventureWorks.Domain.Entities.Sales;
using AdventureWorks.Domain.Entities.Shield;

namespace AdventureWorks.Domain.Entities.Person;

public sealed class PersonEntity : BaseEntity
{
    public int BusinessEntityId { get; set; }

    public int PersonTypeId { get; set; }

    public bool NameStyle { get; set; }

    public string Title { get; set; }

    public string FirstName { get; set; }

    public string MiddleName { get; set; }

    public string LastName { get; set; }

    public string Suffix { get; set; }

    public int EmailPromotion { get; set; }

    public string AdditionalContactInfo { get; set; }

    public string Demographics { get; set; }

    public Guid Rowguid { get; set; }

    public DateTime ModifiedDate { get; set; }

    public ICollection<BusinessEntityContactEntity> BusinessEntityContacts { get; set; }

    public ICollection<CustomerEntity> Customers { get; set; }

    public ICollection<EmailAddressEntity> EmailAddresses { get; set; }

    public ICollection<PersonCreditCard> PersonCreditCards { get; set; }

    public ICollection<PersonPhone> PersonPhones { get; set; }

    public ICollection<EmployeeEntity> Employees { get; set; }

    public BusinessEntity BusinessEntity { get; set; }

    public UserAccountEntity UserAccount { get; set; }

    public PersonTypeEntity PersonType { get; set; }
}