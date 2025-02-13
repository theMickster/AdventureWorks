using AdventureWorks.Common.Settings;
using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace AdventureWorks.API.Controllers.v1.KeyVaultExample;

/// <summary>
/// The controller that handles retrieving mock data from Azure Key Vault.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[ApiExplorerSettings(GroupName = "Azure Key Vault Examples")]
[Route("api/v{version:apiVersion}/keyVaultExample", Name = "KeyVaultExample")]
public sealed class KeyVaultExampleController : ControllerBase
{
    private readonly IOptionsSnapshot<AkvExampleSettings> _akvExampleConfiguration;

    /// <inheritdoc />
    public KeyVaultExampleController(IOptionsSnapshot<AkvExampleSettings> akvExampleConfiguration)
    {
        _akvExampleConfiguration = akvExampleConfiguration;
    }

    /// <summary>
    /// Retrieves a mock secret from Azure Key Vault.
    /// </summary>
    /// <returns>a string containing my favorite comedic movie</returns>
    [HttpGet]
    public IActionResult Get()
    {
        var myFavoriteComedicMovie = _akvExampleConfiguration?.Value?.MyFavoriteComedicMovie ?? string.Empty;

        if (string.IsNullOrWhiteSpace(myFavoriteComedicMovie))
        {
            return NotFound("Unable to locate my favorite comedic movie from Azure Key Vault. Sad panda.");
        }

        return Ok(myFavoriteComedicMovie);
    }
}