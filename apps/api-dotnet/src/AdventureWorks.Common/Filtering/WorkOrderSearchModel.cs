namespace AdventureWorks.Common.Filtering;

/// <summary>
/// Search model for filtering work orders.
/// </summary>
public sealed class WorkOrderSearchModel
{
    /// <summary>
    /// The filter by product identifier.
    /// </summary>
    public int? ProductId { get; set; }

    /// <summary>
    /// The filter by work order start date range start (inclusive).
    /// </summary>
    public DateTime? StartDate { get; set; }

    /// <summary>
    /// The filter by work order start date range end (inclusive).
    /// </summary>
    public DateTime? EndDate { get; set; }

    /// <summary>
    /// The filter to work orders that have scrapped units (true) or none (false).
    /// </summary>
    public bool? HasScrapped { get; set; }

    /// <summary>
    /// The scrap reason identifier to filter by.
    /// </summary>
    public short? ScrapReasonId { get; set; }
}
