﻿using AdventureWorks.Domain.Entities.Shield;

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

    /// <summary>
    /// Retrieve the most recent, non-expired refresh token for a given user.
    /// </summary>
    /// <param name="userId">the unique user identifier; the business entity id.</param>
    /// <returns></returns>
    Task<UserRefreshTokenEntity?> GetMostRecentRefreshTokenByUserIdAsync(int userId);

    /// <summary>
    /// Revoke a user token.
    /// </summary>
    /// <param name="token"></param>
    /// <returns></returns>
    Task RevokeRefreshTokenAsync(UserRefreshTokenEntity token);

    /// <summary>
    /// Revoke all instances of a given token (string). 
    /// </summary>
    /// <remarks>The user associated with the token is irrelevant</remarks>
    /// <param name="refreshToken"></param>
    /// <returns></returns>
    Task<int> RevokeRefreshTokenAsync(string refreshToken);
}
