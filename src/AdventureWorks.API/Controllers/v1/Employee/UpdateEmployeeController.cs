using AdventureWorks.Application.Features.HumanResources.Commands;
using AdventureWorks.Application.Features.HumanResources.Queries;
using AdventureWorks.Models.Features.HumanResources;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;

namespace AdventureWorks.API.Controllers.v1.Employee;

/// <summary>
/// Controller for updating employee personal information.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[ApiExplorerSettings(GroupName = "Human Resources")]
[Produces("application/json")]
[Route("api/v{version:apiVersion}/employees/{id:int}", Name = "UpdateEmployeeControllerV1")]
[Authorize]
public sealed class UpdateEmployeeController : ControllerBase
{
    private readonly ILogger<UpdateEmployeeController> _logger;
    private readonly IMediator _mediator;

    public UpdateEmployeeController(
        ILogger<UpdateEmployeeController> logger,
        IMediator mediator)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(mediator);
        _logger = logger;
        _mediator = mediator;
    }

    /// <summary>
    /// Updates an existing employee's personal information and marital status.
    /// Does not update immutable fields (NationalIdNumber, LoginId, BirthDate, HireDate).
    /// Use separate endpoints for job title, PTO, and address changes.
    /// </summary>
    /// <param name="id">Employee's BusinessEntityId</param>
    /// <param name="inputModel">Employee update data</param>
    /// <returns>Updated employee model</returns>
    /// <response code="200">Employee updated successfully</response>
    /// <response code="400">Invalid input data</response>
    /// <response code="401">Unauthorized - authentication required</response>
    /// <response code="404">Employee not found</response>
    /// <response code="500">Internal server error</response>
    [HttpPut]
    [ProducesResponseType(typeof(EmployeeModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> PutAsync(int id, [FromBody] EmployeeUpdateModel? inputModel)
    {
        if (inputModel == null)
        {
            _logger.LogWarning("UpdateEmployee called with null input model for ID: {Id}", id);
            return BadRequest("The employee input model cannot be null.");
        }

        if (id != inputModel.Id)
        {
            _logger.LogWarning(
                "UpdateEmployee called with mismatched IDs. Route ID: {RouteId}, Model ID: {ModelId}",
                id,
                inputModel.Id);
            return BadRequest("The ID in the route must match the ID in the request body.");
        }

        _logger.LogInformation(
            "Updating employee: ID {Id}, Name: {FirstName} {LastName}",
            id,
            inputModel.FirstName,
            inputModel.LastName);

        var command = new UpdateEmployeeCommand
        {
            Model = inputModel,
            ModifiedDate = DateTime.UtcNow
        };

        await _mediator.Send(command);

        var updatedEmployee = await _mediator.Send(new ReadEmployeeQuery { BusinessEntityId = id });

        if (updatedEmployee == null)
        {
            _logger.LogError("Employee {Id} not found after update", id);
            return NotFound($"Employee with ID {id} not found after update.");
        }

        _logger.LogInformation("Employee {Id} updated successfully", id);

        return Ok(updatedEmployee);
    }

    /// <summary>
    /// Partially updates an employee's personal information using JSON Patch (RFC 6902).
    /// Supports selective field updates without replacing the entire employee record.
    /// </summary>
    /// <param name="id">Employee's BusinessEntityId</param>
    /// <param name="patchDocument">JSON Patch document with operations</param>
    /// <returns>Updated employee model</returns>
    /// <response code="200">Employee updated successfully</response>
    /// <response code="400">Invalid patch document or validation error</response>
    /// <response code="401">Unauthorized - authentication required</response>
    /// <response code="404">Employee not found</response>
    /// <response code="500">Internal server error</response>
    [HttpPatch]
    [Consumes("application/json-patch+json")]
    [ProducesResponseType(typeof(EmployeeModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> PatchAsync(int id, [FromBody] JsonPatchDocument<EmployeeUpdateModel>? patchDocument)
    {
        if (patchDocument == null)
        {
            _logger.LogWarning("PatchEmployee called with null patch document for ID: {Id}", id);
            return BadRequest("The patch document cannot be null.");
        }

        _logger.LogInformation(
            "Patching employee {Id} with {OperationCount} operations",
            id,
            patchDocument.Operations.Count);

        var command = new PatchEmployeeCommand
        {
            EmployeeId = id,
            PatchDocument = patchDocument,
            ModifiedDate = DateTime.UtcNow
        };

        await _mediator.Send(command);

        _logger.LogInformation("Employee {Id} patched successfully", id);

        var updatedEmployee = await _mediator.Send(new ReadEmployeeQuery { BusinessEntityId = id });

        if (updatedEmployee == null)
        {
            _logger.LogError("Employee {Id} not found after update", id);
            return NotFound($"Employee with ID {id} not found after update.");
        }

        _logger.LogInformation("Employee {Id} updated successfully", id);

        return Ok(updatedEmployee);
    }
}
