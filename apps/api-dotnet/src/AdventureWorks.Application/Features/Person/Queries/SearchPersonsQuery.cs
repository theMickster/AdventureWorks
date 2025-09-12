using AdventureWorks.Models.Features.Person;
using MediatR;

namespace AdventureWorks.Application.Features.Person.Queries;

/// <summary>
/// Query to search for persons with optional filters and pagination.
/// </summary>
public sealed class SearchPersonsQuery : IRequest<SearchPersonsQueryResult>
{
    /// <summary>
    /// Filter by first name (partial match).
    /// </summary>
    public string? FirstName { get; set; }

    /// <summary>
    /// Filter by last name (partial match).
    /// </summary>
    public string? LastName { get; set; }

    /// <summary>
    /// Filter by person type code (exact match).
    /// </summary>
    public string? PersonTypeCode { get; set; }

    /// <summary>
    /// The page number (1-based). Defaults to 1.
    /// </summary>
    public int Page { get; set; } = 1;

    /// <summary>
    /// The number of results per page (1-100). Defaults to 20.
    /// </summary>
    public int PageSize { get; set; } = 20;
}

/// <summary>
/// Result of a person search query with pagination metadata.
/// </summary>
public sealed class SearchPersonsQueryResult
{
    /// <summary>
    /// The list of persons matching the search criteria.
    /// </summary>
    public List<SearchPersonsModel> Items { get; set; } = [];

    /// <summary>
    /// The total number of persons matching the search criteria (across all pages).
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// The current page number.
    /// </summary>
    public int PageNumber { get; set; }

    /// <summary>
    /// The number of items per page.
    /// </summary>
    public int PageSize { get; set; }
}
