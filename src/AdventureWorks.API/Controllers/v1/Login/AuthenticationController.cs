using AdventureWorks.Application.Interfaces.Services.Login;
using AdventureWorks.Domain.Models.AccountInfo;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AdventureWorks.API.Controllers.v1.Login;

/// <summary>
/// The controller that coordinates user authentication.
/// </summary>
/// <remarks></remarks>
[Authorize]
[ApiController]
[ApiVersion("1.0")]
[ApiExplorerSettings(GroupName = "Authentication")]
[Produces("application/json")]
[Route("api/v{version:apiVersion}/authenticate", Name = "AuthenticationControllerV1")]
public sealed class AuthenticationController : ControllerBase
{
    private readonly ILogger<AuthenticationController> _logger;
    private readonly IReadUserLoginService _userLoginService;

    /// <summary>
    /// The controller that coordinates user authentication.
    /// </summary>
    /// <remarks></remarks>
    public AuthenticationController(
        ILogger<AuthenticationController> logger,
        IReadUserLoginService userLoginService
        )
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _userLoginService = userLoginService ?? throw new ArgumentNullException(nameof(userLoginService));
    }

    [AllowAnonymous]
    [HttpPost]
    [SuppressMessage("ReSharper", "InvertIf")]
    public async Task<IActionResult> AuthenticateUser(AuthenticationRequestModel? model)
    {
        if (model == null)
        {
            const string message = "Invalid authentication request.";
            _logger.LogInformation(message);
            return BadRequest(message);
        }

        if (model.Username == null || string.IsNullOrWhiteSpace(model.Username))
        {
            const string message = "Invalid authentication request; Username and Password are required.";
            _logger.LogInformation(message);
            return BadRequest(message);
        }

        if (model.Password == null || string.IsNullOrWhiteSpace(model.Password))
        {
            const string message = "Invalid authentication request; Username and Password are required.";
            _logger.LogInformation(message);
            return BadRequest(message);
        }

        var (userAccount, token, errors ) = await _userLoginService.AuthenticateUserAsync(model.Username, model.Password);

        if (errors.Any() || token is null || userAccount is null)
        {
            const string message = "Unable to complete authentication request.";

            if (errors.Any())
            {
                errors.ForEach(y =>
                    _logger.LogInformation(
                        $"User Authentication Attempt Failed. Error Code:{y.ErrorCode} Error Message: {y.ErrorMessage}"));
            }
            else
            {
                _logger.LogInformation(message);
            }
            
            return BadRequest(message);
        }
        
        var responseModel = new AuthenticationResponseModel
        {
            Token = token,
            Username = userAccount.UserName,
            FullName = userAccount.FullName,
        };

        return Ok(responseModel);
    }

}
