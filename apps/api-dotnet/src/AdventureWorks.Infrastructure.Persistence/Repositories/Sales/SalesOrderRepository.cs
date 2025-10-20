using AdventureWorks.Application.PersistenceContracts.Repositories.Sales;
using AdventureWorks.Common.Attributes;
using AdventureWorks.Common.Constants;
using AdventureWorks.Common.Filtering;
using AdventureWorks.Domain.Entities.Sales;
using AdventureWorks.Infrastructure.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace AdventureWorks.Infrastructure.Persistence.Repositories.Sales;

/// <summary>
/// Repository for sales order persistence operations.
/// </summary>
[ServiceLifetimeScoped]
public sealed class SalesOrderRepository(AdventureWorksDbContext dbContext)
    : EfRepository<SalesOrderHeader>(dbContext), ISalesOrderRepository
{
    /// <summary>
    /// Retrieves a paginated list of sales orders and the total count of sales orders in the database.
    /// </summary>
    /// <param name="parameters">the input paging and sorting parameters</param>
    /// <param name="cancellationToken">token to cancel the operation</param>
    /// <returns>A tuple containing the list of sales orders and the total count</returns>
    public async Task<(IReadOnlyList<SalesOrderHeader>, int)> GetSalesOrdersAsync(
        SalesOrderParameter parameters,
        CancellationToken cancellationToken = default)
    {
        var totalCount = await DbContext.SalesOrderHeaders.CountAsync(cancellationToken);

        var salesOrderQuery = BuildSalesOrderQuery()
            .AsQueryable();

        salesOrderQuery = ApplyOrdering(salesOrderQuery, parameters);
        salesOrderQuery = salesOrderQuery.Skip(parameters.GetRecordsToSkip()).Take(parameters.PageSize);

        var results = await salesOrderQuery.ToListAsync(cancellationToken);

        return (results.AsReadOnly(), totalCount);
    }

    /// <summary>
    /// Retrieves a paged list of sales orders filtered using the search model.
    /// </summary>
    /// <param name="parameters">the input paging and sorting parameters</param>
    /// <param name="searchModel">the search filter criteria</param>
    /// <param name="cancellationToken">token to cancel the operation</param>
    /// <returns>A tuple containing the filtered list of sales orders and the total count</returns>
    public async Task<(IReadOnlyList<SalesOrderHeader>, int)> SearchSalesOrdersAsync(
        SalesOrderParameter parameters,
        SalesOrderSearchModel searchModel,
        CancellationToken cancellationToken = default)
    {
        var salesOrderQuery = BuildSalesOrderQuery()
            .AsQueryable();

        // Apply search filters
        if (searchModel != null)
        {
            if (searchModel.OrderDateFrom.HasValue)
            {
                salesOrderQuery = salesOrderQuery.Where(x => x.OrderDate >= searchModel.OrderDateFrom.Value);
            }

            if (searchModel.OrderDateTo.HasValue)
            {
                salesOrderQuery = salesOrderQuery.Where(x => x.OrderDate <= searchModel.OrderDateTo.Value);
            }

            if (searchModel.Status.HasValue)
            {
                salesOrderQuery = salesOrderQuery.Where(x => x.Status == searchModel.Status.Value);
            }

            if (searchModel.SalesPersonId.HasValue)
            {
                salesOrderQuery = salesOrderQuery.Where(x => x.SalesPersonId == searchModel.SalesPersonId.Value);
            }

            if (searchModel.TerritoryId.HasValue)
            {
                salesOrderQuery = salesOrderQuery.Where(x => x.TerritoryId == searchModel.TerritoryId.Value);
            }
        }

        var totalCount = await salesOrderQuery.CountAsync(cancellationToken);

        salesOrderQuery = ApplyOrdering(salesOrderQuery, parameters);
        salesOrderQuery = salesOrderQuery.Skip(parameters.GetRecordsToSkip()).Take(parameters.PageSize);

        var results = await salesOrderQuery.ToListAsync(cancellationToken);

        return (results.AsReadOnly(), totalCount);
    }

    /// <summary>
    /// Retrieves the full detail of a single sales order by its identifier, including line items,
    /// addresses, sales person, and territory. Returns null when the order does not exist.
    /// </summary>
    /// <param name="salesOrderId">the sales order primary key</param>
    /// <param name="cancellationToken">token to cancel the operation</param>
    /// <returns>The matching <see cref="SalesOrderHeader"/>, or null if not found</returns>
    public async Task<SalesOrderHeader?> GetSalesOrderDetailAsync(
        int salesOrderId,
        CancellationToken cancellationToken = default)
    {
        return await DbContext.SalesOrderHeaders
            .AsNoTracking()
            .Include(x => x.SalesOrderDetails)
                .ThenInclude(d => d.Product)
            .Include(x => x.BillToAddressEntity)
                .ThenInclude(a => a.StateProvince)
            .Include(x => x.ShipToAddressEntity)
                .ThenInclude(a => a.StateProvince)
            .Include(x => x.SalesPerson)
                .ThenInclude(sp => sp.Employee)
                    .ThenInclude(e => e.PersonBusinessEntity)
            .Include(x => x.TerritoryEntity)
            .FirstOrDefaultAsync(x => x.SalesOrderId == salesOrderId, cancellationToken);
    }

    /// <summary>
    /// Builds the base query with all necessary entity includes.
    /// </summary>
    /// <returns>A queryable of sales orders with customer and sales person relationships included</returns>
    private IQueryable<SalesOrderHeader> BuildSalesOrderQuery()
    {
        return DbContext.SalesOrderHeaders
            .AsNoTracking()
            .Include(x => x.CustomerEntity)
                .ThenInclude(c => c.Person)
            .Include(x => x.CustomerEntity)
                .ThenInclude(c => c.StoreEntity)
            .Include(x => x.SalesPerson)
                .ThenInclude(sp => sp.Employee)
                    .ThenInclude(e => e.PersonBusinessEntity);
    }

    /// <summary>
    /// Applies ordering to the sales order query based on parameters.
    /// </summary>
    /// <param name="query">the queryable to apply ordering to</param>
    /// <param name="parameters">the sorting parameters</param>
    /// <returns>The ordered queryable</returns>
    private static IQueryable<SalesOrderHeader> ApplyOrdering(
        IQueryable<SalesOrderHeader> query,
        SalesOrderParameter parameters)
    {
        return parameters.OrderBy switch
        {
            SortedResultConstants.OrderDate => parameters.SortOrder == SortedResultConstants.Ascending
                ? query.OrderBy(x => x.OrderDate)
                : query.OrderByDescending(x => x.OrderDate),
            SortedResultConstants.TotalDue => parameters.SortOrder == SortedResultConstants.Ascending
                ? query.OrderBy(x => x.TotalDue)
                : query.OrderByDescending(x => x.TotalDue),
            SortedResultConstants.SalesOrderNumber => parameters.SortOrder == SortedResultConstants.Ascending
                ? query.OrderBy(x => x.SalesOrderNumber)
                : query.OrderByDescending(x => x.SalesOrderNumber),
            _ => parameters.SortOrder == SortedResultConstants.Ascending
                ? query.OrderBy(x => x.SalesOrderId)
                : query.OrderByDescending(x => x.SalesOrderId)
        };
    }
}
