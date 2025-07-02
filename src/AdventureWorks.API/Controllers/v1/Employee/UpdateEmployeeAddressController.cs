using AdventureWorks.Application.Features.HumanResources.Commands;
using AdventureWorks.Models.Features.HumanResources;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;

namespace AdventureWorks.API.Controllers.v1.Employee;

/// <summary>
/// Controller for updating employee address information.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[ApiExplorerSettings(GroupName = "Human Resources")]
[Produces("application/json")]
[Route("api/v{version:apiVersion}/employees/{employeeId:int}/addresses/{addressId:int}", Name = "UpdateEmployeeAddressControllerV1")]
[Authorize]
public sealed class UpdateEmployeeAddressController : ControllerBase
{
    private readonly ILogger<UpdateEmployeeAddressController> _logger;
    private readonly IMediator _mediator;

    public UpdateEmployeeAddressController(
        ILogger<UpdateEmployeeAddressController> logger,
        IMediator mediator)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(mediator);
        _logger = logger;
        _mediator = mediator;
    }

    /// <summary>
    /// Updates an employee's address information (full replacement).
    /// Does not update the AddressType in BusinessEntityAddress junction.
    /// </summary>
    /// <param name="employeeId">Employee's BusinessEntityId</param>
    /// <param name="addressId">Address identifier</param>
    /// <param name="inputModel">Address update data</param>
    /// <returns>NoContent on success</returns>
    /// <response code="204">Address updated successfully</response>
    /// <response code="400">Invalid input data</response>
    /// <response code="401">Unauthorized - authentication required</response>
    /// <response code="404">Employee or address not found</response>
    /// <response code="500">Internal server error</response>
    [HttpPut("")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> PutAsync(
        int employeeId,
        int addressId,
        [FromBody] EmployeeAddressUpdateModel? inputModel)
    {
        if (inputModel == null)
        {
            _logger.LogWarning(
                "UpdateEmployeeAddress called with null input model for employee {EmployeeId}, address {AddressId}",
                employeeId,
                addressId);
            return BadRequest("The address input model cannot be null.");
        }

        if (addressId != inputModel.AddressId)
        {
            _logger.LogWarning(
                "UpdateEmployeeAddress called with mismatched IDs. Route: {RouteId}, Model: {ModelId}",
                addressId,
                inputModel.AddressId);
            return BadRequest("The address ID in the route must match the ID in the request body.");
        }

        _logger.LogInformation(
            "Updating address {AddressId} for employee {EmployeeId}",
            addressId,
            employeeId);

        try
        {
            var command = new UpdateEmployeeAddressCommand
            {
                BusinessEntityId = employeeId,
                Model = inputModel,
                ModifiedDate = DateTime.UtcNow
            };

            await _mediator.Send(command);

            _logger.LogInformation(
                "Address {AddressId} updated successfully for employee {EmployeeId}",
                addressId,
                employeeId);

            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Address {AddressId} or employee {EmployeeId} not found", addressId, employeeId);
            return NotFound(ex.Message);
        }
    }

    /// <summary>
    /// Partially updates an employee's address using JSON Patch (RFC 6902).
    /// Supports selective field updates without replacing the entire address.
    /// Does not update the AddressType in BusinessEntityAddress junction.
    /// </summary>
    /// <param name="employeeId">Employee's BusinessEntityId</param>
    /// <param name="addressId">Address identifier</param>
    /// <param name="patchDocument">JSON Patch document with operations</param>
    /// <returns>NoContent on success</returns>
    /// <response code="204">Address patched successfully</response>
    /// <response code="400">Invalid patch document or validation error</response>
    /// <response code="401">Unauthorized - authentication required</response>
    /// <response code="404">Employee or address not found</response>
    /// <response code="500">Internal server error</response>
    [HttpPatch("")]
    [Consumes("application/json-patch+json")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> PatchAsync(
        int employeeId,
        int addressId,
        [FromBody] JsonPatchDocument<EmployeeAddressUpdateModel>? patchDocument)
    {
        if (patchDocument == null)
        {
            _logger.LogWarning(
                "PatchEmployeeAddress called with null patch document for employee {EmployeeId}, address {AddressId}",
                employeeId,
                addressId);
            return BadRequest("The patch document cannot be null.");
        }

        _logger.LogInformation(
            "Patching address {AddressId} for employee {EmployeeId} with {OperationCount} operations",
            addressId,
            employeeId,
            patchDocument.Operations.Count);

        try
        {
            var command = new PatchEmployeeAddressCommand
            {
                BusinessEntityId = employeeId,
                AddressId = addressId,
                PatchDocument = patchDocument,
                ModifiedDate = DateTime.UtcNow
            };

            await _mediator.Send(command);

            _logger.LogInformation(
                "Address {AddressId} patched successfully for employee {EmployeeId}",
                addressId,
                employeeId);

            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Address {AddressId} or employee {EmployeeId} not found", addressId, employeeId);
            return NotFound(ex.Message);
        }
    }
}
