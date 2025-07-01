using AdventureWorks.Application.Features.HumanResources.Queries;
using AdventureWorks.Common.Constants;
using AdventureWorks.Common.Filtering;
using AdventureWorks.Models.Features.HumanResources;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using System.Text.Json;

namespace AdventureWorks.API.Controllers.v1.Employee;

/// <summary>
/// Controller for retrieving employee information.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[ApiExplorerSettings(GroupName = "Human Resources")]
[Produces("application/json")]
[Route("api/v{version:apiVersion}/employees", Name = "ReadEmployeeControllerV1")]
[Authorize]
public sealed class ReadEmployeeController : ControllerBase
{
    private readonly ILogger<ReadEmployeeController> _logger;
    private readonly IMediator _mediator;

    public ReadEmployeeController(
        ILogger<ReadEmployeeController> logger,
        IMediator mediator)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(mediator);
        _logger = logger;
        _mediator = mediator;
    }

    /// <summary>
    /// Retrieves an employee by their business entity ID.
    /// </summary>
    /// <param name="businessEntityId">The unique business entity identifier</param>
    /// <returns>Employee data if found</returns>
    /// <response code="200">Employee found and returned successfully</response>
    /// <response code="400">Invalid business entity ID provided</response>
    /// <response code="404">Employee not found</response>
    /// <response code="401">Unauthorized - authentication required</response>
    [HttpGet("{businessEntityId:int}", Name = "GetEmployeeById")]
    [ProducesResponseType(typeof(EmployeeModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetByIdAsync(int businessEntityId)
    {
        if (businessEntityId <= 0)
        {
            _logger.LogWarning("Invalid business entity ID provided: {BusinessEntityId}", businessEntityId);
            return BadRequest("A valid business entity ID must be specified.");
        }

        _logger.LogInformation("Retrieving employee with BusinessEntityId: {BusinessEntityId}", businessEntityId);

        var model = await _mediator.Send(new ReadEmployeeQuery { BusinessEntityId = businessEntityId });

        if (model is null)
        {
            _logger.LogWarning("Employee not found with BusinessEntityId: {BusinessEntityId}", businessEntityId);
            return NotFound("Unable to locate the employee.");
        }

        _logger.LogInformation("Successfully retrieved employee with BusinessEntityId: {BusinessEntityId}", businessEntityId);
        return Ok(model);
    }

    /// <summary>
    /// Retrieves a paginated list of employees.
    /// </summary>
    /// <param name="parameters">Pagination and sorting parameters</param>
    /// <returns>Paginated list of employees</returns>
    /// <response code="200">Employee list retrieved successfully</response>
    /// <response code="400">Invalid query parameters</response>
    /// <response code="401">Unauthorized - authentication required</response>
    [HttpGet]
    [ProducesResponseType(typeof(EmployeeSearchResultModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetEmployeeListAsync([FromQuery] EmployeeParameter parameters)
    {
        var searchResult = await _mediator.Send(new ReadEmployeeListQuery { Parameters = parameters });

        if (searchResult.Results is not (null or { Count: 0 }))
        {
            return Ok(searchResult);
        }

        var logErrorParams = new
        {
            Status = AppLoggingConstants.StatusBadRequest,
            Operation = "EmployeeListAsync",
            DateTime = DateTime.UtcNow.ToString(CultureInfo.InvariantCulture),
            Message = "Unable to locate results based upon input query parameters.",
            ErrCode = AppLoggingConstants.HttpGetRequestErrorCode,
            ServiceId = "AdventureWorksApi",
            AdditionalInfo = parameters
        };

        _logger.LogError(JsonSerializer.Serialize(logErrorParams));

        return BadRequest(logErrorParams.Message);
    }

    /// <summary>
    /// Searches employees by various criteria with pagination.
    /// </summary>
    /// <param name="parameters">Pagination and sorting parameters</param>
    /// <param name="employeeSearchModel">Search criteria</param>
    /// <returns>Paginated list of filtered employees</returns>
    /// <response code="200">Employee search completed successfully</response>
    /// <response code="400">Invalid search parameters</response>
    /// <response code="401">Unauthorized - authentication required</response>
    [HttpPost]
    [Route("search", Name = "SearchEmployeesAsync")]
    [ProducesResponseType(typeof(EmployeeSearchResultModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> SearchEmployeesAsync(
        [FromQuery] EmployeeParameter parameters,
        [FromBody] EmployeeSearchModel employeeSearchModel)
    {
        var searchResult = await _mediator.Send(new ReadEmployeeListQuery
        {
            Parameters = parameters,
            SearchModel = employeeSearchModel
        });

        if (searchResult.Results is not (null or { Count: 0 }))
        {
            return Ok(searchResult);
        }

        var logErrorParams = new
        {
            Status = AppLoggingConstants.StatusBadRequest,
            Operation = "EmployeeSearchAsync",
            DateTime = DateTime.UtcNow.ToString(CultureInfo.InvariantCulture),
            Message = "Unable to locate results based upon client input parameters.",
            ErrCode = AppLoggingConstants.HttpGetRequestErrorCode,
            ServiceId = "AdventureWorksApi",
            AdditionalInfo = parameters
        };

        _logger.LogError(JsonSerializer.Serialize(logErrorParams));

        return BadRequest(logErrorParams.Message);
    }
}
