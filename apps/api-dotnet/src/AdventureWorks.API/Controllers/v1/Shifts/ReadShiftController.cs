using AdventureWorks.Application.Features.HumanResources.Queries;
using AdventureWorks.Models.Features.HumanResources;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AdventureWorks.API.Controllers.v1.Shifts;

/// <summary>
/// The controller that coordinates retrieving Shift information.
/// </summary>
/// <remarks></remarks>
[ApiController]
[ApiVersion("1.0")]
[ApiExplorerSettings(GroupName = "Human Resources")]
[Produces("application/json")]
[Route("api/v{version:apiVersion}/shifts", Name = "ReadShiftControllerV1")]
public sealed class ReadShiftController : ControllerBase
{
    private readonly ILogger<ReadShiftController> _logger;
    private readonly IMediator _mediator;

    /// <summary>
    /// The controller that coordinates retrieving Shift information.
    /// </summary>
    /// <remarks></remarks>
    public ReadShiftController(
        ILogger<ReadShiftController> logger,
        IMediator mediator)
    {
        ArgumentNullException.ThrowIfNull(logger, nameof(logger));
        ArgumentNullException.ThrowIfNull(mediator, nameof(mediator));
        _logger = logger;
        _mediator = mediator;
    }

    /// <summary>
    /// Retrieve a shift using its unique identifier
    /// </summary>
    /// <param name="id">the unique identifier</param>
    /// <returns></returns>
    [HttpGet("{id:int}", Name = "GetShiftById")]
    [Produces(typeof(ShiftModel))]
    public async Task<IActionResult> GetByIdAsync(byte id)
    {
        if (id <= 0)
        {
            return BadRequest("A valid shift id must be specified.");
        }

        var model = await _mediator.Send(new ReadShiftQuery { Id = id });

        return model is null ? NotFound("Unable to locate the shift.") : Ok(model);
    }

    /// <summary>
    /// Retrieve the complete shift list
    /// </summary>
    /// <returns></returns>
    [HttpGet(Name = "GetShifts")]
    [Produces(typeof(ShiftModel))]
    public async Task<IActionResult> GetListAsync()
    {
        var model = await _mediator.Send(new ReadShiftListQuery());

        if (model is not { Count: > 0 })
        {
            return NotFound("Unable to locate records in the shift list.");
        }

        return Ok(model);
    }
}
