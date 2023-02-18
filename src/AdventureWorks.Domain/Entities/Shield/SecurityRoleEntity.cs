namespace AdventureWorks.Domain.Entities.Shield;

public class SecurityRoleEntity : BaseEntity
{
    public int Id { get; set; }

    public Guid RecordId { get; set; }

    public string Name { get; set; }

    public string Description { get; set; }

    public int CreatedBy { get; set; }

    public DateTime CreatedOn { get; set; }

    public int ModifiedBy { get; set; }

    public DateTime ModifiedOn { get; set; }

    public ICollection<SecurityGroupSecurityRoleEntity> SecurityGroupSecurityRoleEntities { get; set; }

}
