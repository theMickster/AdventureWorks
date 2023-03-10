using AdventureWorks.Application.Interfaces.Services.ContactType;
using AdventureWorks.Domain.Models.Person;
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
    private readonly IReadContactTypeService _readContactTypeService;

    /// <summary>
    /// The controller that coordinates retrieving Contact Type information.
    /// </summary>
    /// <remarks></remarks>
    public ReadContactTypeController(
        ILogger<ReadContactTypeController> logger,
        IReadContactTypeService readContactTypeService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _readContactTypeService = readContactTypeService ?? throw new ArgumentNullException(nameof(readContactTypeService));
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

        var model = await _readContactTypeService.GetByIdAsync(id).ConfigureAwait(false);

        if (model == null)
        {
            return NotFound("Unable to locate the contact type.");
        }

        return Ok(model);
    }

    /// <summary>
    /// Retrieve the complete contact type list
    /// </summary>
    /// <returns></returns>
    [HttpGet(Name = "GetContactTypes")]
    [Produces(typeof(ContactTypeModel))]
    public async Task<IActionResult> GetListAsync()
    {
        var model = await _readContactTypeService.GetListAsync().ConfigureAwait(false);

        if (!model.Any())
        {
            return NotFound("Unable to locate records the contact type list.");
        }

        return Ok(model);
    }

}
