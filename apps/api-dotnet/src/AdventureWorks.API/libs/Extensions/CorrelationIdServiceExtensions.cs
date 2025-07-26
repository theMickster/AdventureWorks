using AdventureWorks.API.libs.Services;
using AdventureWorks.Application.Interfaces;

namespace AdventureWorks.API.libs.Extensions;

/// <summary>
/// Extension methods for registering correlation ID services
/// </summary>
[ExcludeFromCodeCoverage]
public static class CorrelationIdServiceExtensions
{
    /// <summary>
    /// Registers all correlation ID related services
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddCorrelationIdServices(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        // Register correlation ID accessor as singleton (safe because it uses IHttpContextAccessor which is thread-safe)
        services.AddSingleton<ICorrelationIdAccessor, CorrelationIdAccessor>();

        return services;
    }
}
