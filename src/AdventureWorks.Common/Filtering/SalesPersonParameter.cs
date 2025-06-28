using AdventureWorks.Common.Constants;
using AdventureWorks.Common.Filtering.Base;

namespace AdventureWorks.Common.Filtering;

/// <summary>
/// Used to support paging in the AdventureWorks Sales Person list feature.
/// </summary>
public sealed class SalesPersonParameter : QueryStringParamsBase
{
    private const string SalesPersonIdField = "id";
    private const string SalesPersonFirstNameField = "firstName";
    private const string SalesPersonLastNameField = "lastName";
    private string _orderBy = SalesPersonIdField;

    public string OrderBy
    {
        get
        {
            return _orderBy switch
            {
                SalesPersonIdField => SortedResultConstants.BusinessEntityId,
                SalesPersonFirstNameField => SortedResultConstants.FirstName,
                SalesPersonLastNameField => SortedResultConstants.LastName,
                _ => SortedResultConstants.BusinessEntityId
            };
        }
        set
        {
            ArgumentNullException.ThrowIfNull(value);

            var normalized = value.Trim().ToLowerInvariant();
            _orderBy = normalized switch
            {
                SalesPersonIdField => SalesPersonIdField,
                SalesPersonFirstNameField => SalesPersonFirstNameField,
                SalesPersonLastNameField => SalesPersonLastNameField,
                _ => SalesPersonIdField
            };
        }
    }
}
