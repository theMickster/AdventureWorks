using AdventureWorks.Common.Constants;
using AdventureWorks.Common.Filtering.Base;

namespace AdventureWorks.Common.Filtering;

/// <summary>
/// Used to support paging in the AdventureWorks Product list feature.
/// </summary>
/// <remarks>
/// <para>
/// Valid <c>OrderBy</c> values (case-insensitive): <c>id</c> (default), <c>name</c>,
/// <c>number</c>, <c>listPrice</c>, <c>standardCost</c>. Any unrecognised value falls
/// back to <c>id</c>.
/// </para>
/// <para>
/// <c>PageSize</c> is capped at 50 by the base class <see cref="QueryStringParamsBase"/>.
/// </para>
/// </remarks>
public sealed class ProductParameter : QueryStringParamsBase
{
    private const string ProductIdField = "id";
    private const string ProductNameField = "name";
    private const string ProductNumberField = "number";
    private const string ListPriceField = "listprice";
    private const string StandardCostField = "standardcost";
    private string _orderBy = ProductIdField;

    public string OrderBy
    {
        get
        {
            return _orderBy switch
                {
                    ProductIdField => SortedResultConstants.ProductId,
                    ProductNameField => SortedResultConstants.Name,
                    ProductNumberField => SortedResultConstants.ProductNumber,
                    ListPriceField => SortedResultConstants.ListPrice,
                    StandardCostField => SortedResultConstants.StandardCost,
                    _ => SortedResultConstants.ProductId
                };
        }
        set
        {
            var normalized = value?.Trim().ToLower();
            _orderBy = normalized switch
            {
                ProductIdField => ProductIdField,
                ProductNameField => ProductNameField,
                ProductNumberField => ProductNumberField,
                ListPriceField => ListPriceField,
                StandardCostField => StandardCostField,
                _ => ProductIdField
            };
        }
    }
}
