using AdventureWorks.Application.Interfaces.Services.StateProvince;
using AdventureWorks.Domain.Models;
using Asp.Versioning;
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
    private readonly IReadStateProvinceService _readStateProvinceService;

    /// <summary>
    /// The controller that coordinates retrieving State Province information.
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="readStateProvinceService"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public ReadStateProvinceController(
        ILogger<ReadStateProvinceController> logger,
        IReadStateProvinceService readStateProvinceService
        )
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _readStateProvinceService = readStateProvinceService ?? throw new ArgumentNullException(nameof(readStateProvinceService));
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

        var model = await _readStateProvinceService.GetByIdAsync(id).ConfigureAwait(false);

        if (model == null)
        {
            return NotFound("Unable to locate the state province.");
        }

        return Ok(model);
    }

    /// <summary>
    /// Retrieve the complete state province list
    /// </summary>
    /// <returns></returns>
    [HttpGet(Name = "GetStateProvinces")]
    [Produces(typeof(StateProvinceModel))]
    public async Task<IActionResult> GetListAsync()
    {
        var model = await _readStateProvinceService.GetListAsync().ConfigureAwait(false);

        if (!model.Any())
        {
            return NotFound("Unable to locate records the state province list.");
        }

        return Ok(model);
    }
}
