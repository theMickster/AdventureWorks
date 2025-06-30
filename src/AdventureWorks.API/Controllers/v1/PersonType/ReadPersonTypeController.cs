using AdventureWorks.Application.Features.HumanResources.Queries;
using AdventureWorks.Models.Features.HumanResources;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AdventureWorks.API.Controllers.v1.PersonType;

/// <summary>
/// The controller that coordinates retrieving Person Type information.
/// </summary>
/// <remarks></remarks>
[ApiController]
[Authorize]
[ApiVersion("1.0")]
[ApiExplorerSettings(GroupName = "Person")]
[Produces("application/json")]
[Route("api/v{version:apiVersion}/personTypes", Name = "ReadPersonTypeControllerV1")]
public class ReadPersonTypeController : ControllerBase
{
    private readonly ILogger<ReadPersonTypeController> _logger;
    private readonly IMediator _mediator;

    /// <summary>
    /// The controller that coordinates retrieving Person Type information.
    /// </summary>
    /// <remarks></remarks>
    public ReadPersonTypeController(
        ILogger<ReadPersonTypeController> logger,
        IMediator mediator)
    {
        ArgumentNullException.ThrowIfNull(logger, nameof(logger));
        ArgumentNullException.ThrowIfNull(mediator, nameof(mediator));
        _logger = logger;
        _mediator = mediator;
    }

    /// <summary>
    /// Retrieve a person type using its unique identifier
    /// </summary>
    /// <param name="id">the unique identifier</param>
    /// <returns></returns>
    [HttpGet("{id:int}", Name = "GetPersonTypeById")]
    [Produces(typeof(PersonTypeModel))]
    public async Task<IActionResult> GetByIdAsync(int id)
    {
        if (id <= 0)
        {
            return BadRequest("A valid person type id must be specified.");
        }

        var model = await _mediator.Send(new ReadPersonTypeQuery { Id = id });

        return model is null ? NotFound("Unable to locate the person type.") :  Ok(model);
    }

    /// <summary>
    /// Retrieve the complete person type list
    /// </summary>
    /// <returns></returns>
    [HttpGet(Name = "GetPersonTypes")]
    [Produces(typeof(PersonTypeModel))]
    public async Task<IActionResult> GetListAsync()
    {
        var model = await _mediator.Send(new ReadPersonTypeListQuery());

        if (model is not { Count: > 0 } )
        {
            return NotFound("Unable to locate records the person type list.");
        }

        return Ok(model);
    }
}
