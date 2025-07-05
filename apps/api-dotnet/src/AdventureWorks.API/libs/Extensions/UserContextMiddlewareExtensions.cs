using AdventureWorks.API.libs.Middleware;

namespace AdventureWorks.API.libs.Extensions;

/// <summary>
/// Extension methods for registering user context middleware.
/// </summary>
[ExcludeFromCodeCoverage]
public static class UserContextMiddlewareExtensions
{
    /// <summary>
    /// Adds the user context middleware to the application pipeline.
    /// Should be registered after authentication but before authorization.
    /// </summary>
    /// <param name="app">The application builder</param>
    /// <returns>The application builder for chaining</returns>
    public static IApplicationBuilder UseUserContext(this IApplicationBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);
        return app.UseMiddleware<UserContextMiddleware>();
    }
}
