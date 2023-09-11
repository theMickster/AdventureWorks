using AdventureWorks.Application.Interfaces.Services.Login;
using AdventureWorks.Common.Settings;
using AdventureWorks.Domain.Models.Shield;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

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
    private readonly IUserLoginService _userLoginService;
    private readonly IOptionsSnapshot<TokenSettings> _tokenSettings;

    /// <summary>
    /// The controller that coordinates user authentication.
    /// </summary>
    /// <remarks></remarks>
    public AuthenticationController(
        ILogger<AuthenticationController> logger,
        IUserLoginService userLoginService,
        IOptionsSnapshot<TokenSettings> tokenSettings
        )
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _userLoginService = userLoginService ?? throw new ArgumentNullException(nameof(userLoginService));
        _tokenSettings = tokenSettings ?? throw new ArgumentNullException(nameof(tokenSettings));
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

        var ipAddress = GetRequestIpAddress();

        var (userAccount, token, errors ) = await _userLoginService.AuthenticateUserAsync(model.Username, model.Password, ipAddress);

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
            EmailAddress = userAccount.PrimaryEmailAddress,
            SecurityFunctions = userAccount.SecurityFunctions,
            SecurityGroups = userAccount.SecurityGroups,
            SecurityRoles = userAccount.SecurityRoles
        };

        AppendCookies(responseModel);

        return Ok(responseModel);
    }

    [AllowAnonymous]
    [HttpPost("refreshToken")]
    public async Task<IActionResult> RefreshUserToken()
    {
        Request.Cookies.TryGetValue("X-Refresh-Token", out var refreshToken);
        Request.Cookies.TryGetValue("X-Username", out var username);
        
        if (username == null || string.IsNullOrWhiteSpace(username))
        {
            const string message = "Invalid refresh token request; Username not found but is required.";
            _logger.LogInformation(message);
            return BadRequest(message);
        }

        if (refreshToken == null || string.IsNullOrWhiteSpace(refreshToken))
        {
            const string message = "Invalid refresh token request; Refresh Token not found but is required.";
            _logger.LogInformation(message);
            return BadRequest(message);
        }
        
        var ipAddress = GetRequestIpAddress();

        var (userAccount, token, errors) = await _userLoginService.RefreshTokenAsync(refreshToken, username, ipAddress);

        if (errors.Any() || token is null || userAccount is null)
        {
            const string message = "Unable to complete authentication request.";

            if (errors.Any())
            {
                errors.ForEach(y =>
                    _logger.LogInformation(
                        $"User Authentication Attempt Failed - Token Refresh. Error Code:{y.ErrorCode} Error Message: {y.ErrorMessage}"));
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
            EmailAddress = userAccount.PrimaryEmailAddress,
            SecurityFunctions = userAccount.SecurityFunctions,
            SecurityGroups = userAccount.SecurityGroups,
            SecurityRoles = userAccount.SecurityRoles
        };

        AppendCookies(responseModel);

        return Ok(responseModel);
    }

    [AllowAnonymous]
    [HttpPost("revokeToken")]
    public async Task<IActionResult> RevokeUserToken(RevokeTokenRequestModel? model)
    {
        if (model == null)
        {
            const string message = "Invalid revoke token request.";
            _logger.LogInformation(message);
            return BadRequest(message);
        }

        var (result, errors) = await _userLoginService.RevokeRefreshTokenAsync(model.RefreshToken).ConfigureAwait(false);

        if (!result)
        {
            const string message = "Unable to complete authentication request.";

            if (errors.Any())
            {
                errors.ForEach(y =>
                    _logger.LogInformation(
                        $"User Authentication Attempt Failed - Unable to Revoke Refresh Token. Error Code:{y.ErrorCode} Error Message: {y.ErrorMessage}"));
            }
            else
            {
                _logger.LogInformation(message);
            }

            return BadRequest(message);
        }

        return Ok();
    }

    private string GetRequestIpAddress()
    {
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString() ?? "127.0.0.1";
        ipAddress = ipAddress == "0.0.0.1" ? "127.0.0.1" : ipAddress;
        return ipAddress;
    }

    private void AppendCookies(AuthenticationResponseModel model)
    {
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTime.UtcNow.AddDays(_tokenSettings.Value.RefreshTokenExpirationInDays),
            Secure = true
        };

        Response.Cookies.Append("X-Refresh-Token", model.Token.RefreshToken, cookieOptions);
        Response.Cookies.Append("X-Access-Token", model.Token.Token, cookieOptions);
        Response.Cookies.Append("X-Username", model.Username, cookieOptions);
    }
}
