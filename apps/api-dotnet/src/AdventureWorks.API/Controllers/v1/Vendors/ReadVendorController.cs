using AdventureWorks.Application.Features.Purchasing.Queries;
using AdventureWorks.Common.Filtering;
using AdventureWorks.Models.Features.Purchasing;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AdventureWorks.API.Controllers.v1.Vendors;

/// <summary>
/// The controller that coordinates retrieving vendor information.
/// Authenticated users may read the vendor list — no additional role restriction per project auth policy.
/// </summary>
[ApiController]
[Authorize]
[ApiVersion("1.0")]
[ApiExplorerSettings(GroupName = "Vendor")]
[Produces("application/json")]
[Route("api/v{version:apiVersion}/vendors", Name = "ReadVendorControllerV1")]
public sealed class ReadVendorController : ControllerBase
{
    private readonly IMediator _mediator;

    /// <summary>
    /// The controller that coordinates retrieving vendor information.
    /// </summary>
    public ReadVendorController(IMediator mediator)
    {
        ArgumentNullException.ThrowIfNull(mediator);
        _mediator = mediator;
    }

    /// <summary>
    /// Retrieve a paginated, risk-ranked list of vendors with optional filtering.
    /// </summary>
    /// <remarks>
    /// ## Sorting
    /// Results are always ordered by total spend descending — there is no client-facing sort parameter.
    ///
    /// ## Filtering
    /// - `creditRating`: Filter by credit rating (1=Superior, 2=Excellent, 3=Above Average, 4=Average, 5=Below Average)
    /// - `preferredVendorStatus`: Filter by preferred vendor status
    /// - `activeFlag`: Filter by active flag
    ///
    /// ## Pagination
    /// - `pageNumber`: Page number (1-based). Defaults to 1.
    /// - `pageSize`: Number of records per page (max 50). Defaults to 25.
    /// </remarks>
    /// <param name="parameters">Vendor pagination and filter query parameters</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated, risk-ranked list of vendors</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(VendorSearchResultModel))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAsync(
        [FromQuery] VendorParameter parameters,
        CancellationToken cancellationToken = default)
    {
        var query = new ReadVendorListQuery { Parameters = parameters };

        var searchResult = await _mediator.Send(query, cancellationToken);

        return Ok(searchResult);
    }

    /// <summary>
    /// Retrieve a single vendor's profile and spend metrics.
    /// </summary>
    /// <param name="vendorId">the vendor primary key</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The vendor detail</returns>
    [HttpGet("{vendorId:int}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(VendorDetailModel))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetVendorDetailAsync(
        [FromRoute] int vendorId,
        CancellationToken cancellationToken = default)
    {
        if (vendorId <= 0)
        {
            return BadRequest();
        }

        // A missing vendor throws KeyNotFoundException, which ExceptionHandlerMiddleware translates
        // to a structured 404 body — deliberately left to bubble up rather than checked here.
        var result = await _mediator.Send(new ReadVendorDetailQuery { VendorId = vendorId }, cancellationToken);

        return Ok(result);
    }

    /// <summary>
    /// Retrieve a paginated, filterable page of a single vendor's purchase order history.
    /// </summary>
    /// <remarks>
    /// ## Filtering
    /// - `status`: Filter by purchase order status (1=Pending, 2=Approved, 3=Rejected, 4=Complete)
    /// - `startDate` and `endDate`: Filter by order date range (inclusive)
    ///
    /// ## Sorting
    /// Results are always ordered by order date descending — there is no client-facing sort parameter.
    ///
    /// ## Pagination
    /// - `pageNumber`: Page number (1-based). Defaults to 1.
    /// - `pageSize`: Number of records per page (max 50). Defaults to 25.
    /// </remarks>
    /// <param name="vendorId">the vendor primary key</param>
    /// <param name="parameters">Purchase order pagination and filter query parameters</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated purchase order history for the vendor</returns>
    [HttpGet("{vendorId:int}/purchase-orders")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PurchaseOrderSearchResultModel))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetVendorPurchaseOrdersAsync(
        [FromRoute] int vendorId,
        [FromQuery] VendorPurchaseOrderParameter parameters,
        CancellationToken cancellationToken = default)
    {
        if (vendorId <= 0)
        {
            return BadRequest();
        }

        var query = new ReadVendorPurchaseOrdersQuery { VendorId = vendorId, Parameters = parameters };

        // A missing vendor throws KeyNotFoundException, which ExceptionHandlerMiddleware translates
        // to a structured 404 body — deliberately left to bubble up rather than checked here.
        var searchResult = await _mediator.Send(query, cancellationToken);

        return Ok(searchResult);
    }
}
