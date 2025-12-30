namespace AdventureWorks.Models.Features.Purchasing;

/// <summary>
/// Aggregate purchasing analytics (<c>GET /api/v1/purchasing/analytics</c>): Pareto vendor-spend
/// data plus a purchase order pipeline summary.
/// </summary>
public sealed class PurchasingAnalyticsModel
{
    /// <summary>
    /// Every vendor, ordered by total spend descending (ties broken by <c>VendorId</c> ascending),
    /// with a running cumulative percentage of total spend. Vendors with no purchase orders are
    /// included with a total spend of zero.
    /// </summary>
    public List<VendorSpendModel> ParetoData { get; set; } = [];

    /// <summary>
    /// Purchase order counts and value by status, in status-code order (Pending, Approved,
    /// Rejected, Complete). All four statuses are always present, even when a status has no
    /// purchase orders.
    /// </summary>
    public List<PurchaseOrderPipelineStatusModel> PipelineSummary { get; set; } = [];
}
