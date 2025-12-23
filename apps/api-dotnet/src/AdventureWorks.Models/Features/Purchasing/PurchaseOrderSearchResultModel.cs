using AdventureWorks.Models.Base;

namespace AdventureWorks.Models.Features.Purchasing;

/// <summary>
/// Search result model for a vendor's paginated, filterable purchase order history.
/// </summary>
public sealed class PurchaseOrderSearchResultModel : SearchResultBaseModel<PurchaseOrderSummaryModel>
{
}
