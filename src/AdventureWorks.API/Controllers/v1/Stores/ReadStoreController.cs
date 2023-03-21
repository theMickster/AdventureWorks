using AdventureWorks.Application.Interfaces.Services.Stores;
using AdventureWorks.Common.Filtering;
using AdventureWorks.Domain.Models.Sales;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
    [Produces(typeof(StoreModel))]
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
    [Produces(typeof(List<StoreModel>))]
    public async Task<IActionResult> GetStoreListAsync([FromQuery] StoreParameter parameters)
    {

        var storeList = new List<StoreModel>()
        {
            new (){Id = 1, Name = "Test01"},
            new (){Id = 1, Name = "Test02"}
        };


        return Ok(storeList);
    }
}
