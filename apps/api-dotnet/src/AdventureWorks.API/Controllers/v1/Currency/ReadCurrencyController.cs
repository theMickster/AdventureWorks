using AdventureWorks.Application.Features.Sales.Queries;
using AdventureWorks.Models.Features.Sales;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AdventureWorks.API.Controllers.v1.Currency;

/// <summary>
/// The controller that coordinates retrieving currency information.
/// </summary>
/// <remarks></remarks>
[ApiController]
[Authorize]
[ApiVersion("1.0")]
[ApiExplorerSettings(GroupName = "Currency")]
[Produces("application/json")]
[Route("api/v{version:apiVersion}/currencies", Name = "ReadCurrencyControllerV1")]
public sealed class ReadCurrencyController : ControllerBase
{
    private readonly ILogger<ReadCurrencyController> _logger;
    private readonly IMediator _mediator;

    /// <summary>
    /// The controller that coordinates retrieving currency information.
    /// </summary>
    /// <remarks></remarks>
    public ReadCurrencyController(
        ILogger<ReadCurrencyController> logger,
        IMediator mediator)
    {
        ArgumentNullException.ThrowIfNull(logger, nameof(logger));
        ArgumentNullException.ThrowIfNull(mediator, nameof(mediator));
        _logger = logger;
        _mediator = mediator;
    }

    /// <summary>
    /// Retrieve a currency using its unique code
    /// </summary>
    /// <param name="code">the unique currency code</param>
    /// <param name="cancellationToken">token to cancel the operation</param>
    /// <returns></returns>
    [HttpGet("{code}", Name = "GetCurrencyByCode")]
    [Produces(typeof(CurrencyModel))]
    public async Task<IActionResult> GetByIdAsync(string code, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return BadRequest("A valid currency code must be specified.");
        }

        var model = await _mediator.Send(new ReadCurrencyQuery { Code = code }, cancellationToken);

        return model is null ? NotFound("Unable to locate the currency.") : Ok(model);
    }

    /// <summary>
    /// Retrieve the complete currency list
    /// </summary>
    /// <param name="cancellationToken">token to cancel the operation</param>
    /// <returns></returns>
    [HttpGet(Name = "GetCurrencies")]
    [Produces(typeof(List<CurrencyModel>))]
    public async Task<IActionResult> GetListAsync(CancellationToken cancellationToken = default)
    {
        var model = await _mediator.Send(new ReadCurrencyListQuery(), cancellationToken);

        if (model is not { Count: > 0 })
        {
            return NotFound("Unable to locate records in the currency list.");
        }

        return Ok(model);
    }
}
