namespace AdventureWorks.Domain.Models.Base;

public abstract class SearchResultBaseModel<T>
{
    public int PageNumber { get; set; }
    
    public int PageSize { get; set; }

    public int TotalPages { get; set; }

    public int TotalRecords { get; set; }

    public IReadOnlyList<T> Results { get; set; }
}
