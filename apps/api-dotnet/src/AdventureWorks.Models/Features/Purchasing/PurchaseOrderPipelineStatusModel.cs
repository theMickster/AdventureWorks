namespace AdventureWorks.Models.Features.Purchasing;

/// <summary>
/// Purchase order counts and value rolled up by status, for the purchasing pipeline summary.
/// </summary>
public sealed class PurchaseOrderPipelineStatusModel
{
    /// <summary>
    /// The human-readable status label: Pending, Approved, Rejected, or Complete.
    /// </summary>
    public string StatusLabel { get; set; } = string.Empty;

    /// <summary>
    /// The number of purchase orders in this status. Zero when the status has no purchase orders —
    /// all four statuses are always present in the summary.
    /// </summary>
    public int PoCount { get; set; }

    /// <summary>
    /// The sum of <c>TotalDue</c> across the purchase orders in this status.
    /// </summary>
    public decimal TotalValue { get; set; }
}
