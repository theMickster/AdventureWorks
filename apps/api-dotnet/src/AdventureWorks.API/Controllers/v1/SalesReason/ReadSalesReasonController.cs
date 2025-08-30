using AdventureWorks.Application.Features.Sales.Queries;
using AdventureWorks.Models.Features.Sales;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AdventureWorks.API.Controllers.v1.SalesReason;

/// <summary>
/// The controller that coordinates retrieving sales reason information.
/// </summary>
/// <remarks></remarks>
[ApiController]
[Authorize]
[ApiVersion("1.0")]
[ApiExplorerSettings(GroupName = "Sales Reason")]
[Produces("application/json")]
[Route("api/v{version:apiVersion}/sales-reasons", Name = "ReadSalesReasonControllerV1")]
public sealed class ReadSalesReasonController : ControllerBase
{
    private readonly ILogger<ReadSalesReasonController> _logger;
    private readonly IMediator _mediator;

    /// <summary>
    /// The controller that coordinates retrieving sales reason information.
    /// </summary>
    /// <remarks></remarks>
    public ReadSalesReasonController(
        ILogger<ReadSalesReasonController> logger,
        IMediator mediator)
    {
        ArgumentNullException.ThrowIfNull(logger, nameof(logger));
        ArgumentNullException.ThrowIfNull(mediator, nameof(mediator));
        _logger = logger;
        _mediator = mediator;
    }

    /// <summary>
    /// Retrieve a sales reason using its unique identifier
    /// </summary>
    /// <param name="id">the unique sales reason identifier</param>
    /// <param name="cancellationToken">token to cancel the operation</param>
    /// <returns></returns>
    [HttpGet("{id:int}", Name = "GetSalesReasonById")]
    [Produces(typeof(SalesReasonModel))]
    public async Task<IActionResult> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        if (id <= 0)
        {
            return BadRequest("A valid sales reason id must be specified.");
        }

        var model = await _mediator.Send(new ReadSalesReasonQuery { Id = id }, cancellationToken);

        return model is null ? NotFound("Unable to locate the sales reason.") : Ok(model);
    }

    /// <summary>
    /// Retrieve the complete sales reason list
    /// </summary>
    /// <param name="cancellationToken">token to cancel the operation</param>
    /// <returns></returns>
    [HttpGet(Name = "GetSalesReasons")]
    [Produces(typeof(List<SalesReasonModel>))]
    public async Task<IActionResult> GetListAsync(CancellationToken cancellationToken = default)
    {
        var model = await _mediator.Send(new ReadSalesReasonListQuery(), cancellationToken);

        if (model is not { Count: > 0 })
        {
            return NotFound("Unable to locate records in the sales reason list.");
        }

        return Ok(model);
    }
}
