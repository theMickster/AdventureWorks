using AdventureWorks.Application.Features.Person.Queries;
using AdventureWorks.Models.Features.Person;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AdventureWorks.API.Controllers.v1.Persons;

/// <summary>
/// Controller for retrieving phone numbers for a person.
/// </summary>
[ApiController]
// All authenticated principals are HR administrators by design — object-level role restriction is enforced at token issuance, not at the controller layer.
[Authorize]
[ApiVersion("1.0")]
[ApiExplorerSettings(GroupName = "Person")]
[Produces("application/json")]
[Route("api/v{version:apiVersion}/persons/{personId:int}/phones", Name = "ReadPersonPhoneControllerV1")]
public sealed class ReadPersonPhoneController : ControllerBase
{
    private readonly ILogger<ReadPersonPhoneController> _logger;
    private readonly IMediator _mediator;

    public ReadPersonPhoneController(
        ILogger<ReadPersonPhoneController> logger,
        IMediator mediator)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(mediator);
        _logger = logger;
        _mediator = mediator;
    }

    /// <summary>
    /// Retrieves all phone numbers for the specified person.
    /// </summary>
    /// <param name="personId">The person's BusinessEntityId.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>List of phone numbers for the person.</returns>
    /// <response code="200">Phones retrieved successfully.</response>
    /// <response code="400">Invalid person ID supplied.</response>
    /// <response code="404">Person not found.</response>
    [HttpGet(Name = "GetPersonPhones")]
    [ProducesResponseType(typeof(List<PersonPhoneModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAsync(int personId, CancellationToken cancellationToken = default)
    {
        if (personId <= 0)
        {
            return BadRequest("A valid person id must be specified.");
        }

        _logger.LogInformation("Retrieving all phones for person {PersonId}", personId);

        var phones = await _mediator.Send(new ReadPersonPhoneListQuery { PersonId = personId }, cancellationToken);

        _logger.LogInformation("Retrieved {Count} phones for person {PersonId}", phones.Count, personId);

        return Ok(phones);
    }
}
