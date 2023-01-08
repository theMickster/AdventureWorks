namespace AdventureWorks.Domain.Entities;

public class JobCandidate : BaseEntity
{
    public int JobCandidateId { get; set; }
    public int? BusinessEntityId { get; set; }
    public string Resume { get; set; }
    public DateTime ModifiedDate { get; set; }

    public virtual Employee BusinessEntity { get; set; }
}