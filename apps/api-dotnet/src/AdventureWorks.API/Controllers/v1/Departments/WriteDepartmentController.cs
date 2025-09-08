using AdventureWorks.Application.Features.HumanResources.Commands;
using AdventureWorks.Application.Features.HumanResources.Queries;
using AdventureWorks.Models.Features.HumanResources;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AdventureWorks.API.Controllers.v1.Departments;

/// <summary>
/// The controller that coordinates creating and updating Department information.
/// </summary>
[ApiController]
[Authorize]
[ApiVersion("1.0")]
[ApiExplorerSettings(GroupName = "Human Resources")]
[Produces("application/json")]
[Route("api/v{version:apiVersion}/departments", Name = "WriteDepartmentControllerV1")]
public sealed class WriteDepartmentController : ControllerBase
{
    private readonly ILogger<WriteDepartmentController> _logger;
    private readonly IMediator _mediator;

    /// <summary>
    /// The controller that coordinates creating and updating Department information.
    /// </summary>
    public WriteDepartmentController(
        ILogger<WriteDepartmentController> logger,
        IMediator mediator)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(mediator);
        _logger = logger;
        _mediator = mediator;
    }

    /// <summary>
    /// Creates a new department.
    /// </summary>
    /// <param name="inputModel">Department data including name and group name</param>
    /// <param name="cancellationToken">Token to cancel the operation</param>
    /// <returns>The newly created department</returns>
    /// <response code="201">Department created successfully</response>
    /// <response code="400">Invalid input or duplicate department name</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="500">Internal server error</response>
    [HttpPost]
    [ProducesResponseType(typeof(DepartmentModel), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> PostAsync([FromBody] DepartmentCreateModel? inputModel, CancellationToken cancellationToken = default)
    {
        if (inputModel is null)
        {
            return BadRequest("The department input model cannot be null.");
        }

        var command = new CreateDepartmentCommand
        {
            Model = inputModel,
            ModifiedDate = DateTime.UtcNow
        };

        var newId = await _mediator.Send(command, cancellationToken);
        var model = await _mediator.Send(new ReadDepartmentQuery { Id = newId }, cancellationToken);

        return CreatedAtRoute("GetDepartmentById", new { id = newId }, model);
    }

    /// <summary>
    /// Updates an existing department.
    /// </summary>
    /// <param name="id">The department identifier</param>
    /// <param name="inputModel">Department update data</param>
    /// <param name="cancellationToken">Token to cancel the operation</param>
    /// <returns>The updated department model</returns>
    /// <response code="200">Department updated successfully</response>
    /// <response code="400">Invalid input or ID mismatch</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="404">Department not found</response>
    /// <response code="500">Internal server error</response>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(DepartmentModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> PutAsync(int id, [FromBody] DepartmentUpdateModel? inputModel, CancellationToken cancellationToken = default)
    {
        if (inputModel is null)
        {
            return BadRequest("The department input model cannot be null.");
        }

        if (id != inputModel.Id)
        {
            return BadRequest("The ID in the route must match the ID in the request body.");
        }

        var command = new UpdateDepartmentCommand
        {
            Model = inputModel,
            ModifiedDate = DateTime.UtcNow
        };

        await _mediator.Send(command, cancellationToken);

        var model = await _mediator.Send(new ReadDepartmentQuery { Id = (short)id }, cancellationToken);

        return Ok(model);
    }
}
