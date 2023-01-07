using AdventureWorks.Application.Interfaces.DbContext;
using AdventureWorks.Infrastructure.DbContexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace AdventureWorks.API;

public static class CustomExtensionMethods
{
    public static IServiceCollection AddCustomDbContext(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<IAdventureWorksDbContext, AdventureWorksDbContext>(options =>
        {
            options.UseSqlServer(configuration["AdventureWorksDatabase"],
                sqlServerOptionsAction: sqlOptions =>
                {
                    //Configuring Connection Resiliency: https://docs.microsoft.com/en-us/ef/core/miscellaneous/connection-resiliency 
                    sqlOptions.EnableRetryOnFailure(maxRetryCount: 10, maxRetryDelay: TimeSpan.FromSeconds(30), errorNumbersToAdd: null);
                });

            // Changing default behavior when client evaluation occurs to throw. 
            // Default in EF Core would be to log a warning when client evaluation is performed.
            options.ConfigureWarnings(warnings => warnings.Throw(RelationalEventId.QueryPossibleUnintendedUseOfEqualsWarning));
                
        });

        return services;
    }
}