using AdventureWorks.Application.Features.Person.Commands;
using AdventureWorks.Models.Features.Person;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AdventureWorks.API.Controllers.v1.Persons;

/// <summary>
/// Controller for adding a phone number to a person.
/// </summary>
[ApiController]
// All authenticated principals are HR administrators by design — object-level role restriction is enforced at token issuance, not at the controller layer.
[Authorize]
[ApiVersion("1.0")]
[ApiExplorerSettings(GroupName = "Person")]
[Produces("application/json")]
[Route("api/v{version:apiVersion}/persons/{personId:int}/phones", Name = "CreatePersonPhoneControllerV1")]
public sealed class CreatePersonPhoneController : ControllerBase
{
    private readonly ILogger<CreatePersonPhoneController> _logger;
    private readonly IMediator _mediator;

    public CreatePersonPhoneController(
        ILogger<CreatePersonPhoneController> logger,
        IMediator mediator)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(mediator);
        _logger = logger;
        _mediator = mediator;
    }

    /// <summary>
    /// Adds a new phone number to the specified person.
    /// </summary>
    /// <param name="personId">The person's BusinessEntityId.</param>
    /// <param name="inputModel">The phone number payload.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The newly created phone number.</returns>
    /// <response code="201">Phone created successfully.</response>
    /// <response code="400">Invalid input.</response>
    /// <response code="401">Unauthenticated.</response>
    /// <response code="404">Person not found.</response>
    [HttpPost]
    [ProducesResponseType(typeof(PersonPhoneModel), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> PostAsync(
        int personId,
        [FromBody] PersonPhoneCreateModel? inputModel,
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

        _logger.LogInformation("Adding phone for person {PersonId}", personId);

        var command = new AddPersonPhoneCommand
        {
            PersonId = personId,
            Model = inputModel
        };

        var model = await _mediator.Send(command, cancellationToken);

        return CreatedAtRoute(
            "GetPersonPhoneByPhoneTypeId",
            new { personId, phoneNumberTypeId = model.PhoneNumberTypeId },
            model);
    }
}
