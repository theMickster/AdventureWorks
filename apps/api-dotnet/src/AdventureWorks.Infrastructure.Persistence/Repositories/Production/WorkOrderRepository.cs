using AdventureWorks.Application.PersistenceContracts.Repositories.Production;
using AdventureWorks.Common.Attributes;
using AdventureWorks.Common.Constants;
using AdventureWorks.Common.Filtering;
using AdventureWorks.Domain.Entities.Production;
using AdventureWorks.Infrastructure.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace AdventureWorks.Infrastructure.Persistence.Repositories.Production;

/// <summary>
/// Repository for work order read operations.
/// </summary>
[ServiceLifetimeScoped]
public sealed class WorkOrderRepository(AdventureWorksDbContext dbContext) : IWorkOrderRepository
{
    private readonly AdventureWorksDbContext _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));

    /// <summary>
    /// Retrieves a paginated list of work orders and the total count of work orders in the database.
    /// </summary>
    /// <param name="parameters">the input paging and sorting parameters</param>
    /// <param name="cancellationToken">token to cancel the operation</param>
    /// <returns>A tuple containing the list of work orders and the total count</returns>
    public async Task<(IReadOnlyList<WorkOrder>, int)> GetWorkOrdersAsync(
        WorkOrderParameter parameters,
        CancellationToken cancellationToken = default)
    {
        var totalCount = await _dbContext.WorkOrders.CountAsync(cancellationToken);

        var workOrderQuery = BuildWorkOrderQuery();

        workOrderQuery = ApplyOrdering(workOrderQuery, parameters);
        workOrderQuery = workOrderQuery.Skip(parameters.GetRecordsToSkip()).Take(parameters.PageSize);

        var results = await workOrderQuery.ToListAsync(cancellationToken);

        return (results.AsReadOnly(), totalCount);
    }

    /// <summary>
    /// Retrieves a paged list of work orders filtered using the search model.
    /// </summary>
    /// <param name="parameters">the input paging and sorting parameters</param>
    /// <param name="searchModel">the search filter criteria</param>
    /// <param name="cancellationToken">token to cancel the operation</param>
    /// <returns>A tuple containing the filtered list of work orders and the total count</returns>
    public async Task<(IReadOnlyList<WorkOrder>, int)> SearchWorkOrdersAsync(
        WorkOrderParameter parameters,
        WorkOrderSearchModel searchModel,
        CancellationToken cancellationToken = default)
    {
        var workOrderQuery = BuildWorkOrderQuery();

        if (searchModel.ProductId.HasValue)
        {
            workOrderQuery = workOrderQuery.Where(x => x.ProductId == searchModel.ProductId.Value);
        }

        if (searchModel.StartDate.HasValue)
        {
            workOrderQuery = workOrderQuery.Where(x => x.StartDate >= searchModel.StartDate.Value);
        }

        if (searchModel.EndDate.HasValue)
        {
            workOrderQuery = workOrderQuery.Where(x => x.StartDate <= searchModel.EndDate.Value);
        }

        if (searchModel.HasScrapped.HasValue)
        {
            workOrderQuery = searchModel.HasScrapped.Value
                ? workOrderQuery.Where(x => x.ScrappedQty > 0)
                : workOrderQuery.Where(x => x.ScrappedQty == 0);
        }

        if (searchModel.ScrapReasonId.HasValue)
        {
            workOrderQuery = workOrderQuery.Where(x => x.ScrapReasonId == searchModel.ScrapReasonId.Value);
        }

        var totalCount = await workOrderQuery.CountAsync(cancellationToken);

        workOrderQuery = ApplyOrdering(workOrderQuery, parameters);
        workOrderQuery = workOrderQuery.Skip(parameters.GetRecordsToSkip()).Take(parameters.PageSize);

        var results = await workOrderQuery.ToListAsync(cancellationToken);

        return (results.AsReadOnly(), totalCount);
    }

    /// <summary>
    /// Builds the base query with the product relationship included for <c>ProductName</c> mapping.
    /// </summary>
    /// <returns>A queryable of work orders with the product relationship included</returns>
    private IQueryable<WorkOrder> BuildWorkOrderQuery()
    {
        return _dbContext.WorkOrders
            .AsNoTracking()
            .Include(x => x.Product);
    }

    /// <summary>
    /// Applies ordering to the work order query based on parameters.
    /// </summary>
    /// <param name="query">the queryable to apply ordering to</param>
    /// <param name="parameters">the sorting parameters</param>
    /// <returns>The ordered queryable</returns>
    private static IQueryable<WorkOrder> ApplyOrdering(
        IQueryable<WorkOrder> query,
        WorkOrderParameter parameters)
    {
        return parameters.OrderBy switch
        {
            SortedResultConstants.WorkOrderId => parameters.SortOrder == SortedResultConstants.Ascending
                ? query.OrderBy(x => x.WorkOrderId)
                : query.OrderByDescending(x => x.WorkOrderId),
            SortedResultConstants.DueDate => parameters.SortOrder == SortedResultConstants.Ascending
                ? query.OrderBy(x => x.DueDate).ThenBy(x => x.WorkOrderId)
                : query.OrderByDescending(x => x.DueDate).ThenBy(x => x.WorkOrderId),
            _ => parameters.SortOrder == SortedResultConstants.Ascending
                ? query.OrderBy(x => x.StartDate).ThenBy(x => x.WorkOrderId)
                : query.OrderByDescending(x => x.StartDate).ThenBy(x => x.WorkOrderId)
        };
    }
}
