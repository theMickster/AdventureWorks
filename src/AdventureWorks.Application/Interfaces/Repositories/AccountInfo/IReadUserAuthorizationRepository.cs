using AdventureWorks.Domain.Entities.Shield;

namespace AdventureWorks.Application.Interfaces.Repositories.AccountInfo;

public interface IReadUserAuthorizationRepository
{
    /// <summary>
    /// Retrieve an AdventureWorks user authorization entity.
    /// </summary>
    /// <param name="userId">the user's unique BusinessEntityId</param>
    /// <returns>a user authorization entity</returns>
    Task<UserAuthorizationEntity> GetByUserIdAsync(int userId);
}
