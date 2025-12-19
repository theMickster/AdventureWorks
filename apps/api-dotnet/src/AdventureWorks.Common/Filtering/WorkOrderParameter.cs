using AdventureWorks.Common.Constants;
using AdventureWorks.Common.Filtering.Base;

namespace AdventureWorks.Common.Filtering;

/// <summary>
/// Used to support paging and filtering in the AdventureWorks Work Order list feature.
/// </summary>
/// <remarks>
/// Defaults to sorting by StartDate descending (most recent work orders first) and a page size
/// of 25, overriding the base class's ascending/10 defaults.
/// </remarks>
public sealed class WorkOrderParameter : QueryStringParamsBase
{
    private const string WorkOrderIdField = "workorderid";
    private const string StartDateField = "startdate";
    private const string DueDateField = "duedate";
    private const int DefaultPageSize = 25;

    private string _orderBy = StartDateField;
    private string _sortOrder = SortedResultConstants.Descending;
    private int _pageSize = DefaultPageSize;

    /// <summary>
    /// The order by field for the work order list results.
    /// Supported values: workOrderId, startDate, dueDate. Defaults to startDate.
    /// </summary>
    public string OrderBy
    {
        get
        {
            return _orderBy switch
            {
                WorkOrderIdField => SortedResultConstants.WorkOrderId,
                DueDateField => SortedResultConstants.DueDate,
                _ => SortedResultConstants.StartDate
            };
        }
        set
        {
            ArgumentNullException.ThrowIfNull(value);

            var normalized = value.Trim().ToLower();
            _orderBy = normalized switch
            {
                WorkOrderIdField => WorkOrderIdField,
                DueDateField => DueDateField,
                _ => StartDateField
            };
        }
    }

    /// <summary>
    /// The direction in which to sort the list of results.
    /// Defaults to <see cref="SortedResultConstants.Descending"/> for the work order list,
    /// overriding the base ascending default so the most recent work orders surface first.
    /// </summary>
    public override string SortOrder
    {
        get => _sortOrder;
        init => _sortOrder = value == null ? SortedResultConstants.Descending : value.Trim().ToLower()
            switch
            {
                "asc" or "ascending" => SortedResultConstants.Ascending,
                "desc" or "descending" => SortedResultConstants.Descending,
                _ => SortedResultConstants.Descending
            };
    }

    /// <summary>
    /// The amount of records requested to be returned to a list endpoint's caller.
    /// Defaults to 25 for the work order list, overriding the base class's default of 10.
    /// </summary>
    public override int PageSize
    {
        get => _pageSize;
        init => _pageSize = value <= 0 ? 1 : (value > MaxTake ? MaxTake : value);
    }

    /// <summary>
    /// Filter by product identifier.
    /// </summary>
    public int? ProductId { get; set; }

    /// <summary>
    /// Filter by work order start date range start (inclusive).
    /// </summary>
    public DateTime? StartDate { get; set; }

    /// <summary>
    /// Filter by work order start date range end (inclusive).
    /// </summary>
    public DateTime? EndDate { get; set; }

    /// <summary>
    /// Filter to work orders that have scrapped units (true) or none (false).
    /// </summary>
    public bool? HasScrapped { get; set; }

    /// <summary>
    /// Filter by scrap reason identifier.
    /// </summary>
    public short? ScrapReasonId { get; set; }
}
