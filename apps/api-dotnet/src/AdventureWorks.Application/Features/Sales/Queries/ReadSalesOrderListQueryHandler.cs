using AdventureWorks.Application.PersistenceContracts.Repositories.Sales;
using AdventureWorks.Domain.Entities.Sales;
using AdventureWorks.Models.Features.Sales;
using AutoMapper;
using FluentValidation;
using MediatR;

namespace AdventureWorks.Application.Features.Sales.Queries;

/// <summary>
/// Handler for retrieving paginated sales order lists.
/// </summary>
public sealed class ReadSalesOrderListQueryHandler(
    IMapper mapper,
    ISalesOrderRepository salesOrderRepository,
    IValidator<ReadSalesOrderListQuery> validator)
    : IRequestHandler<ReadSalesOrderListQuery, SalesOrderSearchResultModel>
{
    private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    private readonly ISalesOrderRepository _salesOrderRepository = salesOrderRepository ?? throw new ArgumentNullException(nameof(salesOrderRepository));
    private readonly IValidator<ReadSalesOrderListQuery> _validator = validator ?? throw new ArgumentNullException(nameof(validator));

    /// <summary>
    /// Handles the query to retrieve a paginated list of sales orders.
    /// </summary>
    /// <param name="request">the query request containing pagination and filter parameters</param>
    /// <param name="cancellationToken">token to cancel the operation</param>
    /// <returns>A search result model containing the paginated list of sales orders</returns>
    public async Task<SalesOrderSearchResultModel> Handle(
        ReadSalesOrderListQuery request,
        CancellationToken cancellationToken)
    {
        await _validator.ValidateAndThrowAsync(request, cancellationToken);

        var result = new SalesOrderSearchResultModel
        {
            PageNumber = request.Parameters.PageNumber,
            PageSize = request.Parameters.PageSize,
            TotalRecords = 0,
            Results = new List<SalesOrderModel>()
        };

        IReadOnlyList<SalesOrderHeader> salesOrderEntities;
        var totalRecords = 0;

        if (request.SearchModel is null)
        {
            (salesOrderEntities, totalRecords) = await _salesOrderRepository.GetSalesOrdersAsync(
                request.Parameters,
                cancellationToken);
        }
        else
        {
            (salesOrderEntities, totalRecords) = await _salesOrderRepository.SearchSalesOrdersAsync(
                request.Parameters,
                request.SearchModel,
                cancellationToken);
        }

        if (salesOrderEntities is null or { Count: 0 })
        {
            result.TotalRecords = totalRecords;
            return result;
        }

        var salesOrders = _mapper.Map<List<SalesOrderModel>>(salesOrderEntities);

        result.Results = salesOrders;
        result.TotalRecords = totalRecords;

        return result;
    }
}
