using AdventureWorks.Application.Features.Sales.Queries;
using AdventureWorks.Common.Filtering;
using AdventureWorks.Models.Features.Sales;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AdventureWorks.API.Controllers.v1.Customers;

/// <summary>
/// Controller for retrieving the paged, LTV-ranked customer list.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[ApiExplorerSettings(GroupName = "Sales")]
[Produces("application/json")]
[Route("api/v{version:apiVersion}/customers")]
[Authorize]
public sealed class ReadCustomersController : ControllerBase
{
    private readonly ILogger<ReadCustomersController> _logger;
    private readonly IMediator _mediator;

    public ReadCustomersController(
        ILogger<ReadCustomersController> logger,
        IMediator mediator)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(mediator);
        _logger = logger;
        _mediator = mediator;
    }

    /// <summary>
    /// Retrieves a paged list of customers ranked by lifetime value (LTV), highest spend first.
    /// Supports an optional case-insensitive <c>search</c> filter on display name; a customer's
    /// rank is stable regardless of whether the search filters it out of view.
    /// </summary>
    /// <param name="parameters">Paging and search query string parameters.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The paged, LTV-ranked customer search result.</returns>
    /// <response code="200">Customer list retrieved successfully.</response>
    /// <response code="500">Internal server error.</response>
    [HttpGet(Name = "GetCustomers")]
    [ProducesResponseType(typeof(CustomerSearchResultModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAsync(
        [FromQuery] CustomerParameter parameters,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Retrieving customer LTV list");

        var model = await _mediator.Send(new ReadCustomerListQuery { Parameters = parameters }, cancellationToken);

        return Ok(model);
    }

    /// <summary>
    /// Retrieves a single customer's full detail, including LTV rank and lifetime-spend metrics.
    /// </summary>
    /// <param name="customerId">The customer primary key.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The customer detail.</returns>
    /// <response code="200">Customer detail retrieved successfully.</response>
    /// <response code="400">The customer id was not a positive integer.</response>
    /// <response code="404">No customer with the given id exists.</response>
    /// <response code="500">Internal server error.</response>
    [HttpGet("{customerId:int}", Name = "GetCustomerDetail")]
    [ProducesResponseType(typeof(CustomerDetailModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetByIdAsync(int customerId, CancellationToken cancellationToken = default)
    {
        if (customerId <= 0)
        {
            return BadRequest();
        }

        // A missing customer throws KeyNotFoundException, which ExceptionHandlerMiddleware
        // translates to a structured 404 body — deliberately left to bubble up rather than checked here.
        var model = await _mediator.Send(new ReadCustomerDetailQuery { CustomerId = customerId }, cancellationToken);

        return Ok(model);
    }
}
