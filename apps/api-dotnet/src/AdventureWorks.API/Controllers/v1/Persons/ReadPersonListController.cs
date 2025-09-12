using AdventureWorks.Application.Features.Person.Queries;
using AdventureWorks.Common.Filtering;
using AdventureWorks.Models.Features.Person;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AdventureWorks.API.Controllers.v1.Persons;

/// <summary>
/// Controller for searching and listing persons with optional filters and pagination.
/// </summary>
[ApiController]
[Authorize]
[ApiVersion("1.0")]
[ApiExplorerSettings(GroupName = "Person")]
[Produces("application/json")]
[Route("api/v{version:apiVersion}/persons", Name = "ReadPersonListControllerV1")]
public sealed class ReadPersonListController : ControllerBase
{
    private readonly ILogger<ReadPersonListController> _logger;
    private readonly IMediator _mediator;

    public ReadPersonListController(
        ILogger<ReadPersonListController> logger,
        IMediator mediator)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(mediator);
        _logger = logger;
        _mediator = mediator;
    }

    /// <summary>
    /// Searches for persons based on optional filters with pagination.
    /// At least one search criterion (firstName, lastName, or personTypeCode) is required.
    /// </summary>
    /// <param name="firstName">Optional: Filter by first name (partial match, case-insensitive).</param>
    /// <param name="lastName">Optional: Filter by last name (partial match, case-insensitive).</param>
    /// <param name="personTypeCode">Optional: Filter by person type code (e.g., "EM", "SC", "IN", "VC").</param>
    /// <param name="page">Page number (default 1, minimum 1).</param>
    /// <param name="pageSize">Page size (default 20, range 1-100).</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A paginated list of persons matching the search criteria.</returns>
    /// <response code="200">Person list retrieved successfully.</response>
    /// <response code="400">Invalid search parameters (no filters provided, invalid pagination).</response>
    /// <response code="401">Unauthorized.</response>
    /// <response code="500">Internal server error.</response>
    [HttpGet(Name = "SearchPersons")]
    [ProducesResponseType(typeof(SearchPersonsQueryResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAsync(
        [FromQuery] string? firstName,
        [FromQuery] string? lastName,
        [FromQuery] string? personTypeCode,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Searching for persons - Page: {Page}, PageSize: {PageSize}", page, pageSize);

        var query = new SearchPersonsQuery
        {
            FirstName = firstName,
            LastName = lastName,
            PersonTypeCode = personTypeCode,
            Page = page,
            PageSize = pageSize
        };

        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }
}
