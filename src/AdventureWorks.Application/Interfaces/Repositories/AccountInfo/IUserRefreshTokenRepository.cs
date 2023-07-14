using AdventureWorks.Domain.Entities.Shield;

namespace AdventureWorks.Application.Interfaces.Repositories.AccountInfo;

public interface IUserRefreshTokenRepository : IAsyncRepository<UserRefreshTokenEntity>
{
    /// <summary>
    /// Retrieve a list of refresh tokens for a given user.
    /// </summary>
    /// <param name="userId">the unique user identifier; the business entity id.</param>
    /// <param name="refreshToken">the user token to be verified.</param>
    /// <returns></returns>
    Task<IReadOnlyList<UserRefreshTokenEntity>> GetRefreshTokenListByUserIdAsync(int userId, string refreshToken);
}
