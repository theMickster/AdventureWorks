using AdventureWorks.Application.PersistenceContracts.Repositories.Production;
using AdventureWorks.Domain.Entities.Production;
using AdventureWorks.Models.Features.Production;
using AutoMapper;
using FluentValidation;
using MediatR;

namespace AdventureWorks.Application.Features.Production.Queries;

/// <summary>
/// Handler for retrieving paginated work order lists.
/// </summary>
public sealed class ReadWorkOrderListQueryHandler(
    IMapper mapper,
    IWorkOrderRepository workOrderRepository,
    IValidator<ReadWorkOrderListQuery> validator)
    : IRequestHandler<ReadWorkOrderListQuery, WorkOrderSearchResultModel>
{
    private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    private readonly IWorkOrderRepository _workOrderRepository = workOrderRepository ?? throw new ArgumentNullException(nameof(workOrderRepository));
    private readonly IValidator<ReadWorkOrderListQuery> _validator = validator ?? throw new ArgumentNullException(nameof(validator));

    /// <summary>
    /// Handles the query to retrieve a paginated list of work orders.
    /// </summary>
    /// <param name="request">the query request containing pagination and filter parameters</param>
    /// <param name="cancellationToken">token to cancel the operation</param>
    /// <returns>A search result model containing the paginated list of work orders</returns>
    public async Task<WorkOrderSearchResultModel> Handle(
        ReadWorkOrderListQuery request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        await _validator.ValidateAndThrowAsync(request, cancellationToken);

        var result = new WorkOrderSearchResultModel
        {
            PageNumber = request.Parameters.PageNumber,
            PageSize = request.Parameters.PageSize,
            TotalRecords = 0,
            Results = new List<WorkOrderModel>()
        };

        IReadOnlyList<WorkOrder> workOrderEntities;
        var totalRecords = 0;

        if (request.SearchModel is null)
        {
            (workOrderEntities, totalRecords) = await _workOrderRepository.GetWorkOrdersAsync(
                request.Parameters,
                cancellationToken);
        }
        else
        {
            (workOrderEntities, totalRecords) = await _workOrderRepository.SearchWorkOrdersAsync(
                request.Parameters,
                request.SearchModel,
                cancellationToken);
        }

        if (workOrderEntities is null or { Count: 0 })
        {
            result.TotalRecords = totalRecords;
            return result;
        }

        var workOrders = _mapper.Map<List<WorkOrderModel>>(workOrderEntities);

        result.Results = workOrders;
        result.TotalRecords = totalRecords;

        return result;
    }
}
