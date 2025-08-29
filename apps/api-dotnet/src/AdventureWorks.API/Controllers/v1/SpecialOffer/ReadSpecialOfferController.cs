using AdventureWorks.Application.Features.Sales.Queries;
using AdventureWorks.Models.Features.Sales;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AdventureWorks.API.Controllers.v1.SpecialOffer;

/// <summary>
/// The controller that coordinates retrieving special offer information.
/// </summary>
/// <remarks></remarks>
[ApiController]
[Authorize]
[ApiVersion("1.0")]
[ApiExplorerSettings(GroupName = "Special Offer")]
[Produces("application/json")]
[Route("api/v{version:apiVersion}/special-offers", Name = "ReadSpecialOfferControllerV1")]
public sealed class ReadSpecialOfferController : ControllerBase
{
    private readonly ILogger<ReadSpecialOfferController> _logger;
    private readonly IMediator _mediator;

    /// <summary>
    /// The controller that coordinates retrieving special offer information.
    /// </summary>
    /// <remarks></remarks>
    public ReadSpecialOfferController(
        ILogger<ReadSpecialOfferController> logger,
        IMediator mediator)
    {
        ArgumentNullException.ThrowIfNull(logger, nameof(logger));
        ArgumentNullException.ThrowIfNull(mediator, nameof(mediator));
        _logger = logger;
        _mediator = mediator;
    }

    /// <summary>
    /// Retrieve a special offer using its unique identifier
    /// </summary>
    /// <param name="id">the unique special offer identifier</param>
    /// <param name="cancellationToken">token to cancel the operation</param>
    /// <returns></returns>
    [HttpGet("{id:int}", Name = "GetSpecialOfferById")]
    [Produces(typeof(SpecialOfferModel))]
    public async Task<IActionResult> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        if (id <= 0)
        {
            return BadRequest("A valid special offer id must be specified.");
        }

        var model = await _mediator.Send(new ReadSpecialOfferQuery { Id = id }, cancellationToken);

        return model is null ? NotFound("Unable to locate the special offer.") : Ok(model);
    }

    /// <summary>
    /// Retrieve the complete special offer list
    /// </summary>
    /// <param name="cancellationToken">token to cancel the operation</param>
    /// <returns></returns>
    [HttpGet(Name = "GetSpecialOffers")]
    [Produces(typeof(List<SpecialOfferModel>))]
    public async Task<IActionResult> GetListAsync(CancellationToken cancellationToken = default)
    {
        var model = await _mediator.Send(new ReadSpecialOfferListQuery(), cancellationToken);
        return Ok(model);
    }
}
