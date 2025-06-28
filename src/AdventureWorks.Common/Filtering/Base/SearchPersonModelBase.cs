namespace AdventureWorks.Common.Filtering.Base;

public abstract class SearchPersonModelBase
{
    /// <summary>
    /// The unique integer identifier of the person entity 
    /// </summary>
    public int? Id { get; set; }

    /// <summary>
    /// The first name of the person entity 
    /// </summary>
    public string? FirstName { get; set; }

    /// <summary>
    /// The first name of the person entity 
    /// </summary>
    public string? LastName { get; set; }
}
