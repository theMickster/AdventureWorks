[assembly: InternalsVisibleTo("AdventureWorks.UnitTests")]
namespace AdventureWorks.API.libs;

[ExcludeFromCodeCoverage]
internal static class SetupHttpHeaders
{
    /// <summary>
    /// Extension to create and manage HTTP headers
    /// </summary>
    /// <param name="app">IApplicationBuilder</param>
    /// <param name="isDevelopment">Turn off CSP in Develop Mode as it blocks Swagger</param>
    /// <param name="nonce">guid string param</param>
    /// <returns>IApplicationBuilder</returns>
    public static IApplicationBuilder ConfigureApplicatonHeaders(
        this IApplicationBuilder app, bool isDevelopment,
        string nonce)
    {
        app.Use(async (context, next) => {
            context.Response.GetTypedHeaders().CacheControl = new Microsoft.Net.Http.Headers.CacheControlHeaderValue
            {
                NoCache = true,
                NoStore = true,
                MustRevalidate = true
            };

            var cspHeader = string.Join("; ", new List<string>
            {
                "default-src 'self' *.azurewebsites.net",
                "img-src data: 'self' https:",
                $"object-src 'nonce-{nonce}'",
                $"script-src 'self' 'nonce-{nonce}'",
                $"style-src 'self' 'nonce-{nonce}'",
                "upgrade-insecure-requests",
            });

            // Prohibited Headers
            context.Response.Headers.Remove("splitsdkversion");
            context.Response.Headers.Remove("x-aspnet-version");
            context.Response.Headers.Remove("x-powered-by");
            context.Response.Headers.Remove("server");

            // Required Headers
            context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
            context.Response.Headers.Append("X-Frame-Options", "DENY");
            context.Response.Headers.Append("X-Xss-Protection", "1;mode=block");
            context.Response.Headers.Append("Strict-Transport-Security", "max-age=31536000; includeSubDomains");
            context.Response.Headers.Append("Content-Security-Policy", cspHeader);

            await next(context).ConfigureAwait(false);
        });

        return app;
    }
}