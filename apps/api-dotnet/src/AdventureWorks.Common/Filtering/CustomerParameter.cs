using AdventureWorks.Common.Filtering.Base;

namespace AdventureWorks.Common.Filtering;

/// <summary>
/// Used to support paging and search in the Customer LTV list feature.
/// </summary>
/// <remarks>
/// Uses <c>pageNumber</c>/<c>pageSize</c> (via <see cref="QueryStringParamsBase"/>) for consistency
/// with every other list endpoint in this API, rather than the literal "page" naming used elsewhere
/// in requirements documents for this feature.
/// </remarks>
public sealed class CustomerParameter : QueryStringParamsBase
{
    /// <summary>Optional case-insensitive filter applied to the customer's display name.</summary>
    public string? Search { get; init; }
}
