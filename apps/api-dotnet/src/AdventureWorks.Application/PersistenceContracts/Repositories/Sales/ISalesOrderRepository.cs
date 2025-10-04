using AdventureWorks.Common.Filtering;
using AdventureWorks.Domain.Entities.Sales;

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
}
