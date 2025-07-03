using AdventureWorks.Application.Features.AddressManagement.Commands;
using AdventureWorks.Application.Features.AddressManagement.Queries;
using AdventureWorks.Models.Features.AddressManagement;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AdventureWorks.API.Controllers.v1.Address;

/// <summary>
/// The controller that coordinates creating Address information.
/// </summary>
/// <remarks></remarks>
[ApiController]
[ApiVersion("1.0")]
[ApiExplorerSettings(GroupName = "Address")]
[Produces("application/json")]
[Route("api/v{version:apiVersion}/addresses", Name = "CreateAddressControllerV1")]
public sealed class CreateAddressController : ControllerBase
{
    private readonly ILogger<CreateAddressController> _logger;
    private readonly IMediator _mediator;

    public CreateAddressController(
        ILogger<CreateAddressController> logger,
        IMediator mediator)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(mediator);
        _logger = logger;
        _mediator = mediator;
    }

    /// <summary>
    /// Creates a new address
    /// </summary>
    /// <param name="inputModel">the address to create</param>
    /// <returns></returns>
    [HttpPost]
    [Produces(typeof(AddressModel))]
    public async Task<IActionResult> PostAsync([FromBody] AddressCreateModel? inputModel)
    {
        if (inputModel == null)
        {
            return BadRequest("The address input model cannot be null.");
        }
        var cmd = new CreateAddressCommand { Model = inputModel, ModifiedDate = DateTime.UtcNow, RowGuid = Guid.NewGuid() };

        var addressId = await _mediator.Send(cmd);
        var model = await _mediator.Send(new ReadAddressQuery { Id = addressId });

        return CreatedAtRoute("GetAddressById", new { addressId = model.Id }, model);
            
    }
}