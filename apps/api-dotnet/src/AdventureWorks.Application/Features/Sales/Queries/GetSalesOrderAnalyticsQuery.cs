using AdventureWorks.Common.Filtering;
using AdventureWorks.Models.Features.Sales;
using MediatR;

namespace AdventureWorks.Application.Features.Sales.Queries;

/// <summary>
/// Query that returns aggregated analytics for a filtered slice of sales orders.
/// </summary>
public sealed class GetSalesOrderAnalyticsQuery : IRequest<SalesOrderAnalyticsModel>
{
    /// <summary>
    /// Optional filter criteria. Null returns analytics for the full dataset.
    /// </summary>
    public SalesOrderSearchModel? Filter { get; set; }
}
