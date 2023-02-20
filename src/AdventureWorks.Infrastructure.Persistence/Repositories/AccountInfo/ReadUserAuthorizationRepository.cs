using AdventureWorks.Application.Interfaces.DbContext;
using AdventureWorks.Application.Interfaces.Repositories.AccountInfo;
using AdventureWorks.Common.Attributes;
using AdventureWorks.Domain.Entities.Shield;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace AdventureWorks.Infrastructure.Persistence.Repositories.AccountInfo;

[ServiceLifetimeScoped]
public sealed class ReadUserAuthorizationRepository : IReadUserAuthorizationRepository
{
    private readonly IAdventureWorksDbContext _dbContext;
    private readonly ILogger<ReadUserAuthorizationRepository> _logger;

    public ReadUserAuthorizationRepository(IAdventureWorksDbContext dbContext, ILogger<ReadUserAuthorizationRepository> logger)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Retrieve an AdventureWorks user authorization entity.
    /// </summary>
    /// <param name="userId">the user's unique BusinessEntityId</param>
    /// <returns>a user authorization entity</returns>
    public async Task<UserAuthorizationEntity> GetByUserIdAsync(int userId)
    {
        var userEntity = await _dbContext.UserAccounts
            .FirstOrDefaultAsync(x => x.BusinessEntityId == userId)
            .ConfigureAwait(false);

        if (userEntity is null)
        {
            return null;
        }

        var userGroupMapping =
            await _dbContext.SecurityGroupUserAccounts
                .Include(x => x.SecurityGroup)
                .Include(x => x.UserAccount)
                .Include(x => x.BusinessEntity)
                .Where(x => x.BusinessEntityId == userEntity.BusinessEntityId)
                .ToListAsync()
                .ConfigureAwait(false);

        var securityGroups = userGroupMapping.Select(x => x.SecurityGroup).ToList();

        var securityGroupIds = userGroupMapping.Select(x => x.GroupId).ToList();

        var securityRoles = await _dbContext.SecurityGroupSecurityRoles
                .Include(x => x.SecurityRole)
                .Where(x => securityGroupIds.Contains(x.GroupId))
                .Select(z => z.SecurityRole)
                .ToListAsync()
                .ConfigureAwait(false);

        var securityFunctions = await _dbContext.SecurityGroupSecurityFunctions
                .Include(x => x.SecurityFunction)
                .Where(x => securityGroupIds.Contains(x.GroupId))
                .Select(z => z.SecurityFunction)
                .ToListAsync()
                .ConfigureAwait(false);

        return new UserAuthorizationEntity
        {
            SecurityGroups = securityGroups.AsReadOnly(),
            SecurityRoles = securityRoles.AsReadOnly(),
            SecurityFunctions = securityFunctions.AsReadOnly(),
            BusinessEntityId = userEntity.BusinessEntityId
        };
    }

}
