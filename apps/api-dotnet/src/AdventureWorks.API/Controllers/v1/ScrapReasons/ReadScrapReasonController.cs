using AdventureWorks.Application.Features.Production.Queries;
using AdventureWorks.Models.Features.Production;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AdventureWorks.API.Controllers.v1.ScrapReasons;

/// <summary>
/// The controller that coordinates retrieving scrap reason information.
/// </summary>
/// <remarks></remarks>
[ApiController]
[Authorize]
[ApiVersion("1.0")]
[ApiExplorerSettings(GroupName = "Production")]
[Produces("application/json")]
[Route("api/v{version:apiVersion}/scrap-reasons", Name = "ReadScrapReasonControllerV1")]
public sealed class ReadScrapReasonController : ControllerBase
{
    private readonly ILogger<ReadScrapReasonController> _logger;
    private readonly IMediator _mediator;

    /// <summary>
    /// The controller that coordinates retrieving scrap reason information.
    /// </summary>
    /// <remarks></remarks>
    public ReadScrapReasonController(
        ILogger<ReadScrapReasonController> logger,
        IMediator mediator)
    {
        ArgumentNullException.ThrowIfNull(logger, nameof(logger));
        ArgumentNullException.ThrowIfNull(mediator, nameof(mediator));
        _logger = logger;
        _mediator = mediator;
    }

    /// <summary>
    /// Retrieve a scrap reason using its unique identifier
    /// </summary>
    /// <param name="id">the unique scrap reason identifier</param>
    /// <param name="cancellationToken">token to cancel the operation</param>
    /// <returns></returns>
    [HttpGet("{id:int}", Name = "GetScrapReasonById")]
    [Produces(typeof(ScrapReasonModel))]
    public async Task<IActionResult> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        if (id <= 0 || id > short.MaxValue)
        {
            return BadRequest("A valid scrap reason id must be specified.");
        }

        var model = await _mediator.Send(new ReadScrapReasonQuery { Id = id }, cancellationToken);

        return model is null ? NotFound("Unable to locate the scrap reason.") : Ok(model);
    }

    /// <summary>
    /// Retrieve the complete scrap reason list
    /// </summary>
    /// <param name="cancellationToken">token to cancel the operation</param>
    /// <returns></returns>
    [HttpGet(Name = "GetScrapReasons")]
    [Produces(typeof(List<ScrapReasonModel>))]
    public async Task<IActionResult> GetListAsync(CancellationToken cancellationToken = default)
    {
        var model = await _mediator.Send(new ReadScrapReasonListQuery(), cancellationToken);

        if (model is not { Count: > 0 })
        {
            return NotFound("Unable to locate records in the scrap reason list.");
        }

        return Ok(model);
    }
}
