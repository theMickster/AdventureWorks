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
}
