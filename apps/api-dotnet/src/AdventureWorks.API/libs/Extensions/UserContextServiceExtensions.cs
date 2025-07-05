using AdventureWorks.API.libs.Services;
using AdventureWorks.Application.Interfaces;

namespace AdventureWorks.API.libs.Extensions;

/// <summary>
/// Extension methods for registering user context services.
/// </summary>
[ExcludeFromCodeCoverage]
public static class UserContextServiceExtensions
{
    /// <summary>
    /// Registers user context accessor service.
    /// Registered as singleton (thread-safe via IHttpContextAccessor).
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddUserContextServices(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        // Register as singleton (thread-safe via IHttpContextAccessor)
        services.AddSingleton<IUserContextAccessor, UserContextAccessor>();

        return services;
    }
}
