using AdventureWorks.Application.PersistenceContracts.DbContext;
using Microsoft.Extensions.DependencyInjection;

namespace AdventureWorks.Testing.Console.Verifications;

internal sealed class VerifyDbContext
{
    private readonly IAdventureWorksDbContext _dbContext;

    public VerifyDbContext(IServiceProvider serviceProvider)
    {
        if (serviceProvider == null)
        {
            throw new ArgumentNullException(nameof(serviceProvider));
        }

        _dbContext = serviceProvider.GetRequiredService<IAdventureWorksDbContext>() ??
                     throw new InvalidOperationException(
                         "Unable to find a concrete implementation of IAdventureWorksDbContext ");
    }

    public Task<(bool status, List<string> errorsList)> VerifyAllTheThings()
    {
        var success = true;
        var errorList = new List<string>();

        var functions = _dbContext.SecurityFunctions.ToList();
        if (functions.Count != 0)
        {
            success = false;

        }

        var roles = _dbContext.SecurityRoles.ToList();
        if (roles.Count == 0)
        {
            success = false;
        }

        var groups = _dbContext.SecurityGroups.ToList();
        if (groups.Count == 0)
        {
            success = false;
        }


        return Task.FromResult((success, errorList));
    }
}
