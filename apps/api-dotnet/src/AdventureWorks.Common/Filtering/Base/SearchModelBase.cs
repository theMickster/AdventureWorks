using System.ComponentModel.DataAnnotations;

namespace AdventureWorks.Common.Filtering.Base;

public abstract class SearchModelBase
{
    /// <summary>
    /// The unique integer identifier for the resource 
    /// </summary>
    public int? Id { get; set; }

    /// <summary>
    /// The search string to search the name of the resource
    /// </summary>
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;

}
