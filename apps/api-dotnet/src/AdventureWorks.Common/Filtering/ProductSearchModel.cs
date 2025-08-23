using AdventureWorks.Common.Filtering.Base;
using System.ComponentModel.DataAnnotations;

namespace AdventureWorks.Common.Filtering;

/// <summary>
/// The input model used for searching for AdventureWorks Products
/// </summary>
public sealed class ProductSearchModel : SearchModelBase
{
    [MaxLength(25)]
    public string? ProductNumber { get; set; }

    public int? CategoryId { get; set; }

    public int? SubcategoryId { get; set; }

    [MaxLength(15)]
    public string? Color { get; set; }

    public decimal? MinListPrice { get; set; }

    public decimal? MaxListPrice { get; set; }

    /// <summary>
    /// When true, filters to products where DiscontinuedDate is null (active products).
    /// When false, filters to products where DiscontinuedDate is not null (discontinued products).
    /// </summary>
    public bool? IsActive { get; set; }
}
