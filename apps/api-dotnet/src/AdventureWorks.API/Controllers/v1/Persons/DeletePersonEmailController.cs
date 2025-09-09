using AdventureWorks.Application.Features.Person.Commands;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AdventureWorks.API.Controllers.v1.Persons;

/// <summary>
/// Controller for deleting a person's email address.
/// </summary>
[ApiController]
[Authorize]
[ApiVersion("1.0")]
[ApiExplorerSettings(GroupName = "Person")]
[Produces("application/json")]
[Route("api/v{version:apiVersion}/persons/{personId:int}/emails", Name = "DeletePersonEmailControllerV1")]
public sealed class DeletePersonEmailController : ControllerBase
{
    private readonly ILogger<DeletePersonEmailController> _logger;
    private readonly IMediator _mediator;

    public DeletePersonEmailController(
        ILogger<DeletePersonEmailController> logger,
        IMediator mediator)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(mediator);
        _logger = logger;
        _mediator = mediator;
    }

    /// <summary>
    /// Deletes the email address identified by the composite key.
    /// </summary>
    /// <param name="personId">The person's BusinessEntityId.</param>
    /// <param name="emailAddressId">The email address identifier.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>No content.</returns>
    /// <response code="204">Email deleted successfully.</response>
    /// <response code="400">Invalid input.</response>
    /// <response code="401">Unauthenticated.</response>
    /// <response code="404">Email address not found.</response>
    [HttpDelete("{emailAddressId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteAsync(
        int personId,
        int emailAddressId,
        CancellationToken cancellationToken = default)
    {
        if (personId <= 0)
        {
            return BadRequest("A valid person id must be specified.");
        }

        if (emailAddressId <= 0)
        {
            return BadRequest("A valid email address id must be specified.");
        }

        _logger.LogInformation(
            "Deleting email {EmailAddressId} for person {PersonId}", emailAddressId, personId);

        await _mediator.Send(new DeletePersonEmailCommand
        {
            PersonId = personId,
            EmailAddressId = emailAddressId
        }, cancellationToken);

        return NoContent();
    }
}
