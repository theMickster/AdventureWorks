using AdventureWorks.Application.Features.Sales.Queries;
using AdventureWorks.Models.Features.Sales;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AdventureWorks.API.Controllers.v1.SalesPersons;

/// <summary>
/// The controller that coordinates retrieving sales person information.
/// </summary>
[ApiController]
[Authorize]
[ApiVersion("1.0")]
[ApiExplorerSettings(GroupName = "SalesPerson")]
[Produces("application/json")]
[Route("api/v{version:apiVersion}/salespersons", Name = "ReadSalesPersonControllerV1")]
public sealed class ReadSalesPersonController : ControllerBase
{
    private readonly ILogger<ReadSalesPersonController> _logger;
    private readonly IMediator _mediator;

    /// <summary>
    /// The controller that coordinates retrieving sales person information.
    /// </summary>
    public ReadSalesPersonController(
        ILogger<ReadSalesPersonController> logger,
        IMediator mediator)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(mediator);
        _logger = logger;
        _mediator = mediator;
    }

    /// <summary>
    /// Retrieve a sales person using their unique identifier
    /// </summary>
    /// <param name="salesPersonId">the unique identifier</param>
    /// <returns></returns>
    [HttpGet("{salesPersonId:int}", Name = "GetSalesPersonById")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SalesPersonModel))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetByIdAsync(int salesPersonId)
    {
        if (salesPersonId <= 0)
        {
            return BadRequest("A valid sales person id must be specified.");
        }

        var model = await _mediator.Send(new ReadSalesPersonQuery { Id = salesPersonId });

        return model is null ? NotFound("Unable to locate the sales person.") : Ok(model);
    }
}
