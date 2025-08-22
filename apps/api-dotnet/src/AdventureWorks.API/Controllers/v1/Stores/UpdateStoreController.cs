using AdventureWorks.Application.Features.Sales.Commands;
using AdventureWorks.Application.Features.Sales.Queries;
using AdventureWorks.Models.Features.Sales;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;

namespace AdventureWorks.API.Controllers.v1.Stores;

/// <summary>
/// The controller that coordinates updating store information.
/// </summary>
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
    /// Fully replace a store record.
    /// </summary>
    /// <param name="storeId">The unique store identifier.</param>
    /// <param name="inputModel">The store update payload.</param>
    /// <returns>The updated store.</returns>
    /// <response code="200">Store updated successfully</response>
    /// <response code="400">Invalid store ID or mismatched payload</response>
    /// <response code="404">Store not found</response>
    [HttpPut("{storeId:int}")]
    [Produces(typeof(StoreModel))]
    public async Task<IActionResult> PutAsync(int storeId, [FromBody] StoreUpdateModel? inputModel)
    {
        if (inputModel == null)
        {
            return BadRequest("The store input model cannot be null.");
        }

        if (storeId <= 0)
        {
            return BadRequest("The store id must be a positive integer.");
        }

        if (storeId != inputModel.Id)
        {
            return BadRequest("The store id parameter must match the id of the store update request payload.");
        }
        var cmd = new UpdateStoreCommand { Model = inputModel, ModifiedDate = DateTime.UtcNow };
        await _mediator.Send(cmd);
        var model = await _mediator.Send(new ReadStoreQuery { Id = storeId });

        return model is null ? NotFound("Unable to locate the store.") : Ok(model);
    }

    /// <summary>
    /// Partially update a store record using JSON Patch (RFC 6902)
    /// </summary>
    /// <param name="storeId">the unique store identifier</param>
    /// <param name="patchDocument">the JSON Patch document</param>
    /// <returns>the updated store</returns>
    /// <response code="200">Store updated successfully</response>
    /// <response code="400">Invalid patch document or store id</response>
    /// <response code="404">Store not found</response>
    [HttpPatch("{storeId:int}")]
    [Consumes("application/json-patch+json")]
    [Produces(typeof(StoreModel))]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(StoreModel))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> PatchAsync(int storeId, [FromBody] JsonPatchDocument<StoreUpdateModel>? patchDocument)
    {
        if (patchDocument == null)
        {
            return BadRequest("The patch document cannot be null.");
        }

        if (storeId <= 0)
        {
            return BadRequest("A valid store id must be specified.");
        }

        var cmd = new PatchStoreCommand
        {
            StoreId = storeId,
            PatchDocument = patchDocument,
            ModifiedDate = DateTime.UtcNow
        };

        await _mediator.Send(cmd);

        var model = await _mediator.Send(new ReadStoreQuery { Id = storeId });

        return model is null ? NotFound("Unable to locate the store.") : Ok(model);
    }
}
