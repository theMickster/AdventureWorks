using AdventureWorks.Application.Features.Person.Commands;
using AdventureWorks.Models.Features.Person;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AdventureWorks.API.Controllers.v1.Persons;

/// <summary>
/// Controller for adding an email address to a person.
/// </summary>
[ApiController]
[Authorize]
[ApiVersion("1.0")]
[ApiExplorerSettings(GroupName = "Person")]
[Produces("application/json")]
[Route("api/v{version:apiVersion}/persons/{personId:int}/emails", Name = "CreatePersonEmailControllerV1")]
public sealed class CreatePersonEmailController : ControllerBase
{
    private readonly ILogger<CreatePersonEmailController> _logger;
    private readonly IMediator _mediator;

    public CreatePersonEmailController(
        ILogger<CreatePersonEmailController> logger,
        IMediator mediator)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(mediator);
        _logger = logger;
        _mediator = mediator;
    }

    /// <summary>
    /// Adds a new email address to the specified person.
    /// </summary>
    /// <param name="personId">The person's BusinessEntityId.</param>
    /// <param name="inputModel">The email address payload.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The newly created email address.</returns>
    /// <response code="201">Email created successfully.</response>
    /// <response code="400">Invalid input.</response>
    /// <response code="401">Unauthenticated.</response>
    /// <response code="404">Person not found.</response>
    [HttpPost]
    [ProducesResponseType(typeof(PersonEmailModel), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> PostAsync(
        int personId,
        [FromBody] PersonEmailCreateModel? inputModel,
        CancellationToken cancellationToken = default)
    {
        if (inputModel is null)
        {
            return BadRequest("The email input model cannot be null.");
        }

        if (personId <= 0)
        {
            return BadRequest("A valid person id must be specified.");
        }

        _logger.LogInformation("Adding email for person {PersonId}", personId);

        var command = new AddPersonEmailCommand
        {
            PersonId = personId,
            Model = inputModel
        };

        var model = await _mediator.Send(command, cancellationToken);

        return CreatedAtRoute(
            "GetPersonEmailByCompositeKey",
            new { personId, emailAddressId = model.EmailAddressId },
            model);
    }
}
