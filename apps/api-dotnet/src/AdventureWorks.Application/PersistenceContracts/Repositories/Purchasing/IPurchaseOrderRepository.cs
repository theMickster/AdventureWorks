using AdventureWorks.Domain.Entities.Purchasing;
using AdventureWorks.Models.Features.Purchasing;

namespace AdventureWorks.Application.PersistenceContracts.Repositories.Purchasing;

/// <summary>
/// Repository interface for purchase order persistence operations.
/// </summary>
public interface IPurchaseOrderRepository : IAsyncRepository<PurchaseOrderHeader>
{
    /// <summary>
    /// Retrieves a single purchase order's full detail, including line items.
    /// </summary>
    /// <remarks>
    /// <b>Null-safe approving employee name:</b> <c>ApprovingEmployeeFullName</c> is resolved via
    /// <c>PurchaseOrderHeader.EmployeeEntity.PersonBusinessEntity</c>. When either link cannot be
    /// resolved, the result falls back to <c>"Unknown employee"</c> rather than throwing — the
    /// purchase order itself is still returned. This is a functional requirement, not
    /// defensive-only code, since AdventureWorks data can contain unresolvable employee
    /// references.
    /// </remarks>
    /// <param name="purchaseOrderId">the purchase order primary key</param>
    /// <param name="cancellationToken">token to cancel the operation</param>
    /// <returns>The purchase order detail, or <c>null</c> if no purchase order with that id exists</returns>
    Task<PurchaseOrderDetailModel?> GetPurchaseOrderDetailAsync(int purchaseOrderId, CancellationToken cancellationToken = default);
}
