using AdventureWorks.Application.Features.AddressManagement.Commands;
using AdventureWorks.Application.Features.AddressManagement.Queries;
using AdventureWorks.Models.Features.AddressManagement;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AdventureWorks.API.Controllers.v1.Address;

/// <summary>
/// The controller that coordinates updating Address information.
/// </summary>
/// <remarks></remarks>
[ApiController]
[ApiVersion("1.0")]
[ApiExplorerSettings(GroupName = "Address")]
[Produces("application/json")]
[Route("api/v{version:apiVersion}/addresses", Name = "UpdateAddressControllerV1")]
public sealed class UpdateAddressController : ControllerBase
{
    private readonly ILogger<UpdateAddressController> _logger;
    private readonly IMediator _mediator;

    public UpdateAddressController(
        ILogger<UpdateAddressController> logger,
        IMediator mediator)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(mediator);
        _logger = logger;
        _mediator = mediator;
    }

    /// <summary>
    /// Update an address record
    /// </summary>
    /// <param name="addressId"></param>
    /// <param name="inputModel"></param>
    /// <returns></returns>
    [HttpPut("{addressId:int}")]
    [Produces(typeof(AddressModel))]
    public async Task<IActionResult> PutAsync(int addressId, [FromBody] AddressUpdateModel? inputModel)
    {
        if (inputModel == null)
        {
            return BadRequest("The address input model cannot be null.");
        }

        if (addressId < 0)
        {
            return BadRequest("The address id must be a positive integer.");
        }

        if (addressId != inputModel.Id)
        {
            return BadRequest("The address id parameter must match the id of the address update request payload.");
        }
        var cmd = new UpdateAddressCommand { Model = inputModel, ModifiedDate = DateTime.UtcNow};
        await _mediator.Send(cmd);
        var model = await _mediator.Send(new ReadAddressQuery { Id = addressId });
        
        return Ok(model);
    }
}