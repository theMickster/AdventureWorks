using AdventureWorks.Application.Interfaces.Services.SalesTerritory;
using AdventureWorks.Domain.Models;
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
    private readonly IReadSalesTerritoryService _readSalesTerritoryService;

    /// <summary>
    /// The controller that coordinates retrieving Sales Territory information.
    /// </summary>
    /// <remarks></remarks>
    public ReadSalesTerritoryController(
        ILogger<ReadSalesTerritoryController> logger,
        IReadSalesTerritoryService readSalesTerritoryService
    )
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _readSalesTerritoryService = readSalesTerritoryService ?? throw new ArgumentNullException(nameof(readSalesTerritoryService));
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

        var model = await _readSalesTerritoryService.GetByIdAsync(id).ConfigureAwait(false);

        if (model == null)
        {
            return NotFound("Unable to locate the sales territory.");
        }

        return Ok(model);
    }

    /// <summary>
    /// Retrieve the complete sales territory list
    /// </summary>
    /// <returns></returns>
    [HttpGet(Name = "GetSalesTerritories")]
    [Produces(typeof(SalesTerritoryModel))]
    public async Task<IActionResult> GetListAsync()
    {
        var model = await _readSalesTerritoryService.GetListAsync().ConfigureAwait(false);

        if (!model.Any())
        {
            return NotFound("Unable to locate records the sales territory list.");
        }

        return Ok(model);
    }
}
