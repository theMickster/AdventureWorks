namespace AdventureWorks.Infrastructure.Persistence.Repositories.Purchasing;

/// <summary>
/// Maps the <c>Purchasing.PurchaseOrderHeader.Status</c> byte code to its human-readable label.
/// </summary>
internal static class PurchaseOrderStatusLabels
{
    /// <summary>
    /// Returns the human-readable label for a purchase order status code.
    /// </summary>
    /// <param name="status">1=Pending, 2=Approved, 3=Rejected, 4=Complete.</param>
    /// <returns>The matching label, or "Unknown" for any unrecognised code.</returns>
    public static string GetLabel(byte status) => status switch
    {
        1 => "Pending",
        2 => "Approved",
        3 => "Rejected",
        4 => "Complete",
        _ => "Unknown"
    };
}
