using AdventureWorks.Application.Features.HumanResources.Queries;
using AdventureWorks.Models.Features.HumanResources;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AdventureWorks.API.Controllers.v1.Departments;

/// <summary>
/// Controller for HR department reporting: headcount, headcount summary, and employee roster by department.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[ApiExplorerSettings(GroupName = "Human Resources")]
[Produces("application/json")]
[Route("api/v{version:apiVersion}/departments", Name = "ReadDepartmentReportingControllerV1")]
[Authorize]
public sealed class ReadDepartmentReportingController : ControllerBase
{
    private readonly ILogger<ReadDepartmentReportingController> _logger;
    private readonly IMediator _mediator;

    public ReadDepartmentReportingController(
        ILogger<ReadDepartmentReportingController> logger,
        IMediator mediator)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(mediator);
        _logger = logger;
        _mediator = mediator;
    }

    /// <summary>
    /// Returns the count of active employees currently assigned to the specified department.
    /// </summary>
    /// <param name="departmentId">The unique department identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Department headcount.</returns>
    /// <response code="200">Headcount returned successfully.</response>
    /// <response code="400">Invalid department identifier.</response>
    /// <response code="401">Unauthorized - authentication required.</response>
    /// <response code="404">Department not found.</response>
    [HttpGet("{departmentId:int}/headcount", Name = "GetDepartmentHeadcount")]
    [ProducesResponseType(typeof(DepartmentHeadcountModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetHeadcountAsync(short departmentId, CancellationToken cancellationToken)
    {
        if (departmentId <= 0)
        {
            return BadRequest("The department identifier must be a positive integer.");
        }

        _logger.LogInformation("Retrieving headcount for department {DepartmentId}.", departmentId);

        var model = await _mediator.Send(
            new ReadDepartmentHeadcountQuery { DepartmentId = departmentId },
            cancellationToken);

        return Ok(model);
    }

    /// <summary>
    /// Returns all departments with their active employee counts, sorted by count descending. Departments with zero active employees are included.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Headcount summary for all departments.</returns>
    /// <response code="200">Headcount summary returned successfully.</response>
    /// <response code="401">Unauthorized - authentication required.</response>
    [HttpGet("headcount-summary", Name = "GetDepartmentHeadcountSummary")]
    [ProducesResponseType(typeof(IReadOnlyList<DepartmentHeadcountSummaryModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetHeadcountSummaryAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving department headcount summary.");

        var list = await _mediator.Send(new ReadDepartmentHeadcountSummaryQuery(), cancellationToken);

        return Ok(list);
    }

    /// <summary>
    /// Returns a paginated list of active employees currently assigned to the specified department.
    /// </summary>
    /// <param name="departmentId">The unique department identifier.</param>
    /// <param name="page">Page number (1-based). Defaults to 1.</param>
    /// <param name="pageSize">Number of records per page. Defaults to 20. Maximum 100.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Paginated employee list with X-Total-Count response header.</returns>
    /// <response code="200">Employee list returned successfully.</response>
    /// <response code="400">Invalid department identifier or pagination parameters.</response>
    /// <response code="401">Unauthorized - authentication required.</response>
    /// <response code="404">Department not found.</response>
    [HttpGet("{departmentId:int}/employees", Name = "GetDepartmentEmployees")]
    [ProducesResponseType(typeof(IReadOnlyList<EmployeeModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetEmployeesAsync(
        short departmentId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        if (departmentId <= 0)
        {
            return BadRequest("The department identifier must be a positive integer.");
        }

        if (page <= 0)
        {
            return BadRequest("Page must be greater than zero.");
        }

        if (pageSize <= 0)
        {
            return BadRequest("Page size must be greater than zero.");
        }

        if (pageSize > 100)
        {
            return BadRequest("Page size cannot exceed 100.");
        }

        _logger.LogInformation(
            "Retrieving employees for department {DepartmentId}, page {Page}, pageSize {PageSize}.",
            departmentId, page, pageSize);

        var (employees, totalCount) = await _mediator.Send(
            new ReadEmployeesByDepartmentQuery
            {
                DepartmentId = departmentId,
                Page = page,
                PageSize = pageSize
            },
            cancellationToken);

        Response.Headers["X-Total-Count"] = totalCount.ToString();
        return Ok(employees);
    }
}
