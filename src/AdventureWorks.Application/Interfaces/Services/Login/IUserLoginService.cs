using AdventureWorks.Domain.Models.Shield;
using FluentValidation.Results;
using System.Net;

namespace AdventureWorks.Application.Interfaces.Services.Login;

public interface IUserLoginService
{
    /// <summary>
    /// Authenticate an AdventureWorks user and, if the request is valid, the generate a JWT security token
    /// </summary>
    /// <param name="username"></param>
    /// <param name="password"></param>
    /// <param name="ipAddress"></param>
    /// <returns>a tuple that includes the user model, security token, and validation failure list </returns>
    Task<(UserAccountModel?, UserAccountTokenModel?, List<ValidationFailure>)> AuthenticateUserAsync(string username, string password, string ipAddress);

    /// <summary>
    /// Generate a user's JWT security token from a refresh token
    /// </summary>
    /// <param name="refreshToken"></param>
    /// <param name="username"></param>
    /// <param name="ipAddress"></param>
    /// <returns>a tuple that includes the user model, security token, and validation failure list </returns>
    Task<(UserAccountModel?, UserAccountTokenModel?, List<ValidationFailure>)> RefreshTokenAsync(string refreshToken, string username, string ipAddress);
}
