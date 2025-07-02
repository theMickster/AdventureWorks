using AdventureWorks.Application.Features.HumanResources.Queries;
using AdventureWorks.Models.Features.HumanResources;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AdventureWorks.API.Controllers.v1.Employee;

/// <summary>
/// Controller for retrieving employee address information.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[ApiExplorerSettings(GroupName = "Human Resources")]
[Produces("application/json")]
[Route("api/v{version:apiVersion}/employees/{employeeId:int}/addresses", Name = "ReadEmployeeAddressControllerV1")]
[Authorize]
public sealed class ReadEmployeeAddressController : ControllerBase
{
    private readonly ILogger<ReadEmployeeAddressController> _logger;
    private readonly IMediator _mediator;

    public ReadEmployeeAddressController(
        ILogger<ReadEmployeeAddressController> logger,
        IMediator mediator)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(mediator);
        _logger = logger;
        _mediator = mediator;
    }

    /// <summary>
    /// Retrieves all addresses for a specific employee.
    /// </summary>
    /// <param name="employeeId">Employee's BusinessEntityId</param>
    /// <returns>List of addresses with address type information</returns>
    /// <response code="200">Addresses retrieved successfully</response>
    /// <response code="404">Employee not found</response>
    /// <response code="500">Internal server error</response>
    [HttpGet(Name = "GetEmployeeAddresses")]
    [ProducesResponseType(typeof(IReadOnlyList<EmployeeAddressModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAllAsync(int employeeId)
    {
        _logger.LogInformation("Retrieving all addresses for employee {EmployeeId}", employeeId);

        var query = new ReadEmployeeAddressListQuery
        {
            BusinessEntityId = employeeId
        };

        var addresses = await _mediator.Send(query);

        _logger.LogInformation("Retrieved {Count} addresses for employee {EmployeeId}", addresses.Count, employeeId);

        return Ok(addresses);
    }

    /// <summary>
    /// Retrieves a specific address for an employee.
    /// </summary>
    /// <param name="employeeId">Employee's BusinessEntityId</param>
    /// <param name="addressId">Address identifier</param>
    /// <returns>Address details with address type information</returns>
    /// <response code="200">Address retrieved successfully</response>
    /// <response code="404">Employee or address not found</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("{addressId:int}", Name = "GetEmployeeAddressById")]
    [ProducesResponseType(typeof(EmployeeAddressModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetByIdAsync(int employeeId, int addressId)
    {
        _logger.LogInformation(
            "Retrieving address {AddressId} for employee {EmployeeId}",
            addressId,
            employeeId);

        var query = new ReadEmployeeAddressQuery
        {
            BusinessEntityId = employeeId,
            AddressId = addressId
        };

        var address = await _mediator.Send(query);

        if (address == null)
        {
            _logger.LogWarning(
                "Address {AddressId} not found for employee {EmployeeId}",
                addressId,
                employeeId);
            return NotFound($"Address {addressId} not found for employee {employeeId}.");
        }

        return Ok(address);
    }
}
