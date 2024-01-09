using AdventureWorks.Application.Interfaces.Services.Stores;
using AdventureWorks.Common.Constants;
using AdventureWorks.Common.Filtering;
using AdventureWorks.Domain.Models.Sales;
using Asp.Versioning;
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
    private readonly IReadStoreService _readStoreService;

    /// <summary>
    /// The controller that coordinates retrieving store information.
    /// </summary>
    /// <remarks></remarks>
    public ReadStoreController(
        ILogger<ReadStoreController> logger,
        IReadStoreService readStoreService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _readStoreService = readStoreService ?? throw new ArgumentNullException(nameof(readStoreService));
    }

    /// <summary>
    /// Retrieve an store using its unique identifier
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

        var address = await _readStoreService.GetByIdAsync(storeId);

        if (address == null)
        {
            return NotFound("Unable to locate Store.");
        }

        return Ok(address);
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
        var searchResult = await _readStoreService.GetStoresAsync(parameters).ConfigureAwait(false);

        if (searchResult.Results == null || !searchResult.Results.Any())
        {
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

        return Ok(searchResult);
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
        var searchResult = await _readStoreService.SearchStoresAsync(parameters, storeSearchModel).ConfigureAwait(false);

        if (searchResult.Results == null || !searchResult.Results.Any())
        {
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

        return Ok(searchResult);
    }
}
