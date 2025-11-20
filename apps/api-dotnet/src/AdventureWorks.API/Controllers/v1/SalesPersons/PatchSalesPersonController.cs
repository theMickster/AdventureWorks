using AdventureWorks.Application.Features.Sales.Commands;
using AdventureWorks.Application.Features.Sales.Queries;
using AdventureWorks.Models.Features.Sales;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AdventureWorks.API.Controllers.v1.SalesPersons;

/// <summary>
/// The controller that coordinates partial updates to sales person sales configuration.
/// </summary>
[ApiController]
[Authorize]
[ApiVersion("1.0")]
[ApiExplorerSettings(GroupName = "SalesPerson")]
[Produces("application/json")]
[Route("api/v{version:apiVersion}/salespersons", Name = "PatchSalesPersonControllerV1")]
public sealed class PatchSalesPersonController : ControllerBase
{
    private readonly ILogger<PatchSalesPersonController> _logger;
    private readonly IMediator _mediator;

    /// <summary>
    /// The controller that coordinates partial updates to sales person sales configuration.
    /// </summary>
    public PatchSalesPersonController(
        ILogger<PatchSalesPersonController> logger,
        IMediator mediator)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(mediator);
        _logger = logger;
        _mediator = mediator;
    }

    /// <summary>
    /// Update the sales configuration (territory, quota, bonus, commission) for a sales person.
    /// </summary>
    /// <param name="salesPersonId">The unique sales person identifier.</param>
    /// <param name="inputModel">The sales config update payload.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The updated sales person.</returns>
    /// <response code="200">Sales config updated successfully</response>
    /// <response code="400">Invalid sales person ID or mismatched payload</response>
    /// <response code="404">Sales person not found</response>
    [HttpPatch("{salesPersonId:int}/sales-config")]
    [Produces(typeof(SalesPersonModel))]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SalesPersonModel))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> PatchAsync(int salesPersonId, [FromBody] SalesPersonSalesConfigUpdateModel? inputModel, CancellationToken cancellationToken)
    {
        if (inputModel == null)
        {
            return BadRequest("The sales person sales config input model cannot be null.");
        }

        if (salesPersonId <= 0)
        {
            return BadRequest("The sales person id must be a positive integer.");
        }

        if (salesPersonId != inputModel.Id)
        {
            return BadRequest("The sales person id parameter must match the id of the sales person sales config update request payload.");
        }

        var cmd = new UpdateSalesPersonSalesConfigCommand
        {
            Model = inputModel,
            ModifiedDate = DateTime.UtcNow,
            UserName = HttpContext.User.Identity?.Name ?? "unknown"
        };

        await _mediator.Send(cmd, cancellationToken);
        var model = await _mediator.Send(new ReadSalesPersonQuery { Id = salesPersonId }, cancellationToken);

        return model is null ? NotFound("Unable to locate the sales person.") : Ok(model);
    }
}
