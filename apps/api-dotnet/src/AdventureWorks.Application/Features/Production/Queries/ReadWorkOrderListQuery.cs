using AdventureWorks.Common.Filtering;
using AdventureWorks.Models.Features.Production;
using MediatR;

namespace AdventureWorks.Application.Features.Production.Queries;

/// <summary>
/// Query to retrieve a paginated list of work orders.
/// </summary>
public sealed class ReadWorkOrderListQuery : IRequest<WorkOrderSearchResultModel>
{
    /// <summary>
    /// Pagination and sorting parameters.
    /// </summary>
    public required WorkOrderParameter Parameters { get; set; }

    /// <summary>
    /// Optional search/filter criteria.
    /// </summary>
    public WorkOrderSearchModel? SearchModel { get; set; }
}
