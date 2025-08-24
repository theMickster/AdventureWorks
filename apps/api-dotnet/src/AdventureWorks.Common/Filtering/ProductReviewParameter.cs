using AdventureWorks.Common.Constants;
using AdventureWorks.Common.Filtering.Base;

namespace AdventureWorks.Common.Filtering;

/// <summary>
/// Used to support paging in the AdventureWorks Product Review list feature.
/// </summary>
public sealed class ProductReviewParameter : QueryStringParamsBase
{
    private const string IdField = "id";
    private const string RatingField = "rating";
    private const string ReviewDateField = "reviewdate";
    private string _orderBy = IdField;

    public string OrderBy
    {
        get
        {
            return _orderBy switch
                {
                    IdField => SortedResultConstants.ProductReviewId,
                    RatingField => SortedResultConstants.Rating,
                    ReviewDateField => SortedResultConstants.ReviewDate,
                    _ => SortedResultConstants.ProductReviewId
                };
        }
        set =>
            _orderBy = value?.Trim().ToLower() == IdField ? IdField
                        : value?.Trim().ToLower() == RatingField ? RatingField
                        : value?.Trim().ToLower() == ReviewDateField ? ReviewDateField : IdField;
    }
}
