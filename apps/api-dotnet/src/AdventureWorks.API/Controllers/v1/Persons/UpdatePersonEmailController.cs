using AdventureWorks.Application.Features.Person.Commands;
using AdventureWorks.Application.Features.Person.Queries;
using AdventureWorks.Models.Features.Person;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AdventureWorks.API.Controllers.v1.Persons;

/// <summary>
/// Controller for updating a person's email address.
/// </summary>
[ApiController]
[Authorize]
[ApiVersion("1.0")]
[ApiExplorerSettings(GroupName = "Person")]
[Produces("application/json")]
[Route("api/v{version:apiVersion}/persons/{personId:int}/emails", Name = "UpdatePersonEmailControllerV1")]
public sealed class UpdatePersonEmailController : ControllerBase
{
    private readonly ILogger<UpdatePersonEmailController> _logger;
    private readonly IMediator _mediator;

    public UpdatePersonEmailController(
        ILogger<UpdatePersonEmailController> logger,
        IMediator mediator)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(mediator);
        _logger = logger;
        _mediator = mediator;
    }

    /// <summary>
    /// Updates an existing email address for the specified person.
    /// </summary>
    /// <param name="personId">The person's BusinessEntityId.</param>
    /// <param name="emailAddressId">The email address identifier.</param>
    /// <param name="inputModel">The update payload.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The updated email address.</returns>
    /// <response code="200">Email updated successfully.</response>
    /// <response code="400">Invalid input.</response>
    /// <response code="401">Unauthenticated.</response>
    /// <response code="404">Person or email address not found.</response>
    [HttpPut("{emailAddressId:int}", Name = "GetPersonEmailByCompositeKey")]
    [ProducesResponseType(typeof(PersonEmailModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> PutAsync(
        int personId,
        int emailAddressId,
        [FromBody] PersonEmailUpdateModel? inputModel,
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

        if (emailAddressId <= 0)
        {
            return BadRequest("A valid email address id must be specified.");
        }

        _logger.LogInformation(
            "Updating email {EmailAddressId} for person {PersonId}", emailAddressId, personId);

        var command = new UpdatePersonEmailCommand
        {
            PersonId = personId,
            EmailAddressId = emailAddressId,
            Model = inputModel
        };

        await _mediator.Send(command, cancellationToken);

        var model = await _mediator.Send(new ReadPersonEmailQuery
        {
            PersonId = personId,
            EmailAddressId = emailAddressId
        }, cancellationToken);

        if (model is null)
        {
            return NotFound(
                $"Email address not found after update for PersonId={personId}, EmailAddressId={emailAddressId}.");
        }

        return Ok(model);
    }
}
