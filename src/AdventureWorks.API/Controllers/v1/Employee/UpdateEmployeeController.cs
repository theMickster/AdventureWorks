using AdventureWorks.Application.Features.HumanResources.Commands;
using AdventureWorks.Models.Features.HumanResources;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AdventureWorks.API.Controllers.v1.Employee;

/// <summary>
/// Controller for updating employee personal information.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[ApiExplorerSettings(GroupName = "Human Resources")]
[Produces("application/json")]
[Route("api/v{version:apiVersion}/employees", Name = "UpdateEmployeeControllerV1")]
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
    /// <returns>NoContent on success</returns>
    /// <response code="204">Employee updated successfully</response>
    /// <response code="400">Invalid input data</response>
    /// <response code="401">Unauthorized - authentication required</response>
    /// <response code="404">Employee not found</response>
    /// <response code="500">Internal server error</response>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
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

        try
        {
            var command = new UpdateEmployeeCommand
            {
                Model = inputModel,
                ModifiedDate = DateTime.UtcNow
            };

            await _mediator.Send(command);

            _logger.LogInformation("Employee {Id} updated successfully", id);

            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Employee {Id} not found", id);
            return NotFound($"Employee with ID {id} not found.");
        }
    }
}
