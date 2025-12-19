namespace AdventureWorks.Models.Features.Production;

/// <summary>
/// Represents a production work order in API responses.
/// </summary>
public sealed class WorkOrderModel
{
    /// <summary>
    /// The unique identifier for the work order.
    /// </summary>
    public int WorkOrderId { get; set; }

    /// <summary>
    /// The identifier of the product being manufactured.
    /// </summary>
    public int ProductId { get; set; }

    /// <summary>
    /// The name of the product being manufactured.
    /// </summary>
    public required string ProductName { get; set; }

    /// <summary>
    /// The quantity ordered.
    /// </summary>
    public int OrderedQty { get; set; }

    /// <summary>
    /// The quantity stocked (finished and put into inventory).
    /// </summary>
    public int StockedQty { get; set; }

    /// <summary>
    /// The quantity scrapped during manufacturing.
    /// </summary>
    public short ScrappedQty { get; set; }

    /// <summary>
    /// The percentage of ordered units that were successfully stocked (StockedQty / OrderedQty * 100).
    /// </summary>
    public decimal YieldRate { get; set; }

    /// <summary>
    /// The date manufacturing of the work order started.
    /// </summary>
    public DateTime StartDate { get; set; }

    /// <summary>
    /// The date manufacturing of the work order finished, or null if still in progress.
    /// </summary>
    public DateTime? EndDate { get; set; }

    /// <summary>
    /// The date the work order was due to be completed.
    /// </summary>
    public DateTime DueDate { get; set; }

    /// <summary>
    /// True when the work order finished after its due date.
    /// </summary>
    public bool IsCompletedLate { get; set; }
}
