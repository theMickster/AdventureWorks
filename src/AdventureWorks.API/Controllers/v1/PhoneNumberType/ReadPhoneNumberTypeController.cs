using AdventureWorks.Application.Features.Person.Queries;
using AdventureWorks.Models.Features.Person;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AdventureWorks.API.Controllers.v1.PhoneNumberType;

/// <summary>
/// The controller that coordinates retrieving PhoneNumberType information.
/// </summary>
/// <remarks></remarks>
[ApiController]
[ApiVersion("1.0")]
[ApiExplorerSettings(GroupName = "Person")]
[Produces("application/json")]
[Route("api/v{version:apiVersion}/phoneNumberTypes", Name = "ReadPhoneNumberTypeControllerV1")]
public sealed class ReadPhoneNumberTypeController : ControllerBase
{
    private readonly ILogger<ReadPhoneNumberTypeController> _logger;
    private readonly IMediator _mediator;

    /// <summary>
    /// The controller that coordinates retrieving PhoneNumberType information.
    /// </summary>
    /// <remarks></remarks>
    public ReadPhoneNumberTypeController(
        ILogger<ReadPhoneNumberTypeController> logger,
        IMediator mediator)
    {
        ArgumentNullException.ThrowIfNull(logger, nameof(logger));
        ArgumentNullException.ThrowIfNull(mediator, nameof(mediator));
        _logger = logger;
        _mediator = mediator;
    }

    /// <summary>
    /// Retrieve a phone number type using its unique identifier
    /// </summary>
    /// <param name="id">the unique identifier</param>
    /// <returns></returns>
    [HttpGet("{id:int}", Name = "GetPhoneNumberTypeById")]
    [Produces(typeof(PhoneNumberTypeModel))]
    public async Task<IActionResult> GetByIdAsync(int id)
    {
        if (id <= 0)
        {
            return BadRequest("A valid phone number type id must be specified.");
        }

        var model = await _mediator.Send(new ReadPhoneNumberTypeQuery { Id = id });

        return model is null ? NotFound("Unable to locate the phone number type.") : Ok(model);
    }

    /// <summary>
    /// Retrieve the complete phone number type list
    /// </summary>
    /// <returns></returns>
    [HttpGet(Name = "GetPhoneNumberTypes")]
    [Produces(typeof(PhoneNumberTypeModel))]
    public async Task<IActionResult> GetListAsync()
    {
        var model = await _mediator.Send(new ReadPhoneNumberTypeListQuery());

        if (model is not { Count: > 0 })
        {
            return NotFound("Unable to locate records in the phone number type list.");
        }

        return Ok(model);
    }
}
