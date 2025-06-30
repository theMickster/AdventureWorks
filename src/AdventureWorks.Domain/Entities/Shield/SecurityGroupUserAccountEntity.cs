using AdventureWorks.Domain.Entities.Person;

namespace AdventureWorks.Domain.Entities.Shield;

public sealed class SecurityGroupUserAccountEntity : BaseEntity
{
    public int Id { get; set; }

    public Guid RecordId { get; set; }

    public int GroupId { get; set; }

    public int BusinessEntityId { get; set; }

    public int CreatedBy { get; set; }

    public DateTime CreatedOn { get; set; }

    public int ModifiedBy { get; set; }

    public DateTime ModifiedOn { get; set; }

    public SecurityGroupEntity SecurityGroup { get; set; }

    public UserAccountEntity UserAccount { get; set; }

    public BusinessEntity BusinessEntity { get; set; }

}
