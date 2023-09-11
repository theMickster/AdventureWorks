using AdventureWorks.Domain.Models.Shield;

namespace AdventureWorks.Application.Interfaces.Services.Login;

public interface ITokenService
{
    /// <summary>
    /// Creates a JWT security token for the given <see cref="UserAccountModel"/>
    /// </summary>
    /// <param name="userAccount">the AdventureWorks user requesting the token</param>
    /// <param name="ipAddress">the IP of the remote host making the token request</param>
    /// <returns>a valid AdventureWorks API token model</returns>
    Task<UserAccountTokenModel> GenerateUserTokenAsync(UserAccountModel userAccount, string ipAddress);
}
