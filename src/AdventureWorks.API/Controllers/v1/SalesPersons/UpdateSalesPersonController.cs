using AdventureWorks.Application.Features.Sales.Commands;
using AdventureWorks.Application.Features.Sales.Queries;
using AdventureWorks.Models.Features.Sales;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AdventureWorks.API.Controllers.v1.SalesPersons;

/// <summary>
/// The controller that coordinates updating sales person information.
/// </summary>
[ApiController]
[Authorize]
[ApiVersion("1.0")]
[ApiExplorerSettings(GroupName = "SalesPerson")]
[Produces("application/json")]
[Route("api/v{version:apiVersion}/salespersons", Name = "UpdateSalesPersonControllerV1")]
public sealed class UpdateSalesPersonController : ControllerBase
{
    private readonly ILogger<UpdateSalesPersonController> _logger;
    private readonly IMediator _mediator;

    /// <summary>
    /// The controller that coordinates updating sales person information.
    /// </summary>
    public UpdateSalesPersonController(
        ILogger<UpdateSalesPersonController> logger,
        IMediator mediator)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(mediator);
        _logger = logger;
        _mediator = mediator;
    }

    /// <summary>
    /// Update a sales person record
    /// </summary>
    /// <param name="salesPersonId">the sales person identifier</param>
    /// <param name="inputModel">the sales person update model</param>
    /// <returns></returns>
    [HttpPut("{salesPersonId:int}")]
    [Produces(typeof(SalesPersonModel))]
    public async Task<IActionResult> PutAsync(int salesPersonId, [FromBody] SalesPersonUpdateModel? inputModel)
    {
        if (inputModel == null)
        {
            return BadRequest("The sales person input model cannot be null.");
        }

        if (salesPersonId <= 0)
        {
            return BadRequest("The sales person id must be a positive integer.");
        }

        if (salesPersonId != inputModel.Id)
        {
            return BadRequest("The sales person id parameter must match the id of the sales person update request payload.");
        }

        var cmd = new UpdateSalesPersonCommand
        {
            Model = inputModel,
            ModifiedDate = DateTime.UtcNow
        };

        await _mediator.Send(cmd);
        var model = await _mediator.Send(new ReadSalesPersonQuery { Id = salesPersonId });

        return Ok(model);
    }
}
