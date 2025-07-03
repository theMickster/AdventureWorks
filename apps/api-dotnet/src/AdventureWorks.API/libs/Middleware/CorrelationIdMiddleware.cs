using AdventureWorks.Application.Interfaces;
using AdventureWorks.Common.Constants;

[assembly: InternalsVisibleTo("AdventureWorks.UnitTests")]
namespace AdventureWorks.API.libs.Middleware;

/// <summary>
/// Middleware that ensures every request has a correlation ID for distributed tracing.
/// If the request doesn't have an X-Correlation-ID header, one is generated.
/// The correlation ID is added to the response headers and made available throughout the request pipeline.
/// </summary>

[ExcludeFromCodeCoverage]
public sealed class CorrelationIdMiddleware(RequestDelegate next, ILogger<CorrelationIdMiddleware> logger)
{
    private readonly RequestDelegate _next = next ?? throw new ArgumentNullException(nameof(next));
    private readonly ILogger<CorrelationIdMiddleware> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    /// <summary>
    /// Processes the HTTP request to extract or generate a correlation ID
    /// </summary>
    /// <param name="context">The HTTP context</param>
    /// <param name="correlationIdAccessor">Service for accessing/storing the correlation ID</param>
    public async Task InvokeAsync(HttpContext context, ICorrelationIdAccessor correlationIdAccessor)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(correlationIdAccessor);

        // Try to get correlation ID from request header
        var correlationId = context.Request.Headers[ConfigurationConstants.CorrelationIdHeaderName].FirstOrDefault();

        // If not present, generate a new one
        if (string.IsNullOrWhiteSpace(correlationId))
        {
            correlationId = Guid.NewGuid().ToString("D");
            _logger.LogDebug("Generated new correlation ID: {CorrelationId}", correlationId);
        }
        else
        {
            _logger.LogDebug("Using existing correlation ID from request: {CorrelationId}", correlationId);
        }

        // Store in accessor for use throughout the request
        correlationIdAccessor.SetCorrelationId(correlationId);

        // Add to response headers for client tracking
        context.Response.OnStarting(() =>
        {
            if (!context.Response.Headers.ContainsKey(ConfigurationConstants.CorrelationIdHeaderName))
            {
                context.Response.Headers.Append(ConfigurationConstants.CorrelationIdHeaderName, correlationId);
            }
            return Task.CompletedTask;
        });

        // Continue processing the request
        await _next(context);
    }
}
