using AdventureWorks.Models.Features.Sales;
using MediatR;

namespace AdventureWorks.Application.Features.Sales.Queries;

/// <summary>
/// Query to retrieve the full detail of a single sales order by its identifier.
/// </summary>
public sealed class ReadSalesOrderDetailQuery : IRequest<SalesOrderDetailModel?>
{
    /// <summary>
    /// The sales order primary key.
    /// </summary>
    public required int SalesOrderId { get; set; }
}
