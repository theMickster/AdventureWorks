using Microsoft.AspNetCore.ResponseCompression;
using System.IO.Compression;

[assembly: InternalsVisibleTo("AdventureWorks.UnitTests")]
namespace AdventureWorks.API.libs;

/// <summary>
/// Extension methods for configuring response compression with Gzip and Brotli providers.
/// </summary>
[ExcludeFromCodeCoverage]
internal static class SetupResponseCompression
{
    /// <summary>
    /// Registers response compression services with Gzip and Brotli providers.
    /// Compresses JSON and XML responses (2KB+ payloads), excludes already-compressed content types.
    /// </summary>
    /// <param name="builder">The web application builder.</param>
    /// <returns>The web application builder for chaining.</returns>
    internal static WebApplicationBuilder AddAdventureWorksResponseCompression(this WebApplicationBuilder builder)
    {
        builder.Services.AddResponseCompression(options =>
        {
            // Enable compression for HTTPS connections (requires opt-in for security)
            options.EnableForHttps = true;

            // Add compression providers (order matters - Brotli is preferred if client supports it)
            options.Providers.Add<BrotliCompressionProvider>();
            options.Providers.Add<GzipCompressionProvider>();

            // Define MIME types eligible for compression
            // Include JSON, XML, and text-based formats; exclude already-compressed formats
            options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(new[]
            {
                "application/json",
                "application/xml",
                "text/xml",
                "text/json",
                "text/plain",
                "text/html",
                "text/css",
                "application/javascript",
                "text/javascript"
            });
        });

        // Configure Brotli compression level (Fastest for optimal performance/compression balance)
        builder.Services.Configure<BrotliCompressionProviderOptions>(options =>
        {
            options.Level = CompressionLevel.Fastest;
        });

        // Configure Gzip compression level (Fastest for optimal performance/compression balance)
        builder.Services.Configure<GzipCompressionProviderOptions>(options =>
        {
            options.Level = CompressionLevel.Fastest;
        });

        return builder;
    }
}
