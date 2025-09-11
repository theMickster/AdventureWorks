using AdventureWorks.Application.Features.Person.Queries;
using AdventureWorks.Models.Features.Person;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AdventureWorks.API.Controllers.v1.Persons;

/// <summary>
/// Controller for retrieving consolidated person details.
/// </summary>
[ApiController]
[Authorize]
[ApiVersion("1.0")]
[ApiExplorerSettings(GroupName = "Person")]
[Produces("application/json")]
[Route("api/v{version:apiVersion}/persons", Name = "ReadPersonControllerV1")]
public sealed class ReadPersonController : ControllerBase
{
    private readonly ILogger<ReadPersonController> _logger;
    private readonly IMediator _mediator;

    public ReadPersonController(
        ILogger<ReadPersonController> logger,
        IMediator mediator)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(mediator);
        _logger = logger;
        _mediator = mediator;
    }

    /// <summary>
    /// Retrieves consolidated details for a single person.
    /// </summary>
    /// <param name="personId">The person's BusinessEntityId.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The person details.</returns>
    [HttpGet("{personId:int}", Name = "GetPersonById")]
    [ProducesResponseType(typeof(PersonDetailModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByIdAsync(int personId, CancellationToken cancellationToken = default)
    {
        if (personId <= 0)
        {
            return BadRequest("A valid person id must be specified.");
        }

        _logger.LogInformation("Retrieving details for person {PersonId}", personId);

        var model = await _mediator.Send(new ReadPersonQuery { PersonId = personId }, cancellationToken);

        if (model is null)
        {
            _logger.LogInformation("Person {PersonId} not found", personId);
            return NotFound("Unable to locate the person.");
        }

        return Ok(model);
    }
}
