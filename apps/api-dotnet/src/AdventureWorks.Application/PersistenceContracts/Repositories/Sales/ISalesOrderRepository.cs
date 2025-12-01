using AdventureWorks.Common.Filtering;
using AdventureWorks.Domain.Entities.Sales;
using AdventureWorks.Models.Features.Sales;

namespace AdventureWorks.Application.PersistenceContracts.Repositories.Sales;

/// <summary>
/// Repository interface for sales order persistence operations.
/// </summary>
public interface ISalesOrderRepository : IAsyncRepository<SalesOrderHeader>
{
    /// <summary>
    /// Retrieves a paginated list of sales orders and the total count of sales orders in the database.
    /// </summary>
    /// <param name="parameters">the input paging and filtering parameters</param>
    /// <param name="cancellationToken">token to cancel the operation</param>
    /// <returns>A tuple containing the list of sales orders and the total count</returns>
    Task<(IReadOnlyList<SalesOrderHeader>, int)> GetSalesOrdersAsync(
        SalesOrderParameter parameters,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a paged list of sales orders filtered using the <paramref name="searchModel"/> input parameter.
    /// </summary>
    /// <param name="parameters">the input paging parameters</param>
    /// <param name="searchModel">the search filter criteria</param>
    /// <param name="cancellationToken">token to cancel the operation</param>
    /// <returns>A tuple containing the filtered list of sales orders and the total count</returns>
    Task<(IReadOnlyList<SalesOrderHeader>, int)> SearchSalesOrdersAsync(
        SalesOrderParameter parameters,
        SalesOrderSearchModel searchModel,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves the full detail of a single sales order by its identifier, including line items,
    /// addresses, sales person, and territory. Returns null when the order does not exist.
    /// </summary>
    /// <param name="salesOrderId">the sales order primary key</param>
    /// <param name="cancellationToken">token to cancel the operation</param>
    /// <returns>The matching <see cref="SalesOrderHeader"/>, or null if not found</returns>
    Task<SalesOrderHeader?> GetSalesOrderDetailAsync(
        int salesOrderId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns aggregated analytics — total revenue, order count, percentage of the unfiltered total,
    /// and a monthly revenue trend — for the slice of orders matching the optional filter.
    /// Monthly trend is capped at 24 entries ordered ascending; date ranges spanning more than 24
    /// months will have the oldest months silently dropped. Each trend entry includes
    /// <see cref="AdventureWorks.Models.Features.Sales.SalesOrderMonthlyTrendModel.IsPartialMonth"/>,
    /// which is true when the latest order date in the filtered dataset falls before the last calendar
    /// day of that month. Returns an empty <c>MonthlyTrend</c> immediately when no orders match.
    /// </summary>
    /// <param name="filter">Optional filter criteria. Null means no filter (full dataset).</param>
    /// <param name="cancellationToken">token to cancel the operation</param>
    /// <returns>
    /// Aggregated <see cref="AdventureWorks.Models.Features.Sales.SalesOrderAnalyticsModel"/> for
    /// the filtered slice. <c>MonthlyTrend</c> is empty when no orders match; all numeric totals
    /// are zero in that case.
    /// </returns>
    Task<SalesOrderAnalyticsModel> GetSalesOrderAnalyticsAsync(
        SalesOrderSearchModel? filter,
        CancellationToken cancellationToken = default);
}
