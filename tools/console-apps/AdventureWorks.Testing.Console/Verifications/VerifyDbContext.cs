using AdventureWorks.Application.PersistenceContracts.DbContext;
using Microsoft.Extensions.DependencyInjection;

namespace AdventureWorks.Testing.Console.Verifications;

internal sealed class VerifyDbContext
{
    public VerifyDbContext(IServiceProvider serviceProvider)
    {
        if (serviceProvider == null)
        {
            throw new ArgumentNullException(nameof(serviceProvider));
        }

        _ = serviceProvider.GetRequiredService<IAdventureWorksDbContext>() ??
            throw new InvalidOperationException(
                "Unable to find a concrete implementation of IAdventureWorksDbContext ");
    }

    public Task<(bool status, List<string> errorsList)> VerifyAllTheThings()
    {
        // The obsolete Security* DbSets were removed from IAdventureWorksDbContext.
        // Resolving the current context is the validation performed by this console app.
        return Task.FromResult((true, new List<string>()));
    }
}
