namespace AdventureWorks.Models.Base;

public abstract class SearchResultBaseModel<T>
{
    public int PageNumber { get; set; } = 1;

    public int PageSize { get; set; } = 50;

    public int TotalPages => PageSize == int.MinValue ? 0 : Convert.ToInt32( Math.Ceiling( TotalRecords / (double)PageSize));

    public bool HasPreviousPage => PageNumber > 1;

    public bool HasNextPage => PageNumber < TotalPages;

    public int TotalRecords { get; set; } = 0;

    public IReadOnlyList<T> Results { get; set; }
}
