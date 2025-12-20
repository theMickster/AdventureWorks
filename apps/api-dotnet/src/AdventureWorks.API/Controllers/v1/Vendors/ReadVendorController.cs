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
}
