using AdventureWorks.Application.Features.Sales.Commands;
using AdventureWorks.Application.Features.Sales.Queries;
using AdventureWorks.Models.Features.Sales;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AdventureWorks.API.Controllers.v1.SalesPersons;

/// <summary>
/// The controller that coordinates creating sales person information.
/// </summary>
[ApiController]
[Authorize]
[ApiVersion("1.0")]
[ApiExplorerSettings(GroupName = "SalesPerson")]
[Produces("application/json")]
[Route("api/v{version:apiVersion}/salespersons", Name = "CreateSalesPersonControllerV1")]
public sealed class CreateSalesPersonController : ControllerBase
{
    private readonly ILogger<CreateSalesPersonController> _logger;
    private readonly IMediator _mediator;

    /// <summary>
    /// The controller that coordinates creating sales person information.
    /// </summary>
    public CreateSalesPersonController(
        ILogger<CreateSalesPersonController> logger,
        IMediator mediator)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(mediator);
        _logger = logger;
        _mediator = mediator;
    }

    /// <summary>
    /// Creates a new sales person
    /// </summary>
    /// <param name="inputModel">the sales person to create</param>
    /// <returns></returns>
    [HttpPost]
    [Produces(typeof(SalesPersonModel))]
    public async Task<IActionResult> PostAsync([FromBody] SalesPersonCreateModel? inputModel)
    {
        if (inputModel == null)
        {
            _logger.LogWarning("CreateSalesPerson called with null input model");
            return BadRequest("The sales person input model cannot be null.");
        }

        _logger.LogInformation(
            "Creating sales person: TerritoryId={TerritoryId}, HireDate={HireDate}, AddressTypeId={AddressTypeId}, HasSalesQuota={HasSalesQuota}",
            inputModel.TerritoryId,
            inputModel.HireDate,
            inputModel.AddressTypeId,
            inputModel.SalesQuota.HasValue);

        var cmd = new CreateSalesPersonCommand
        {
            Model = inputModel,
            ModifiedDate = DateTime.UtcNow,
            RowGuid = Guid.NewGuid()
        };

        var salesPersonId = await _mediator.Send(cmd);
        var model = await _mediator.Send(new ReadSalesPersonQuery { Id = salesPersonId });
        ArgumentNullException.ThrowIfNull(model);

        _logger.LogInformation(
            "Sales person created successfully: SalesPersonId={SalesPersonId}, TerritoryId={TerritoryId}, JobTitle={JobTitle}",
            salesPersonId,
            model.TerritoryId,
            model.JobTitle);

        return CreatedAtRoute("GetSalesPersonById", new { salesPersonId = model.Id }, model);
    }
}
