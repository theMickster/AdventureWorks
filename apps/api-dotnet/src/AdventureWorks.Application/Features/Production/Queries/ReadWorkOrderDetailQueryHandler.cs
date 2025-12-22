using AdventureWorks.Application.PersistenceContracts.Repositories.Production;
using AdventureWorks.Models.Features.Production;
using AutoMapper;
using MediatR;

namespace AdventureWorks.Application.Features.Production.Queries;

/// <summary>
/// Handler for retrieving the full detail of a single work order.
/// </summary>
public sealed class ReadWorkOrderDetailQueryHandler(
    IMapper mapper,
    IWorkOrderRepository workOrderRepository)
    : IRequestHandler<ReadWorkOrderDetailQuery, WorkOrderDetailModel>
{
    private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    private readonly IWorkOrderRepository _workOrderRepository = workOrderRepository ?? throw new ArgumentNullException(nameof(workOrderRepository));

    /// <summary>
    /// Handles the query to retrieve the full detail of a single work order.
    /// </summary>
    /// <param name="request">the query request containing the work order identifier</param>
    /// <param name="cancellationToken">token to cancel the operation</param>
    /// <returns>The mapped work order detail model</returns>
    /// <exception cref="KeyNotFoundException">Thrown when no work order exists with the given identifier</exception>
    public async Task<WorkOrderDetailModel> Handle(
        ReadWorkOrderDetailQuery request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var workOrder = await _workOrderRepository.GetByIdAsync(request.WorkOrderId, cancellationToken);

        if (workOrder is null)
        {
            throw new KeyNotFoundException($"Work order with ID {request.WorkOrderId} not found.");
        }

        return _mapper.Map<WorkOrderDetailModel>(workOrder);
    }
}
