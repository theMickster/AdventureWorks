namespace AdventureWorks.Domain.Entities.Person;

public sealed class PersonTypeEntity : BaseEntity
{
    public int PersonTypeId { get; set; }

    public Guid PersonTypeGuid { get; set; }

    public string PersonTypeCode { get; set; }

    public string PersonTypeName { get; set; }

    public string PersonTypeDescription { get; set; }

    public int CreatedBy { get; set; }

    public DateTime CreatedOn { get; set; }

    public int ModifiedBy { get; set; }

    public DateTime ModifiedOn { get; set; }
}
