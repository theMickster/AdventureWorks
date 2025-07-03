using AdventureWorks.Application.Features.AddressManagement.Queries;
using AdventureWorks.Models.Features.AddressManagement;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AdventureWorks.API.Controllers.v1.Address;

/// <summary>
/// The controller that coordinates retrieving Address information.
/// </summary>
/// <remarks></remarks>
[ApiController]
[ApiVersion("1.0")]
[ApiExplorerSettings(GroupName = "Address")]
[Produces("application/json")]
[Route("api/v{version:apiVersion}/addresses", Name = "ReadAddressControllerV1")]
public sealed class ReadAddressController : ControllerBase
{
    private readonly ILogger<ReadAddressController> _logger;
    private readonly IMediator _mediator;

    /// <summary>
    /// The controller that coordinates retrieving Address information.
    /// </summary>
    /// <remarks></remarks>
    public ReadAddressController( 
        ILogger<ReadAddressController> logger,
        IMediator mediator)
    {
        ArgumentNullException.ThrowIfNull(logger, nameof(logger));
        ArgumentNullException.ThrowIfNull(mediator, nameof(mediator));
        _logger = logger;
        _mediator = mediator;
    }

    /// <summary>
    /// Retrieve an address using its unique identifier
    /// </summary>
    /// <param name="addressId">the unique identifier</param>
    /// <returns></returns>
    [HttpGet("{addressId:int}", Name="GetAddressById")]
    [Produces(typeof(AddressModel))]
    public async Task<IActionResult> GetByIdAsync(int addressId)
    {
        if (addressId <= 0)
        {
            return BadRequest("A valid address id must be specified.");
        }

        var model = await _mediator.Send(new ReadAddressQuery{Id = addressId });

        return model is null ? NotFound("Unable to locate the Address.") : Ok(model);
    }
}
