using AdventureWorks.UserSetup.Console.Entities;
using Microsoft.EntityFrameworkCore;

namespace AdventureWorks.UserSetup.Console.DbContexts;

internal interface IAdventureWorksUserSetupContext
{
    DbSet<UserAccount> UserAccounts { get; set; }

    Task UpdateUserAccountsAsync(UserAccount entity);
}
