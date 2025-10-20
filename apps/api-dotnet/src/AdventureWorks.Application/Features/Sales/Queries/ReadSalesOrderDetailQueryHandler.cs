using AdventureWorks.Application.PersistenceContracts.Repositories.Sales;
using AdventureWorks.Models.Features.Sales;
using AutoMapper;
using MediatR;

namespace AdventureWorks.Application.Features.Sales.Queries;

/// <summary>
/// Handler for retrieving the full detail of a single sales order.
/// </summary>
public sealed class ReadSalesOrderDetailQueryHandler(
    IMapper mapper,
    ISalesOrderRepository salesOrderRepository)
    : IRequestHandler<ReadSalesOrderDetailQuery, SalesOrderDetailModel?>
{
    private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    private readonly ISalesOrderRepository _salesOrderRepository = salesOrderRepository ?? throw new ArgumentNullException(nameof(salesOrderRepository));

    /// <summary>
    /// Handles the query to retrieve full detail for a single sales order.
    /// </summary>
    /// <param name="request">the query request containing the sales order identifier</param>
    /// <param name="cancellationToken">token to cancel the operation</param>
    /// <returns>The detail model, or null if the order does not exist</returns>
    public async Task<SalesOrderDetailModel?> Handle(
        ReadSalesOrderDetailQuery request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var entity = await _salesOrderRepository.GetSalesOrderDetailAsync(request.SalesOrderId, cancellationToken);

        if (entity is null)
        {
            return null;
        }

        return _mapper.Map<SalesOrderDetailModel>(entity);
    }
}
