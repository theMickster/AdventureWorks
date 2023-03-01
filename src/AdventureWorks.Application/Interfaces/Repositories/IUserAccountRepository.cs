using AdventureWorks.Domain.Entities.Shield;

namespace AdventureWorks.Application.Interfaces.Repositories;

public interface IUserAccountRepository
{
    Task<UserAccountEntity> GetByIdAsync(int id);

    Task<UserAccountEntity> GetByUserNameAsync(string username);
    
    Task<IReadOnlyList<UserAccountEntity>> ListAllAsync();
}
