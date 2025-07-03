using AdventureWorks.UserSetup.Console.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace AdventureWorks.UserSetup.Console.Services;

internal class UpdateUserAccountService
{
    private readonly IAdventureWorksUserSetupContext _dbContext;

    public UpdateUserAccountService(
        IAdventureWorksUserSetupContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<bool> UpdateUserAccountPasswords(string passwordPrefix)
    {
        var accounts =
            await _dbContext.UserAccounts
                .Where(x => string.IsNullOrWhiteSpace(x.PasswordHash))
                .Take(10000)
                .OrderBy(x => x.Id)
                .ToListAsync();

        var id = accounts.Select(y => y.Id).ToList();

        accounts.ForEach(userAccount =>
        {
            var password = $"{passwordPrefix}{userAccount.Id}!";
            var hash = BC.HashPassword(password);

            if (!BC.Verify(password, hash))
            {
                return;
            }

            userAccount.PasswordHash = hash;
            _dbContext.UpdateUserAccountsAsync(userAccount);
        });
        

        return true;
    }

}
