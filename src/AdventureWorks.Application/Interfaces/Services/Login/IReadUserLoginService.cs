using AdventureWorks.Domain.Models.AccountInfo;
using FluentValidation.Results;

namespace AdventureWorks.Application.Interfaces.Services.Login;

public interface IReadUserLoginService
{
    /// <summary>
    /// Authenticate an AdventureWorks user and, if the request is valid, the generate a JWT security token
    /// </summary>
    /// <param name="username"></param>
    /// <param name="password"></param>
    /// <returns>a tuple that includes the user model, security token, and validation failure list </returns>
    Task<(UserAccountModel?, UserAccountTokenModel?, List<ValidationFailure>)> AuthenticateUserAsync(string username, string password);
}
