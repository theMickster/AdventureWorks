using AdventureWorks.Common.Constants;
using AdventureWorks.Common.Filtering.Base;

namespace AdventureWorks.Common.Filtering;

/// <summary>
/// Used to support paging and filtering in the AdventureWorks Sales Order list feature.
/// </summary>
public sealed class SalesOrderParameter : QueryStringParamsBase
{
    private const string SalesOrderIdField = "salesOrderId";
    private const string OrderDateField = "orderDate";
    private const string TotalDueField = "totalDue";
    private const string SalesOrderNumberField = "salesOrderNumber";
    private string _orderBy = SalesOrderIdField;

    /// <summary>
    /// The order by field for the sales order list results.
    /// Supported values: salesOrderId, orderDate, totalDue, salesOrderNumber.
    /// </summary>
    public string OrderBy
    {
        get
        {
            return _orderBy switch
            {
                OrderDateField => SortedResultConstants.OrderDate,
                TotalDueField => SortedResultConstants.TotalDue,
                SalesOrderNumberField => SortedResultConstants.SalesOrderNumber,
                _ => SortedResultConstants.SalesOrderId
            };
        }
        set
        {
            ArgumentNullException.ThrowIfNull(value);

            var normalized = value.Trim();
            _orderBy = normalized switch
            {
                OrderDateField => OrderDateField,
                TotalDueField => TotalDueField,
                SalesOrderNumberField => SalesOrderNumberField,
                _ => SalesOrderIdField
            };
        }
    }

    /// <summary>
    /// Filter by order date range start (inclusive).
    /// </summary>
    public DateTime? OrderDateFrom { get; set; }

    /// <summary>
    /// Filter by order date range end (inclusive).
    /// </summary>
    public DateTime? OrderDateTo { get; set; }

    /// <summary>
    /// Filter by sales order status (1=In process, 2=Approved, 3=Backordered, 4=Rejected, 5=Shipped, 6=Cancelled).
    /// </summary>
    public byte? Status { get; set; }

    /// <summary>
    /// Filter by sales person identifier.
    /// </summary>
    public int? SalesPersonId { get; set; }

    /// <summary>
    /// Filter by territory identifier.
    /// </summary>
    public int? TerritoryId { get; set; }
}
