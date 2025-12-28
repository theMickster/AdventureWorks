using AdventureWorks.Application.PersistenceContracts.Repositories.Purchasing;
using AdventureWorks.Common.Attributes;
using AdventureWorks.Domain.Entities.Purchasing;
using AdventureWorks.Infrastructure.Persistence.DbContexts;
using AdventureWorks.Models.Features.Purchasing;
using Microsoft.EntityFrameworkCore;

namespace AdventureWorks.Infrastructure.Persistence.Repositories.Purchasing;

/// <summary>
/// Repository for purchase order persistence operations.
/// </summary>
[ServiceLifetimeScoped]
public sealed class PurchaseOrderRepository(AdventureWorksDbContext dbContext)
    : EfRepository<PurchaseOrderHeader>(dbContext), IPurchaseOrderRepository
{
    /// <summary>
    /// The fallback name used when the approving employee cannot be resolved.
    /// </summary>
    private const string UnknownEmployeeName = "Unknown employee";

    /// <summary>
    /// Retrieves a single purchase order's full detail, including line items.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Include-materialize, not correlated-subquery projection:</b> Unlike
    /// <see cref="VendorRepository.GetVendorPurchaseOrdersAsync"/>'s per-row correlated
    /// subquery for a paged list, this method fetches the single purchase order with
    /// <c>.Include(p => p.PurchaseOrderDetails)</c>, then computes derived fields (<c>DueDate</c>,
    /// <c>ApprovingEmployeeFullName</c>) in C# after materialization. For a single-record fetch
    /// this is simpler and no less efficient than a subquery projection — the subquery style used
    /// for list endpoints exists specifically to avoid pulling full related rows across many
    /// purchase orders at once, which does not apply here.
    /// </para>
    /// <para>
    /// <b>Employee/Person resolved via explicit LEFT JOIN, not <c>Include</c>:</b>
    /// <c>PurchaseOrderHeader.EmployeeId</c> and <c>EmployeeEntity.BusinessEntityId</c> are both
    /// non-nullable, so EF Core treats <c>EmployeeEntity</c>/<c>PersonBusinessEntity</c> as
    /// <i>required</i> navigations by convention. <c>Include</c> on a required navigation whose
    /// principal row does not exist causes the EF Core InMemory provider to drop the entire root
    /// row (inner-join-like behavior), not just leave the navigation null — which would make an
    /// unresolvable employee reference look like the purchase order itself does not exist. An
    /// explicit <c>join ... into ... DefaultIfEmpty()</c> query is a true LEFT JOIN regardless of
    /// the navigation's required/optional status, so it correctly yields a null employee name
    /// without ever affecting whether the purchase order row is found.
    /// </para>
    /// <para>
    /// <b>Null-safe approving employee name:</b> <c>ApprovingEmployeeFullName</c> falls back to
    /// <c>"Unknown employee"</c> when the employee or person cannot be resolved, rather than
    /// throwing. This is a functional requirement — AdventureWorks data can contain purchase
    /// orders whose <c>EmployeeId</c> does not resolve to a real employee/person — not
    /// defensive-only code, and must not be "cleaned up" by assuming the join always succeeds.
    /// </para>
    /// </remarks>
    /// <param name="purchaseOrderId">the purchase order primary key</param>
    /// <param name="cancellationToken">token to cancel the operation</param>
    /// <returns>The purchase order detail, or <c>null</c> if no purchase order with that id exists</returns>
    public async Task<PurchaseOrderDetailModel?> GetPurchaseOrderDetailAsync(int purchaseOrderId, CancellationToken cancellationToken = default)
    {
        var purchaseOrder = await DbContext.PurchaseOrderHeaders
            .AsNoTracking()
            .Include(p => p.PurchaseOrderDetails)
            .FirstOrDefaultAsync(p => p.PurchaseOrderId == purchaseOrderId, cancellationToken);

        if (purchaseOrder is null)
        {
            return null;
        }

        var lineItems = purchaseOrder.PurchaseOrderDetails
            .Select(d => new PurchaseOrderLineItemModel
            {
                PurchaseOrderDetailId = d.PurchaseOrderDetailId,
                ProductId = d.ProductId,
                DueDate = d.DueDate,
                OrderQty = d.OrderQty,
                UnitPrice = d.UnitPrice,
                LineTotal = d.LineTotal,
                ReceivedQty = d.ReceivedQty,
                RejectedQty = d.RejectedQty,
                StockedQty = d.StockedQty
            })
            .ToList();

        // PurchaseOrderHeader has no DueDate column: derive it as the earliest line-item due
        // date, falling back to OrderDate when there are zero line items.
        var dueDate = lineItems.Count > 0
            ? lineItems.Min(d => d.DueDate)
            : purchaseOrder.OrderDate;

        // Explicit LEFT JOIN: see remarks above for why Include cannot be used here.
        var approvingEmployeeFullName = await (
            from employee in DbContext.Employees.AsNoTracking()
            where employee.BusinessEntityId == purchaseOrder.EmployeeId
            join person in DbContext.Persons.AsNoTracking()
                on employee.BusinessEntityId equals person.BusinessEntityId into personJoin
            from person in personJoin.DefaultIfEmpty()
            select person == null ? null : person.FirstName + " " + person.LastName)
            .FirstOrDefaultAsync(cancellationToken)
            ?? UnknownEmployeeName;

        return new PurchaseOrderDetailModel
        {
            PurchaseOrderId = purchaseOrder.PurchaseOrderId,
            Status = purchaseOrder.Status,
            StatusLabel = PurchaseOrderStatusLabels.GetLabel(purchaseOrder.Status),
            OrderDate = purchaseOrder.OrderDate,
            DueDate = dueDate,
            ShipDate = purchaseOrder.ShipDate,
            VendorId = purchaseOrder.VendorId,
            EmployeeId = purchaseOrder.EmployeeId,
            ApprovingEmployeeFullName = approvingEmployeeFullName,
            ShipMethodId = purchaseOrder.ShipMethodId,
            SubTotal = purchaseOrder.SubTotal,
            TaxAmt = purchaseOrder.TaxAmt,
            Freight = purchaseOrder.Freight,
            TotalDue = purchaseOrder.TotalDue,
            LineItems = lineItems
        };
    }
}
