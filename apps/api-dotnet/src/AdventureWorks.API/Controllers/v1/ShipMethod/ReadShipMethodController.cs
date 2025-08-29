using AdventureWorks.Application.Features.Sales.Queries;
using AdventureWorks.Models.Features.Sales;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AdventureWorks.API.Controllers.v1.ShipMethod;

/// <summary>
/// The controller that coordinates retrieving ship method information.
/// </summary>
/// <remarks></remarks>
[ApiController]
[Authorize]
[ApiVersion("1.0")]
[ApiExplorerSettings(GroupName = "Ship Method")]
[Produces("application/json")]
[Route("api/v{version:apiVersion}/ship-methods", Name = "ReadShipMethodControllerV1")]
public sealed class ReadShipMethodController : ControllerBase
{
    private readonly ILogger<ReadShipMethodController> _logger;
    private readonly IMediator _mediator;

    /// <summary>
    /// The controller that coordinates retrieving ship method information.
    /// </summary>
    /// <remarks></remarks>
    public ReadShipMethodController(
        ILogger<ReadShipMethodController> logger,
        IMediator mediator)
    {
        ArgumentNullException.ThrowIfNull(logger, nameof(logger));
        ArgumentNullException.ThrowIfNull(mediator, nameof(mediator));
        _logger = logger;
        _mediator = mediator;
    }

    /// <summary>
    /// Retrieve a ship method using its unique identifier
    /// </summary>
    /// <param name="id">the unique identifier</param>
    /// <param name="cancellationToken">token to cancel the operation</param>
    /// <returns></returns>
    [HttpGet("{id:int}", Name = "GetShipMethodById")]
    [Produces(typeof(ShipMethodModel))]
    public async Task<IActionResult> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        if (id <= 0)
        {
            return BadRequest("A valid ship method id must be specified.");
        }

        var model = await _mediator.Send(new ReadShipMethodQuery { Id = id }, cancellationToken);

        return model is null ? NotFound("Unable to locate the ship method.") : Ok(model);
    }

    /// <summary>
    /// Retrieve the complete ship method list
    /// </summary>
    /// <param name="cancellationToken">token to cancel the operation</param>
    /// <returns></returns>
    [HttpGet(Name = "GetShipMethods")]
    [Produces(typeof(List<ShipMethodModel>))]
    public async Task<IActionResult> GetListAsync(CancellationToken cancellationToken = default)
    {
        var model = await _mediator.Send(new ReadShipMethodListQuery(), cancellationToken);
        return Ok(model);
    }
}
