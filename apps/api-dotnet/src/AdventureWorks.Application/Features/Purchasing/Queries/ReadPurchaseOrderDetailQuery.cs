using AdventureWorks.Models.Features.Purchasing;
using MediatR;

namespace AdventureWorks.Application.Features.Purchasing.Queries;

/// <summary>
/// Query to retrieve a single purchase order's full detail, including line items.
/// </summary>
public sealed class ReadPurchaseOrderDetailQuery : IRequest<PurchaseOrderDetailModel>
{
    /// <summary>
    /// The purchase order primary key.
    /// </summary>
    public required int PurchaseOrderId { get; set; }
}
