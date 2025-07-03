using AdventureWorks.Application.Features.Sales.Commands;
using AdventureWorks.Application.Features.Sales.Queries;
using AdventureWorks.Models.Features.Sales;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AdventureWorks.API.Controllers.v1.Stores;

/// <summary>
/// The controller that coordinates updating store information.
/// </summary>
/// <remarks></remarks>
[ApiController]
[Authorize]
[ApiVersion("1.0")]
[ApiExplorerSettings(GroupName = "Store")]
[Produces("application/json")]
[Route("api/v{version:apiVersion}/stores", Name = "UpdateStoreControllerV1")]
public sealed class UpdateStoreController : ControllerBase
{
    private readonly ILogger<UpdateStoreController> _logger;
    private readonly IMediator _mediator;

    /// <summary>
    /// The controller that coordinates updating store information.
    /// </summary>
    /// <remarks></remarks>
    public UpdateStoreController(
        ILogger<UpdateStoreController> logger,
        IMediator mediator)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(mediator);
        _logger = logger;
        _mediator = mediator;
    }

    /// <summary>
    /// Update a store record
    /// </summary>
    /// <param name="storeId"></param>
    /// <param name="inputModel"></param>
    /// <returns></returns>
    [HttpPut("{storeId:int}")]
    [Produces(typeof(StoreModel))]
    public async Task<IActionResult> PutAsync(int storeId, [FromBody] StoreUpdateModel? inputModel)
    {
        if (inputModel == null)
        {
            return BadRequest("The store input model cannot be null.");
        }

        if (storeId < 0)
        {
            return BadRequest("The store id must be a positive integer.");
        }

        if (storeId != inputModel.Id)
        {
            return BadRequest("The store id parameter must match the id of the address update request payload.");
        }
        var cmd = new UpdateStoreCommand { Model = inputModel, ModifiedDate = DateTime.UtcNow };
        await _mediator.Send(cmd);
        var model = await _mediator.Send(new ReadStoreQuery { Id = storeId });

        return Ok(model);
    }
}
