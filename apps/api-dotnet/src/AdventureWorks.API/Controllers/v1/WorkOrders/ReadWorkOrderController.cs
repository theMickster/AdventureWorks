using AdventureWorks.Application.Features.Production.Queries;
using AdventureWorks.Common.Filtering;
using AdventureWorks.Models.Features.Production;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AdventureWorks.API.Controllers.v1.WorkOrders;

/// <summary>
/// The controller that coordinates retrieving production work order information.
/// </summary>
[ApiController]
[Authorize]
[ApiVersion("1.0")]
[ApiExplorerSettings(GroupName = "WorkOrder")]
[Produces("application/json")]
[Route("api/v{version:apiVersion}/work-orders", Name = "ReadWorkOrderControllerV1")]
public sealed class ReadWorkOrderController : ControllerBase
{
    private readonly IMediator _mediator;

    /// <summary>
    /// The controller that coordinates retrieving production work order information.
    /// </summary>
    public ReadWorkOrderController(IMediator mediator)
    {
        ArgumentNullException.ThrowIfNull(mediator);
        _mediator = mediator;
    }

    /// <summary>
    /// Retrieve a paginated list of work orders with optional filtering.
    /// </summary>
    /// <remarks>
    /// ## Filtering
    /// - `productId`: Filter by product identifier
    /// - `startDate` and `endDate`: Filter by work order start date range (inclusive)
    /// - `hasScrapped`: Filter to work orders with scrapped units (true) or none (false)
    /// - `scrapReasonId`: Filter by scrap reason identifier
    ///
    /// ## Sorting
    /// - `orderBy`: Sort field (workOrderId, startDate, dueDate). Defaults to startDate.
    /// - `sortOrder`: Sort direction (asc/ascending or desc/descending). Defaults to descending.
    ///
    /// ## Pagination
    /// - `pageNumber`: Page number (1-based). Defaults to 1.
    /// - `pageSize`: Number of records per page (max 50). Defaults to 25.
    /// </remarks>
    /// <param name="parameters">Work order pagination and filter query parameters</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated list of work orders</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(WorkOrderSearchResultModel))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAsync(
        [FromQuery] WorkOrderParameter parameters,
        CancellationToken cancellationToken = default)
    {
        WorkOrderSearchModel? searchModel = null;
        if (parameters.ProductId.HasValue || parameters.StartDate.HasValue || parameters.EndDate.HasValue ||
            parameters.HasScrapped.HasValue || parameters.ScrapReasonId.HasValue)
        {
            searchModel = new WorkOrderSearchModel
            {
                ProductId = parameters.ProductId,
                StartDate = parameters.StartDate,
                EndDate = parameters.EndDate,
                HasScrapped = parameters.HasScrapped,
                ScrapReasonId = parameters.ScrapReasonId
            };
        }

        var query = new ReadWorkOrderListQuery
        {
            Parameters = parameters,
            SearchModel = searchModel
        };

        var searchResult = await _mediator.Send(query, cancellationToken);

        return Ok(searchResult);
    }
}
