using AdventureWorks.Common.Constants;
using System.Diagnostics.Contracts;

namespace AdventureWorks.Common.Filtering.Base;

public abstract class QueryStringParamsBase
{
    private int _take = 10;
    private string _sortOrder = SortedResultConstants.Ascending;

    /// <summary>
    /// The maximum amount of records that a may be requested in a list endpoint
    /// </summary>
    protected int MaxTake => 50;

    /// <summary>
    /// The minimum page number that a may be requested in a list endpoint
    /// </summary>
    protected int MinPageNumber { get; private set; } = 1;

    /// <summary>
    /// The page number requested 
    /// </summary>
    /// <remarks>The page number cannot be less than one (1).</remarks>
    public int PageNumber
    {
        get => MinPageNumber;
        set => MinPageNumber = value <= 0 ? 1 : value;
    }

    /// <summary>
    /// Returns the number of total records to skip.
    /// </summary>
    /// <remarks>Computed property leveraged by the data access provider to start the data retrieval at the correct record.</remarks>
    public int GetRecordsToSkip() => (PageNumber - 1) * PageSize;

    /// <summary>
    /// The amount of records requested to be returned to a list endpoint's caller
    /// </summary>
    /// <remarks>The page size cannot be greater than fifty (50).</remarks>
    public int PageSize
    {
        get => _take;
        set => _take = value > MaxTake ? MaxTake : value;
    }

    /// <summary>
    /// The direction in which to sort the list of results.
    /// </summary>
    public string SortOrder
    {
        get => _sortOrder;
        set => _sortOrder = value == null ? SortedResultConstants.Ascending : value.Trim().ToLower()
            switch
            {
                "asc" => SortedResultConstants.Ascending,
                "ascending" => SortedResultConstants.Ascending,
                "desc" => SortedResultConstants.Descending,
                "descending" => SortedResultConstants.Descending,
                _ => SortedResultConstants.Ascending
            };
    }
}
