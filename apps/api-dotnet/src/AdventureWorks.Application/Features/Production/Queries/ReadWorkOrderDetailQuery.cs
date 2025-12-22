using AdventureWorks.Models.Features.Production;
using MediatR;

namespace AdventureWorks.Application.Features.Production.Queries;

/// <summary>
/// Query to retrieve the full detail of a single work order.
/// </summary>
public sealed class ReadWorkOrderDetailQuery : IRequest<WorkOrderDetailModel>
{
    /// <summary>
    /// The unique identifier of the work order to retrieve.
    /// </summary>
    public required int WorkOrderId { get; set; }
}
