using AdventureWorks.API.libs.Services;
using AdventureWorks.API.libs.Telemetry;
using AdventureWorks.Application.Interfaces;
using Microsoft.ApplicationInsights.Extensibility;

namespace AdventureWorks.API.libs.Extensions;

/// <summary>
/// Extension methods for registering correlation ID services
/// </summary>
[ExcludeFromCodeCoverage]
public static class CorrelationIdServiceExtensions
{
    /// <summary>
    /// Registers all correlation ID related services including accessor and Application Insights telemetry initializer
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddCorrelationIdServices(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        // Register correlation ID accessor as singleton (safe because it uses IHttpContextAccessor which is thread-safe)
        services.AddSingleton<ICorrelationIdAccessor, CorrelationIdAccessor>();

        // Register Application Insights telemetry initializer
        services.AddSingleton<ITelemetryInitializer, CorrelationIdTelemetryInitializer>();

        return services;
    }
}
