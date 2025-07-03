using AdventureWorks.Application.Features.HumanResources.Queries;
using AdventureWorks.Models.Features.HumanResources;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AdventureWorks.API.Controllers.v1.ContactType;

/// <summary>
/// The controller that coordinates retrieving Contact Type information.
/// </summary>
/// <remarks></remarks>
[ApiController]
[Authorize]
[ApiVersion("1.0")]
[ApiExplorerSettings(GroupName = "Contact Type")]
[Produces("application/json")]
[Route("api/v{version:apiVersion}/contactTypes", Name = "ReadContactTypeControllerV1")]
public class ReadContactTypeController : ControllerBase
{
    private readonly ILogger<ReadContactTypeController> _logger;
    private readonly IMediator _mediator;

    /// <summary>
    /// The controller that coordinates retrieving Contact Type information.
    /// </summary>
    /// <remarks></remarks>
    public ReadContactTypeController(
        ILogger<ReadContactTypeController> logger,
        IMediator mediator)
    {
        ArgumentNullException.ThrowIfNull(logger, nameof(logger));
        ArgumentNullException.ThrowIfNull(mediator, nameof(mediator));
        _logger = logger;
        _mediator = mediator;
    }

    /// <summary>
    /// Retrieve a contact type using its unique identifier
    /// </summary>
    /// <param name="id">the unique identifier</param>
    /// <returns></returns>
    [HttpGet("{id:int}", Name = "GetContactTypeById")]
    [Produces(typeof(ContactTypeModel))]
    public async Task<IActionResult> GetByIdAsync(int id)
    {
        if (id <= 0)
        {
            return BadRequest("A valid contact type id must be specified.");
        }

        var model = await _mediator.Send(new ReadContactTypeQuery{ Id = id });

        return model is null ? NotFound("Unable to locate the contact type.") : Ok(model);
    }

    /// <summary>
    /// Retrieve the complete contact type list
    /// </summary>
    /// <returns></returns>
    [HttpGet(Name = "GetContactTypes")]
    [Produces(typeof(ContactTypeModel))]
    public async Task<IActionResult> GetListAsync()
    {
        var model = await _mediator.Send(new ReadContactTypeListQuery());

        if (model is not { Count: > 0 })
        {
            return NotFound("Unable to locate records the contact type list.");
        }

        return Ok(model);
    }

}
