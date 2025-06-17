using AdventureWorks.Application.Features.Sales.Commands;
using AdventureWorks.Application.Features.Sales.Queries;
using AdventureWorks.Models.Features.Sales;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AdventureWorks.API.Controllers.v1.Stores;

/// <summary>
/// The controller that coordinates creating store information.
/// </summary>
/// <remarks></remarks>
[ApiController]
[Authorize]
[ApiVersion("1.0")]
[ApiExplorerSettings(GroupName = "Store")]
[Produces("application/json")]
[Route("api/v{version:apiVersion}/stores", Name = "CreateStoreControllerV1")]
public sealed class CreateStoreController : ControllerBase
{
    private readonly ILogger<CreateStoreController> _logger;
    private readonly IMediator _mediator;

    /// <summary>
    /// The controller that coordinates creating store information.
    /// </summary>
    /// <remarks></remarks>
    public CreateStoreController(
        ILogger<CreateStoreController> logger,
        IMediator mediator)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(mediator);
        _logger = logger;
        _mediator = mediator;
    }

    /// <summary>
    /// Creates a new store
    /// </summary>
    /// <param name="inputModel">the store to create</param>
    /// <returns></returns>
    [HttpPost]
    [Produces(typeof(StoreModel))]
    public async Task<IActionResult> PostAsync([FromBody] StoreCreateModel? inputModel)
    {
        if (inputModel == null)
        {
            return BadRequest("The store input model cannot be null.");
        }
        var cmd = new CreateStoreCommand { Model = inputModel, ModifiedDate = DateTime.UtcNow, RowGuid = Guid.NewGuid() };

        var addressId = await _mediator.Send(cmd);
        var model = await _mediator.Send(new ReadStoreQuery { Id = addressId });

        return CreatedAtRoute("GetStoreById", new { addressId = model.Id }, model);
    }
}
