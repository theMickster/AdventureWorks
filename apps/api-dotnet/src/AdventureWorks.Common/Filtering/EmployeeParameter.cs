using AdventureWorks.Common.Constants;
using AdventureWorks.Common.Filtering.Base;

namespace AdventureWorks.Common.Filtering;

/// <summary>
/// Used to support paging in the AdventureWorks Employee list feature.
/// </summary>
public sealed class EmployeeParameter : QueryStringParamsBase
{
    private const string EmployeeIdField = "id";
    private const string EmployeeFirstNameField = "firstName";
    private const string EmployeeLastNameField = "lastName";
    private string _orderBy = EmployeeIdField;

    public string OrderBy
    {
        get
        {
            return _orderBy switch
            {
                EmployeeIdField => SortedResultConstants.BusinessEntityId,
                EmployeeFirstNameField => SortedResultConstants.FirstName,
                EmployeeLastNameField => SortedResultConstants.LastName,
                _ => SortedResultConstants.BusinessEntityId
            };
        }
        set
        {
            ArgumentNullException.ThrowIfNull(value);

            var normalized = value.Trim().ToLowerInvariant();
            _orderBy = normalized switch
            {
                EmployeeIdField => EmployeeIdField,
                EmployeeFirstNameField => EmployeeFirstNameField,
                EmployeeLastNameField => EmployeeLastNameField,
                _ => EmployeeIdField
            };
        }
    }
}
