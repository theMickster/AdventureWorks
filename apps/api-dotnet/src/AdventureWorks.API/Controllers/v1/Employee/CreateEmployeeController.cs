using AdventureWorks.Application.Features.HumanResources.Commands;
using AdventureWorks.Application.Features.HumanResources.Queries;
using AdventureWorks.Models.Features.HumanResources;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AdventureWorks.API.Controllers.v1.Employee;

/// <summary>
/// Controller for creating new employees.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[ApiExplorerSettings(GroupName = "Human Resources")]
[Produces("application/json")]
[Route("api/v{version:apiVersion}/employees", Name = "CreateEmployeeControllerV1")]
[Authorize]
public sealed class CreateEmployeeController : ControllerBase
{
    private readonly ILogger<CreateEmployeeController> _logger;
    private readonly IMediator _mediator;

    public CreateEmployeeController(
        ILogger<CreateEmployeeController> logger,
        IMediator mediator)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(mediator);
        _logger = logger;
        _mediator = mediator;
    }

    /// <summary>
    /// Creates a new employee with person data and optional contact information.
    /// </summary>
    /// <param name="inputModel">Employee creation data including person details, contact info, and address</param>
    /// <returns>The created employee model</returns>
    /// <response code="201">Employee created successfully</response>
    /// <response code="400">Invalid input data</response>
    /// <response code="401">Unauthorized - authentication required</response>
    /// <response code="500">Internal server error</response>
    [HttpPost]
    [ProducesResponseType(typeof(EmployeeModel), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> PostAsync([FromBody] EmployeeCreateModel? inputModel)
    {
        if (inputModel == null)
        {
            _logger.LogWarning("CreateEmployee called with null input model");
            return BadRequest("The employee input model cannot be null.");
        }

        _logger.LogInformation("Create new employee request received");

        var command = new CreateEmployeeCommand
        {
            Model = inputModel,
            ModifiedDate = DateTime.UtcNow,
            RowGuid = Guid.NewGuid()
        };

        var businessEntityId = await _mediator.Send(command);
        var model = await _mediator.Send(new ReadEmployeeQuery { BusinessEntityId = businessEntityId });

        _logger.LogInformation("Employee created successfully with BusinessEntityId: {BusinessEntityId}", businessEntityId);

        return CreatedAtRoute("GetEmployeeById", new { businessEntityId }, model);
    }
}
