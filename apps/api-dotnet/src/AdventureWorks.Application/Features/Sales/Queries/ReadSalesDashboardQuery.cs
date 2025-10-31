using AdventureWorks.Models.Features.Sales;
using MediatR;

namespace AdventureWorks.Application.Features.Sales.Queries;

/// <summary>
/// Query that returns the aggregate KPI data for the sales dashboard.
/// </summary>
public sealed class ReadSalesDashboardQuery : IRequest<SalesDashboardModel> { }
