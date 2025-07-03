namespace AdventureWorks.Application.Interfaces;

/// <summary>
/// Provides access to the current correlation ID for distributed tracing
/// </summary>
public interface ICorrelationIdAccessor
{
    /// <summary>
    /// Gets the correlation ID for the current request context
    /// </summary>
    /// <returns>The correlation ID, or null if not set</returns>
    string? CorrelationId { get; }

    /// <summary>
    /// Sets the correlation ID for the current request context
    /// </summary>
    /// <param name="correlationId">The correlation ID to set</param>
    void SetCorrelationId(string correlationId);
}
