using AdventureWorks.Application.Features.Sales.Queries;
using AdventureWorks.Models.Features.Sales;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AdventureWorks.API.Controllers.v1.SalesTerritory;

/// <summary>
/// The controller that coordinates retrieving Sales Territory information.
/// </summary>
/// <remarks></remarks>
[ApiController]
[ApiVersion("1.0")]
[ApiExplorerSettings(GroupName = "Sales Territory")]
[Produces("application/json")]
[Route("api/v{version:apiVersion}/territories", Name = "ReadSalesTerritoryControllerV1")]
public sealed class ReadSalesTerritoryController : ControllerBase
{
    private readonly ILogger<ReadSalesTerritoryController> _logger;
    private readonly IMediator _mediator;

    /// <summary>
    /// The controller that coordinates retrieving Sales Territory information.
    /// </summary>
    /// <remarks></remarks>
    public ReadSalesTerritoryController(
        ILogger<ReadSalesTerritoryController> logger,
        IMediator mediator)
    {
        ArgumentNullException.ThrowIfNull(logger, nameof(logger));
        ArgumentNullException.ThrowIfNull(mediator, nameof(mediator));
        _logger = logger;
        _mediator = mediator;
    }

    /// <summary>
    /// Retrieve a sales territory using its unique identifier
    /// </summary>
    /// <param name="id">the unique identifier</param>
    /// <returns></returns>
    [HttpGet("{id:int}", Name = "GetSalesTerritoryById")]
    [Produces(typeof(SalesTerritoryModel))]
    public async Task<IActionResult> GetByIdAsync(int id)
    {
        if (id <= 0)
        {
            return BadRequest("A valid sales territory id must be specified.");
        }

        var model = await _mediator.Send(new ReadSalesTerritoryQuery { Id = id });

        return model is null ? NotFound("Unable to locate the sales territory.") : Ok(model);
    }

    /// <summary>
    /// Retrieve the complete sales territory list
    /// </summary>
    /// <returns></returns>
    [HttpGet(Name = "GetSalesTerritories")]
    [Produces(typeof(SalesTerritoryModel))]
    public async Task<IActionResult> GetListAsync()
    {
        var model = await _mediator.Send(new ReadSalesTerritoryListQuery());

        if (model is not { Count: > 0 })
        {
            return NotFound("Unable to locate records the sales territory list.");
        }

        return Ok(model);
    }
}
