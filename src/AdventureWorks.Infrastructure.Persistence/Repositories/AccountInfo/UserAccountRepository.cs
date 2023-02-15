using AdventureWorks.Application.Interfaces.Repositories;
using AdventureWorks.Common.Attributes;
using AdventureWorks.Infrastructure.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AdventureWorks.Domain.Entities.Shield;

namespace AdventureWorks.Infrastructure.Persistence.Repositories.AccountInfo;

[ServiceLifetimeScoped]
public sealed class UserAccountRepository : IUserAccountRepository
{
    private readonly AdventureWorksDbContext _dbContext;
    private readonly ILogger<UserAccountRepository> _logger;

    public UserAccountRepository(AdventureWorksDbContext dbContext, ILogger<UserAccountRepository> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<UserAccountEntity> GetByIdAsync(int id)
    {
        return await _dbContext.Set<UserAccountEntity>()
            .FirstOrDefaultAsync(x => x.BusinessEntityId == id)
            .ConfigureAwait(false);
    }

    public async Task<UserAccountEntity> GetByUserNameAsync(string username)
    {
        username = username.ToLower().Trim();

        var results = await _dbContext.Set<UserAccountEntity>()
            .Include(x => x.Person)
            .Where(x => x.UserName.ToLower().Trim() == username)
            .ToListAsync()
            .ConfigureAwait(false);

        if (!results.Any())
        {
            return null;
        }

        if (results.Count == 1)
        {
            return results.First();
        }

        _logger.LogInformation($"Multiple accounts for user {username} located.");

        return null;
    }

    public async Task<IReadOnlyList<UserAccountEntity>> ListAllAsync()
    {
        return await _dbContext.Set<UserAccountEntity>()
            .Include(x => x.Person)
            .ToListAsync()
            .ConfigureAwait(false);
    }


}
