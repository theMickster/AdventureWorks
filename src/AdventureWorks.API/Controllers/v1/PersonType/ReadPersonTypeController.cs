using AdventureWorks.Application.Interfaces.Services.PersonType;
using AdventureWorks.Domain.Models.Person;
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
[ApiExplorerSettings(GroupName = "Person Type")]
[Produces("application/json")]
[Route("api/v{version:apiVersion}/personTypes", Name = "ReadPersonTypeControllerV1")]
public class ReadPersonTypeController : ControllerBase
{
    private readonly ILogger<ReadPersonTypeController> _logger;
    private readonly IReadPersonTypeService _readPersonTypeService;

    /// <summary>
    /// The controller that coordinates retrieving Person Type information.
    /// </summary>
    /// <remarks></remarks>
    public ReadPersonTypeController(
        ILogger<ReadPersonTypeController> logger,
        IReadPersonTypeService readPersonTypeService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _readPersonTypeService = readPersonTypeService ?? throw new ArgumentNullException(nameof(readPersonTypeService));
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

        var model = await _readPersonTypeService.GetByIdAsync(id).ConfigureAwait(false);

        if (model == null)
        {
            return NotFound("Unable to locate the person type.");
        }

        return Ok(model);
    }

    /// <summary>
    /// Retrieve the complete person type list
    /// </summary>
    /// <returns></returns>
    [HttpGet(Name = "GetPersonTypes")]
    [Produces(typeof(PersonTypeModel))]
    public async Task<IActionResult> GetListAsync()
    {
        var model = await _readPersonTypeService.GetListAsync().ConfigureAwait(false);

        if (!model.Any())
        {
            return NotFound("Unable to locate records the person type list.");
        }

        return Ok(model);
    }
}
