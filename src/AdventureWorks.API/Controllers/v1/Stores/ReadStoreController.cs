using AdventureWorks.Application.Features.Sales.Queries;
using AdventureWorks.Common.Constants;
using AdventureWorks.Common.Filtering;
using AdventureWorks.Models.Features.Sales;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using System.Text.Json;

namespace AdventureWorks.API.Controllers.v1.Stores;

/// <summary>
/// The controller that coordinates retrieving store information.
/// </summary>
/// <remarks></remarks>
[ApiController]
[Authorize]
[ApiVersion("1.0")]
[ApiExplorerSettings(GroupName = "Store")]
[Produces("application/json")]
[Route("api/v{version:apiVersion}/stores", Name = "ReadStoreControllerV1")]
public sealed class ReadStoreController : ControllerBase
{
    private readonly ILogger<ReadStoreController> _logger;
    private readonly IMediator _mediator;

    /// <summary>
    /// The controller that coordinates retrieving store information.
    /// </summary>
    /// <remarks></remarks>
    public ReadStoreController(
        ILogger<ReadStoreController> logger,
        IMediator mediator)
    {
        ArgumentNullException.ThrowIfNull(logger, nameof(logger));
        ArgumentNullException.ThrowIfNull(mediator, nameof(mediator));
        _logger = logger;
        _mediator = mediator;
    }

    /// <summary>
    /// Retrieve a store using its unique identifier
    /// </summary>
    /// <param name="storeId">the unique identifier</param>
    /// <returns></returns>
    [HttpGet("{storeId:int}", Name = "GetStoreById")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(StoreModel))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetByIdAsync(int storeId)
    {
        if (storeId <= 0)
        {
            return BadRequest("A valid store id must be specified.");
        }

        var model = await _mediator.Send(new ReadStoreQuery { Id = storeId });

        return model is null ? NotFound("Unable to locate the store.") : Ok(model);
    }

    /// <summary>
    /// Retrieves a paged list of stores
    /// </summary>
    /// <param name="parameters">store pagination query string</param>
    /// <returns></returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(StoreSearchResultModel))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetStoreListAsync([FromQuery] StoreParameter parameters)
    {
        var searchResult = await _mediator.Send( new ReadStoreListQuery{Parameters = parameters});

        if (searchResult.Results is not (null or { Count: 0 }))
        {
            return Ok(searchResult);
        }
        
        var logErrorParams = new
        {
            Status = AppLoggingConstants.StatusBadRequest,
            Operation = "StoreListAsync",
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
    /// Retrieves a paged list of stores
    /// </summary>
    /// <param name="parameters">store pagination query string</param>
    /// <param name="storeSearchModel">the store search input model</param>
    /// <returns></returns>
    [HttpPost]
    [Route("search", Name = "SearchStoresAsync")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(StoreSearchResultModel))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SearchStoresAsync([FromQuery] StoreParameter parameters, [FromBody] StoreSearchModel storeSearchModel)
    {
        var searchResult = await _mediator.Send(new ReadStoreListQuery { Parameters = parameters, SearchModel = storeSearchModel});

        if (searchResult.Results is not (null or { Count: 0 }))
        {
            return Ok(searchResult);
        }

        var logErrorParams = new
        {
            Status = AppLoggingConstants.StatusBadRequest,
            Operation = "StoreSearchAsync",
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
