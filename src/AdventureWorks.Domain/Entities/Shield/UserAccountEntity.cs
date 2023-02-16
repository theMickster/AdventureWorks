namespace AdventureWorks.Domain.Entities.Shield;

public sealed class UserAccountEntity : BaseEntity
{
    public int BusinessEntityId { get; set; }

    public Guid RecordId { get; set; }

    public DateTime ModifiedDate { get; set; }

    public string UserName { get; set; }

    public string PasswordHash { get; set; }

    public int PrimaryEmailAddressId { get; set; }

    public BusinessEntity BusinessEntity { get; set; }

    public Person Person { get; set; }

    public EmailAddress EmailAddress { get; set; }

}
