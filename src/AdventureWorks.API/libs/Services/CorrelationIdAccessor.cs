using AdventureWorks.Application.Interfaces;

[assembly: InternalsVisibleTo("AdventureWorks.UnitTests")]
namespace AdventureWorks.API.libs.Services;

/// <summary>
/// Provides thread-safe access to the current correlation ID using HttpContext
/// </summary>
internal sealed class CorrelationIdAccessor(IHttpContextAccessor httpContextAccessor) : ICorrelationIdAccessor
{
    private const string CorrelationIdItemKey = "CorrelationId";
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));

    /// <summary>
    /// Gets the correlation ID from the current HttpContext
    /// </summary>
    public string? CorrelationId
    {
        get
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext?.Items.TryGetValue(CorrelationIdItemKey, out var correlationId) == true)
            {
                return correlationId as string;
            }
            return null;
        }
    }

    /// <summary>
    /// Sets the correlation ID in the current HttpContext
    /// </summary>
    /// <param name="correlationId">The correlation ID to set</param>
    public void SetCorrelationId(string correlationId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(correlationId);

        var httpContext = _httpContextAccessor.HttpContext;
        httpContext?.Items[CorrelationIdItemKey] = correlationId;
    }
}
