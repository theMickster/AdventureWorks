#nullable enable
using System;
using AdventureWorks.Application.Interfaces.Repositories.AccountInfo;
using AdventureWorks.Common.Attributes;
using AdventureWorks.Domain.Entities.Shield;
using AdventureWorks.Infrastructure.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AdventureWorks.Infrastructure.Persistence.Repositories.AccountInfo;

[ServiceLifetimeScoped]
public sealed class UserRefreshTokenRepository : EfRepository<UserRefreshTokenEntity>, IUserRefreshTokenRepository
{
    public UserRefreshTokenRepository(AdventureWorksDbContext dbContext) : base(dbContext)
    {
    }

    /// <summary>
    /// Retrieve a list of refresh tokens for a given user.
    /// </summary>
    /// <param name="userId">the unique user identifier; the business entity id.</param>
    /// <param name="refreshToken">the user token to be verified.</param>
    /// <returns></returns>
    public async Task<IReadOnlyList<UserRefreshTokenEntity>> GetRefreshTokenListByUserIdAsync(int userId, string refreshToken)
    {
        return (await DbContext.UserRefreshTokens.Where(x => x.BusinessEntityId == userId
                                                      && x.RefreshToken.ToLower().Trim() ==
                                                      refreshToken.ToLower().Trim())
            .ToListAsync()).AsReadOnly();
    }

    /// <summary>
    /// Retrieve the most recent, non-expired refresh token for a given user.
    /// </summary>
    /// <param name="userId">the unique user identifier; the business entity id.</param>
    /// <returns></returns>
    public async Task<UserRefreshTokenEntity?> GetMostRecentRefreshTokenByUserIdAsync(int userId)
    {
        return await DbContext.UserRefreshTokens
                .FirstOrDefaultAsync(x => x.BusinessEntityId == userId && !x.IsExpired);
    }

    /// <summary>
    /// Revoke a user token.
    /// </summary>
    /// <param name="token"></param>
    /// <returns></returns>
    public async Task RevokeRefreshTokenAsync(UserRefreshTokenEntity token)
    {
        await RevokeAndUpdateToken(token).ConfigureAwait(false);
    }

    /// <summary>
    /// Revoke all instances of a given token (string). 
    /// </summary>
    /// <remarks>The user associated with the token is irrelevant</remarks>
    /// <param name="refreshToken"></param>
    /// <returns></returns>
    public async Task<int> RevokeRefreshTokenAsync(string refreshToken)
    {
        var tokensToRevoke = await DbContext.UserRefreshTokens
            .Where(x => x.RefreshToken.ToLower().Trim() == refreshToken.ToLower().Trim()).ToListAsync();

        if (tokensToRevoke.Count == 0)
        {
            return 0;
        }

        foreach (var token in tokensToRevoke)
        {
            await RevokeAndUpdateToken(token).ConfigureAwait(false);
        }

        return tokensToRevoke.Count;
    }

    private async Task RevokeAndUpdateToken(UserRefreshTokenEntity token)
    {
        token.IsRevoked = true;
        token.RevokedOn = DateTime.UtcNow;
        await UpdateAsync(token);
    }
}
