namespace AdventureWorks.Domain.Entities.AccountInfo;

public sealed class UserAccountEntity : BaseEntity
{
    public int BusinessEntityId { get; set; }

    public Guid RecordId { get; set; }

    public DateTime ModifiedDate { get; set; }

    public string UserName { get; set; }

    public string PasswordHash { get; set; }

    public BusinessEntity BusinessEntity { get; set; }

    public Person Person { get; set; }

}
