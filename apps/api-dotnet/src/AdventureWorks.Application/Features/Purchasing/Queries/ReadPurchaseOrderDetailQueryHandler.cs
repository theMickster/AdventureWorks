using AdventureWorks.Application.PersistenceContracts.Repositories.Purchasing;
using AdventureWorks.Models.Features.Purchasing;
using MediatR;

namespace AdventureWorks.Application.Features.Purchasing.Queries;

/// <summary>
/// Handler for retrieving a single purchase order's full detail, including line items.
/// </summary>
/// <remarks>
/// Throws <see cref="KeyNotFoundException"/> — caught by <c>ExceptionHandlerMiddleware</c> and translated
/// to a structured 404 body (<c>error</c>/<c>correlationId</c>/<c>timestamp</c>) — rather than returning
/// null for the controller to translate. This mirrors <see cref="ReadVendorDetailQueryHandler"/>, the
/// vendor detail sibling feature.
/// </remarks>
public sealed class ReadPurchaseOrderDetailQueryHandler(IPurchaseOrderRepository purchaseOrderRepository)
    : IRequestHandler<ReadPurchaseOrderDetailQuery, PurchaseOrderDetailModel>
{
    private readonly IPurchaseOrderRepository _purchaseOrderRepository = purchaseOrderRepository ?? throw new ArgumentNullException(nameof(purchaseOrderRepository));

    /// <summary>
    /// Handles the query to retrieve a single purchase order's full detail.
    /// </summary>
    /// <param name="request">the query request containing the purchase order identifier</param>
    /// <param name="cancellationToken">token to cancel the operation</param>
    /// <returns>The purchase order detail</returns>
    /// <exception cref="KeyNotFoundException">no purchase order with <see cref="ReadPurchaseOrderDetailQuery.PurchaseOrderId"/> exists</exception>
    public async Task<PurchaseOrderDetailModel> Handle(ReadPurchaseOrderDetailQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var detail = await _purchaseOrderRepository.GetPurchaseOrderDetailAsync(request.PurchaseOrderId, cancellationToken);

        return detail ?? throw new KeyNotFoundException($"Purchase order {request.PurchaseOrderId} was not found.");
    }
}
