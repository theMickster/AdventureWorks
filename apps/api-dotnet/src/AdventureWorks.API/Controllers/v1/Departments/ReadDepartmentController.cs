using AdventureWorks.Application.Features.HumanResources.Queries;
using AdventureWorks.Models.Features.HumanResources;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AdventureWorks.API.Controllers.v1.Departments;

/// <summary>
/// The controller that coordinates retrieving Department information.
/// </summary>
/// <remarks></remarks>
[ApiController]
[ApiVersion("1.0")]
[ApiExplorerSettings(GroupName = "Human Resources")]
[Produces("application/json")]
[Route("api/v{version:apiVersion}/departments", Name = "ReadDepartmentControllerV1")]
public sealed class ReadDepartmentController : ControllerBase
{
    private readonly ILogger<ReadDepartmentController> _logger;
    private readonly IMediator _mediator;

    /// <summary>
    /// The controller that coordinates retrieving Department information.
    /// </summary>
    /// <remarks></remarks>
    public ReadDepartmentController(
        ILogger<ReadDepartmentController> logger,
        IMediator mediator)
    {
        ArgumentNullException.ThrowIfNull(logger, nameof(logger));
        ArgumentNullException.ThrowIfNull(mediator, nameof(mediator));
        _logger = logger;
        _mediator = mediator;
    }

    /// <summary>
    /// Retrieve a department using its unique identifier
    /// </summary>
    /// <param name="id">the unique identifier</param>
    /// <returns></returns>
    [HttpGet("{id:int}", Name = "GetDepartmentById")]
    [Produces(typeof(DepartmentModel))]
    public async Task<IActionResult> GetByIdAsync(short id)
    {
        if (id <= 0)
        {
            return BadRequest("A valid department id must be specified.");
        }

        var model = await _mediator.Send(new ReadDepartmentQuery { Id = id });

        return model is null ? NotFound("Unable to locate the department.") : Ok(model);
    }

    /// <summary>
    /// Retrieve the complete department list
    /// </summary>
    /// <returns></returns>
    [HttpGet(Name = "GetDepartments")]
    [Produces(typeof(DepartmentModel))]
    public async Task<IActionResult> GetListAsync()
    {
        var model = await _mediator.Send(new ReadDepartmentListQuery());

        if (model is not { Count: > 0 })
        {
            return NotFound("Unable to locate records in the department list.");
        }

        return Ok(model);
    }
}
