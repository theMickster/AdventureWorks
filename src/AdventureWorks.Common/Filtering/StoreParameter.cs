using AdventureWorks.Common.Constants;
using AdventureWorks.Common.Filtering.Base;

namespace AdventureWorks.Common.Filtering;

/// <summary>
/// Used to support paging in the AdventureWorks Store list feature.
/// </summary>
public sealed class StoreParameter : QueryStringParamsBase
{
    private const string StoreIdField = "id";
    private const string StoreNameField = "name";
    private string _orderBy = StoreIdField;

    public string OrderBy
    {
        get
        {
            return _orderBy switch
                {
                    StoreIdField => SortedResultConstants.BusinessEntityId,
                    StoreNameField => SortedResultConstants.Name,
                    _ => SortedResultConstants.BusinessEntityId
                };
        }
        set =>
            _orderBy = value?.Trim().ToLower() == StoreIdField ? StoreIdField
                        : value?.Trim().ToLower() == StoreNameField ? StoreNameField : StoreIdField;
    }
}
