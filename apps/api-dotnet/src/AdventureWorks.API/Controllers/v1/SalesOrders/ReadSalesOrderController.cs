using AdventureWorks.Application.Features.Sales.Queries;
using AdventureWorks.Common.Constants;
using AdventureWorks.Common.Filtering;
using AdventureWorks.Models.Features.Sales;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using System.Text.Json;

namespace AdventureWorks.API.Controllers.v1.SalesOrders;

/// <summary>
/// The controller that coordinates retrieving sales order information.
/// Authenticated users may read the sales order list — no additional role restriction per project auth policy.
/// </summary>
[ApiController]
[Authorize]
[ApiVersion("1.0")]
[ApiExplorerSettings(GroupName = "SalesOrder")]
[Produces("application/json")]
[Route("api/v{version:apiVersion}/sales-orders", Name = "ReadSalesOrderControllerV1")]
public sealed class ReadSalesOrderController : ControllerBase
{
    private readonly ILogger<ReadSalesOrderController> _logger;
    private readonly IMediator _mediator;

    /// <summary>
    /// The controller that coordinates retrieving sales order information.
    /// </summary>
    public ReadSalesOrderController(
        ILogger<ReadSalesOrderController> logger,
        IMediator mediator)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(mediator);
        _logger = logger;
        _mediator = mediator;
    }

    /// <summary>
    /// Retrieve a paginated list of sales orders with optional filtering.
    /// </summary>
    /// <remarks>
    /// ## Filtering
    /// - `orderDateFrom` and `orderDateTo`: Filter by order date range (inclusive)
    /// - `status`: Filter by order status (1=In process, 2=Approved, 3=Backordered, 4=Rejected, 5=Shipped, 6=Cancelled)
    /// - `salesPersonId`: Filter by sales person identifier
    /// - `territoryId`: Filter by territory identifier
    ///
    /// ## Sorting
    /// - `orderBy`: Sort field (salesOrderId, orderDate, totalDue, salesOrderNumber). Defaults to salesOrderId.
    /// - `sortOrder`: Sort direction (asc/ascending or desc/descending). Defaults to ascending.
    ///
    /// ## Pagination
    /// - `pageNumber`: Page number (1-based). Defaults to 1.
    /// - `pageSize`: Number of records per page (max 50). Defaults to 10.
    /// </remarks>
    /// <param name="parameters">Sales order pagination and filter query parameters</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated list of sales orders</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SalesOrderSearchResultModel))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAsync(
        [FromQuery] SalesOrderParameter parameters,
        CancellationToken cancellationToken = default)
    {
        // Create search model if any filter is specified
        SalesOrderSearchModel? searchModel = null;
        if (parameters.OrderDateFrom.HasValue || parameters.OrderDateTo.HasValue ||
            parameters.Status.HasValue || parameters.SalesPersonId.HasValue ||
            parameters.TerritoryId.HasValue)
        {
            searchModel = new SalesOrderSearchModel
            {
                OrderDateFrom = parameters.OrderDateFrom,
                OrderDateTo = parameters.OrderDateTo,
                Status = parameters.Status,
                SalesPersonId = parameters.SalesPersonId,
                TerritoryId = parameters.TerritoryId
            };
        }

        var query = new ReadSalesOrderListQuery
        {
            Parameters = parameters,
            SearchModel = searchModel
        };

        var searchResult = await _mediator.Send(query, cancellationToken);

        if (searchResult.Results is null or { Count: 0 })
        {
            var logParams = new
            {
                Status = AppLoggingConstants.StatusOk,
                Operation = "SalesOrderListAsync",
                DateTime = DateTime.UtcNow.ToString(CultureInfo.InvariantCulture),
                Message = "No results found based upon input query parameters.",
                ErrCode = AppLoggingConstants.HttpGetRequestErrorCode,
                ServiceId = "AdventureWorksApi"
            };

            _logger.LogInformation(JsonSerializer.Serialize(logParams));
        }

        return Ok(searchResult);
    }

    /// <summary>
    /// Retrieve the full detail of a single sales order by its identifier.
    /// </summary>
    /// <param name="salesOrderId">the sales order primary key</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The sales order detail, or 404 if not found</returns>
    [HttpGet("{salesOrderId:int}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SalesOrderDetailModel))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetDetailAsync(
        [FromRoute] int salesOrderId,
        CancellationToken cancellationToken = default)
    {
        if (salesOrderId <= 0)
        {
            return BadRequest();
        }

        var query = new ReadSalesOrderDetailQuery { SalesOrderId = salesOrderId };

        var result = await _mediator.Send(query, cancellationToken);

        if (result is null)
        {
            return NotFound();
        }

        return Ok(result);
    }

    /// <summary>
    /// Search sales orders using filter criteria supplied in the request body.
    /// Pagination and sort parameters are passed as query-string parameters; filter criteria are in the body.
    /// An empty body returns the same result as the GET list endpoint.
    /// </summary>
    /// <param name="parameters">Pagination and sort query parameters</param>
    /// <param name="searchModel">Filter criteria (AccountNumber, Status, date range, salesPersonId, territoryId)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated list of sales orders matching the search criteria</returns>
    [HttpPost("search")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SalesOrderSearchResultModel))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> SearchAsync(
        [FromQuery] SalesOrderParameter parameters,
        [FromBody] SalesOrderSearchModel? searchModel,
        CancellationToken cancellationToken = default)
    {
        var query = new ReadSalesOrderListQuery
        {
            Parameters = parameters,
            SearchModel = searchModel
        };
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Returns aggregated analytics for a filtered slice of sales orders.
    /// </summary>
    /// <remarks>
    /// POST is used because the filter is supplied in the request body.
    /// A null or empty body returns analytics for the full dataset.
    /// </remarks>
    [HttpPost("analytics")]
    [Authorize]
    [ProducesResponseType(typeof(SalesOrderAnalyticsModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAnalyticsAsync(
        [FromBody] SalesOrderSearchModel? filter,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(
            new GetSalesOrderAnalyticsQuery { Filter = filter }, cancellationToken);
        return Ok(result);
    }
}
