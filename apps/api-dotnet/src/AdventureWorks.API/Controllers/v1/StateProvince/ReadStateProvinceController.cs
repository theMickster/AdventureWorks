using AdventureWorks.Application.Features.AddressManagement.Queries;
using AdventureWorks.Models.Features.AddressManagement;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AdventureWorks.API.Controllers.v1.StateProvince;

/// <summary>
/// The controller that coordinates retrieving State Province information.
/// </summary>
/// <remarks></remarks>
[ApiController]
[ApiVersion("1.0")]
[ApiExplorerSettings(GroupName = "State Province")]
[Produces("application/json")]
[Route("api/v{version:apiVersion}/states", Name = "ReadStateProvinceControllerV1")]

public sealed class ReadStateProvinceController : ControllerBase
{
    private readonly ILogger<ReadStateProvinceController> _logger;
    private readonly IMediator _mediator;

    /// <summary>
    /// The controller that coordinates retrieving State Province information.
    /// </summary>
    public ReadStateProvinceController(
        ILogger<ReadStateProvinceController> logger,
        IMediator mediator
        )
    {
        ArgumentNullException.ThrowIfNull(logger, nameof(logger));
        ArgumentNullException.ThrowIfNull(mediator, nameof(mediator));
        _logger = logger;
        _mediator = mediator;
    }

    /// <summary>
    /// Retrieve a state province using its unique identifier
    /// </summary>
    /// <param name="id">the unique identifier</param>
    /// <returns></returns>
    [HttpGet("{id:int}", Name = "GetStateProvinceById")]
    [Produces(typeof(StateProvinceModel))]
    public async Task<IActionResult> GetByIdAsync(int id)
    {
        if (id <= 0)
        {
            return BadRequest("A valid state province id must be specified.");
        }

        var model = await _mediator.Send(new ReadStateProvinceQuery() { Id = id });

        return model is null ? NotFound("Unable to locate the state province.") : Ok(model);
    }

    /// <summary>
    /// Retrieve the complete state province list
    /// </summary>
    /// <returns></returns>
    [HttpGet(Name = "GetStateProvinces")]
    [Produces(typeof(StateProvinceModel))]
    public async Task<IActionResult> GetListAsync()
    {
        var model = await _mediator.Send(new ReadStateProvinceListQuery());

        if (model is not { Count: > 0 })
        {
            return NotFound("Unable to locate records the state province list.");
        }

        return Ok(model);
    }
}
