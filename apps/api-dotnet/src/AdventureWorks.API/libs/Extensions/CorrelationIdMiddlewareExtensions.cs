using AdventureWorks.API.libs.Middleware;

namespace AdventureWorks.API.libs.Extensions;

/// <summary>
/// Extension methods for registering correlation ID middleware
/// </summary>
[ExcludeFromCodeCoverage]
public static class CorrelationIdMiddlewareExtensions
{
    /// <summary>
    /// Adds the correlation ID middleware to the application pipeline.
    /// This should be registered early in the pipeline to ensure correlation IDs are available for all subsequent middleware.
    /// </summary>
    /// <param name="app">The application builder</param>
    /// <returns>The application builder for chaining</returns>
    public static IApplicationBuilder UseCorrelationId(this IApplicationBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);
        return app.UseMiddleware<CorrelationIdMiddleware>();
    }
}
