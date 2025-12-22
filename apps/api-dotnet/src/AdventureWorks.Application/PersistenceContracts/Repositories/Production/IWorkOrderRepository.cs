using AdventureWorks.Common.Filtering;
using AdventureWorks.Domain.Entities.Production;

namespace AdventureWorks.Application.PersistenceContracts.Repositories.Production;

/// <summary>
/// Repository interface for work order read operations.
/// </summary>
public interface IWorkOrderRepository
{
    /// <summary>
    /// Retrieves a paginated list of work orders and the total count of work orders in the database.
    /// </summary>
    /// <param name="parameters">the input paging and sorting parameters</param>
    /// <param name="cancellationToken">token to cancel the operation</param>
    /// <returns>A tuple containing the list of work orders and the total count</returns>
    Task<(IReadOnlyList<WorkOrder>, int)> GetWorkOrdersAsync(
        WorkOrderParameter parameters,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a paged list of work orders filtered using the <paramref name="searchModel"/> input parameter.
    /// </summary>
    /// <param name="parameters">the input paging and sorting parameters</param>
    /// <param name="searchModel">the search filter criteria</param>
    /// <param name="cancellationToken">token to cancel the operation</param>
    /// <returns>A tuple containing the filtered list of work orders and the total count</returns>
    Task<(IReadOnlyList<WorkOrder>, int)> SearchWorkOrdersAsync(
        WorkOrderParameter parameters,
        WorkOrderSearchModel searchModel,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a single work order by its identifier, including product and scrap reason details.
    /// </summary>
    /// <param name="workOrderId">the unique work order identifier</param>
    /// <param name="cancellationToken">token to cancel the operation</param>
    /// <returns>The matching work order, or null when no work order exists with the given identifier</returns>
    Task<WorkOrder?> GetByIdAsync(int workOrderId, CancellationToken cancellationToken = default);
}
