using AdventureWorks.Common.Constants;
using AdventureWorks.Common.Filtering.Base;

namespace AdventureWorks.Common.Filtering;

/// <summary>
/// Used to support paging in the store customer list feature.
/// </summary>
/// <remarks>
/// Unlike the rest of the codebase, the customer list defaults to descending order
/// because the most useful default view is highest-spending customers first.
/// </remarks>
public sealed class StoreCustomerParameter : QueryStringParamsBase
{
    /// <summary>Canonical sort column: customer lifetime spend (default).</summary>
    public const string LifetimeSpend = "LifetimeSpend";

    /// <summary>Canonical sort column: contact person full name.</summary>
    public const string PersonName = "PersonName";

    /// <summary>Canonical sort column: total order count for the customer.</summary>
    public const string OrderCount = "OrderCount";

    /// <summary>Canonical sort column: date of the customer's most recent order.</summary>
    public const string LastOrderDate = "LastOrderDate";

    private const string LifetimeSpendField = "lifetimespend";
    private const string PersonNameField = "personname";
    private const string OrderCountField = "ordercount";
    private const string LastOrderDateField = "lastorderdate";

    private string _orderBy = LifetimeSpend;
    private string _sortOrder = SortedResultConstants.Descending;

    /// <summary>
    /// The column to sort the customer list by. Accepts <see cref="LifetimeSpend"/>,
    /// <see cref="PersonName"/>, <see cref="OrderCount"/>, or <see cref="LastOrderDate"/>
    /// (case-insensitive); any other value falls back to <see cref="LifetimeSpend"/>.
    /// </summary>
    public string OrderBy
    {
        get => _orderBy;
        set =>
            _orderBy = value?.Trim().ToLower() switch
            {
                LifetimeSpendField => LifetimeSpend,
                PersonNameField => PersonName,
                OrderCountField => OrderCount,
                LastOrderDateField => LastOrderDate,
                _ => LifetimeSpend
            };
    }

    /// <summary>
    /// The direction in which to sort the list of results.
    /// Defaults to <see cref="SortedResultConstants.Descending"/> for the customer list,
    /// overriding the base ascending default so the most relevant rows surface first.
    /// </summary>
    public override string SortOrder
    {
        get => _sortOrder;
        init => _sortOrder = value == null ? SortedResultConstants.Descending : value.Trim().ToLower()
            switch
            {
                "asc" or "ascending" => SortedResultConstants.Ascending,
                "desc" or "descending" => SortedResultConstants.Descending,
                _ => SortedResultConstants.Descending
            };
    }
}
