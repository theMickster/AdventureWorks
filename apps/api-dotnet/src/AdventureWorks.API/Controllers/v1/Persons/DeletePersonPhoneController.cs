using AdventureWorks.Application.Features.Person.Commands;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AdventureWorks.API.Controllers.v1.Persons;

/// <summary>
/// Controller for deleting a person's phone number.
/// </summary>
[ApiController]
// All authenticated principals are HR administrators by design — object-level role restriction is enforced at token issuance, not at the controller layer.
[Authorize]
[ApiVersion("1.0")]
[ApiExplorerSettings(GroupName = "Person")]
[Produces("application/json")]
[Route("api/v{version:apiVersion}/persons/{personId:int}/phones", Name = "DeletePersonPhoneControllerV1")]
public sealed class DeletePersonPhoneController : ControllerBase
{
    private readonly ILogger<DeletePersonPhoneController> _logger;
    private readonly IMediator _mediator;

    public DeletePersonPhoneController(
        ILogger<DeletePersonPhoneController> logger,
        IMediator mediator)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(mediator);
        _logger = logger;
        _mediator = mediator;
    }

    /// <summary>
    /// Deletes the phone number identified by the person and phone type.
    /// </summary>
    /// <param name="personId">The person's BusinessEntityId.</param>
    /// <param name="phoneNumberTypeId">The phone number type identifier.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>No content.</returns>
    /// <response code="204">Phone deleted successfully.</response>
    /// <response code="400">Invalid input.</response>
    /// <response code="401">Unauthenticated.</response>
    /// <response code="404">Person or phone number not found.</response>
    [HttpDelete("{phoneNumberTypeId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteAsync(
        int personId,
        int phoneNumberTypeId,
        CancellationToken cancellationToken = default)
    {
        if (personId <= 0)
        {
            return BadRequest("A valid person id must be specified.");
        }

        if (phoneNumberTypeId <= 0)
        {
            return BadRequest("A valid phone number type id must be specified.");
        }

        _logger.LogInformation(
            "Deleting phone type {PhoneNumberTypeId} for person {PersonId}", phoneNumberTypeId, personId);

        await _mediator.Send(new DeletePersonPhoneCommand
        {
            PersonId = personId,
            PhoneNumberTypeId = phoneNumberTypeId
        }, cancellationToken);

        return NoContent();
    }
}
