using AdventureWorks.Application.Features.AddressManagement.Queries;
using AdventureWorks.Models.Features.AddressManagement;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AdventureWorks.API.Controllers.v1.AddressType;

/// <summary>
/// The controller that coordinates retrieving Address Type information.
/// </summary>
/// <remarks></remarks>
[ApiController]
[Authorize]
[ApiVersion("1.0")]
[ApiExplorerSettings(GroupName = "Address Type")]
[Produces("application/json")]
[Route("api/v{version:apiVersion}/addressTypes", Name = "ReadAddressTypeControllerV1")]
public sealed class ReadAddressTypeController : ControllerBase
{
    private readonly ILogger<ReadAddressTypeController> _logger;
    private readonly IMediator _mediator;

    /// <summary>
    /// The controller that coordinates retrieving Address Type information.
    /// </summary>
    /// <remarks></remarks>
    public ReadAddressTypeController(
        ILogger<ReadAddressTypeController> logger,
        IMediator mediator
        )
    {
        ArgumentNullException.ThrowIfNull(logger, nameof(logger));
        ArgumentNullException.ThrowIfNull(mediator, nameof(mediator));
        _logger = logger;
        _mediator = mediator;
    }


    /// <summary>
    /// Retrieve a address type using its unique identifier
    /// </summary>
    /// <param name="id">the unique identifier</param>
    /// <returns></returns>
    [HttpGet("{id:int}", Name = "GetAddressTypeById")]
    [Produces(typeof(AddressTypeModel))]
    public async Task<IActionResult> GetByIdAsync(int id)
    {
        if (id <= 0)
        {
            return BadRequest("A valid address type id must be specified.");
        }

        var model = await _mediator.Send(new ReadAddressTypeQuery{ Id = id });

        return model is null ? NotFound("Unable to locate the address type.") : Ok(model);
    }

    /// <summary>
    /// Retrieve the complete address type list
    /// </summary>
    /// <returns></returns>
    [HttpGet(Name = "GetAddressTypes")]
    [Produces(typeof(AddressTypeModel))]
    public async Task<IActionResult> GetListAsync()
    {
        var model = await _mediator.Send(new ReadAddressTypeListQuery());

        if (model is not { Count: > 0 })
        {
            return NotFound("Unable to locate records the address type list.");
        }

        return Ok(model);
    }
}
