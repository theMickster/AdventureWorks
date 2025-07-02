using AdventureWorks.Application.Interfaces;
using AdventureWorks.Common.Constants;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility;

namespace AdventureWorks.API.libs.Telemetry;

/// <summary>
/// Application Insights telemetry initializer that adds correlation ID to all telemetry items.
/// This ensures correlation IDs are tracked in Application Insights for distributed tracing.
/// </summary>
public sealed class CorrelationIdTelemetryInitializer(ICorrelationIdAccessor correlationIdAccessor) : ITelemetryInitializer
{
    private readonly ICorrelationIdAccessor _correlationIdAccessor = correlationIdAccessor ?? throw new ArgumentNullException(nameof(correlationIdAccessor));

    /// <summary>
    /// Initializes telemetry item with correlation ID custom property
    /// </summary>
    /// <param name="telemetry">The telemetry item to initialize</param>
    public void Initialize(ITelemetry telemetry)
    {
        ArgumentNullException.ThrowIfNull(telemetry);

        var correlationId = _correlationIdAccessor.CorrelationId;

        if (!string.IsNullOrWhiteSpace(correlationId))
        {
            // Add as custom property for queries in Application Insights
            if (!telemetry.Context.GlobalProperties.ContainsKey(ConfigurationConstants.CorrelationIdHeaderName))
            {
                telemetry.Context.GlobalProperties.Add(ConfigurationConstants.CorrelationIdHeaderName, correlationId);
            }

            // Also add as custom dimension for better filtering
            if (!telemetry.Context.GlobalProperties.ContainsKey("CorrelationId"))
            {
                telemetry.Context.GlobalProperties.Add("CorrelationId", correlationId);
            }
        }
    }
}
