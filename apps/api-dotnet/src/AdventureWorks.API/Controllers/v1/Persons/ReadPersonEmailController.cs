using AdventureWorks.Application.Features.Person.Queries;
using AdventureWorks.Models.Features.Person;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AdventureWorks.API.Controllers.v1.Persons;

/// <summary>
/// Controller for retrieving email addresses for a person.
/// </summary>
[ApiController]
[Authorize]
[ApiVersion("1.0")]
[ApiExplorerSettings(GroupName = "Person")]
[Produces("application/json")]
[Route("api/v{version:apiVersion}/persons/{personId:int}/emails", Name = "ReadPersonEmailControllerV1")]
public sealed class ReadPersonEmailController : ControllerBase
{
    private readonly ILogger<ReadPersonEmailController> _logger;
    private readonly IMediator _mediator;

    public ReadPersonEmailController(
        ILogger<ReadPersonEmailController> logger,
        IMediator mediator)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(mediator);
        _logger = logger;
        _mediator = mediator;
    }

    /// <summary>
    /// Retrieves all email addresses for the specified person.
    /// </summary>
    /// <param name="personId">The person's BusinessEntityId.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>List of email addresses for the person.</returns>
    /// <response code="200">Emails retrieved successfully.</response>
    /// <response code="400">Invalid person ID supplied.</response>
    /// <response code="404">Person not found.</response>
    [HttpGet(Name = "GetPersonEmails")]
    [ProducesResponseType(typeof(List<PersonEmailModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAllAsync(int personId, CancellationToken cancellationToken = default)
    {
        if (personId <= 0)
        {
            return BadRequest("A valid person id must be specified.");
        }

        _logger.LogInformation("Retrieving all emails for person {PersonId}", personId);

        var emails = await _mediator.Send(new ReadPersonEmailListQuery { PersonId = personId }, cancellationToken);

        _logger.LogInformation("Retrieved {Count} emails for person {PersonId}", emails.Count, personId);

        return Ok(emails);
    }
}
