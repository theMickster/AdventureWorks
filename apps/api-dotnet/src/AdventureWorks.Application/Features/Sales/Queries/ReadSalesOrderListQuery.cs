using AdventureWorks.Common.Filtering;
using AdventureWorks.Models.Features.Sales;
using MediatR;

namespace AdventureWorks.Application.Features.Sales.Queries;

/// <summary>
/// Query to retrieve a paginated list of sales orders.
/// </summary>
public sealed class ReadSalesOrderListQuery : IRequest<SalesOrderSearchResultModel>
{
    /// <summary>
    /// Pagination and sorting parameters.
    /// </summary>
    public required SalesOrderParameter Parameters { get; set; }

    /// <summary>
    /// Optional search/filter criteria.
    /// </summary>
    public SalesOrderSearchModel? SearchModel { get; set; }
}
