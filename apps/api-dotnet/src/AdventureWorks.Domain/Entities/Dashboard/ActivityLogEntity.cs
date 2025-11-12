namespace AdventureWorks.Domain.Entities.Dashboard;

public class ActivityLogEntity : BaseEntity
{
    public int ActivityLogId { get; set; }
    public required string EntityType { get; set; }
    public int EntityId { get; set; }
    public required string Action { get; set; }
    public required string UserName { get; set; }
    public DateTime Timestamp { get; set; }
}
