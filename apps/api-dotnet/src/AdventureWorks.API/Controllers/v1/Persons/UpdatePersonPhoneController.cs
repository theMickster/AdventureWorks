using AdventureWorks.Application.Features.Person.Commands;
using AdventureWorks.Models.Features.Person;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AdventureWorks.API.Controllers.v1.Persons;

/// <summary>
/// Controller for updating a person's phone number.
/// </summary>
[ApiController]
// All authenticated principals are HR administrators by design — object-level role restriction is enforced at token issuance, not at the controller layer.
[Authorize]
[ApiVersion("1.0")]
[ApiExplorerSettings(GroupName = "Person")]
[Produces("application/json")]
[Route("api/v{version:apiVersion}/persons/{personId:int}/phones", Name = "UpdatePersonPhoneControllerV1")]
public sealed class UpdatePersonPhoneController : ControllerBase
{
    private readonly ILogger<UpdatePersonPhoneController> _logger;
    private readonly IMediator _mediator;

    public UpdatePersonPhoneController(
        ILogger<UpdatePersonPhoneController> logger,
        IMediator mediator)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(mediator);
        _logger = logger;
        _mediator = mediator;
    }

    /// <summary>
    /// Updates the phone number for the specified person and phone type.
    /// The phone number type stays fixed; only the number is replaced via transactional delete+insert.
    /// </summary>
    /// <param name="personId">The person's BusinessEntityId.</param>
    /// <param name="phoneNumberTypeId">The phone number type identifier.</param>
    /// <param name="inputModel">The update payload containing the new phone number.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The updated phone number.</returns>
    /// <response code="200">Phone updated successfully.</response>
    /// <response code="400">Invalid input.</response>
    /// <response code="401">Unauthenticated.</response>
    /// <response code="404">Person or phone number not found.</response>
    [HttpPut("{phoneNumberTypeId:int}", Name = "GetPersonPhoneByPhoneTypeId")]
    [ProducesResponseType(typeof(PersonPhoneModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> PutAsync(
        int personId,
        int phoneNumberTypeId,
        [FromBody] PersonPhoneUpdateModel? inputModel,
        CancellationToken cancellationToken = default)
    {
        if (inputModel is null)
        {
            return BadRequest("The phone input model cannot be null.");
        }

        if (personId <= 0)
        {
            return BadRequest("A valid person id must be specified.");
        }

        if (phoneNumberTypeId <= 0)
        {
            return BadRequest("A valid phone number type id must be specified.");
        }

        _logger.LogInformation(
            "Updating phone type {PhoneNumberTypeId} for person {PersonId}", phoneNumberTypeId, personId);

        var command = new UpdatePersonPhoneCommand
        {
            PersonId = personId,
            PhoneNumberTypeId = phoneNumberTypeId,
            Model = inputModel
        };

        var model = await _mediator.Send(command, cancellationToken);

        return Ok(model);
    }
}
