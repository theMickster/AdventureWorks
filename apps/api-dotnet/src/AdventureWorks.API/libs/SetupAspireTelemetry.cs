using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

namespace AdventureWorks.API.libs;

[ExcludeFromCodeCoverage]
internal static class SetupAspireTelemetry
{
    /// <summary>
    /// Registers the OpenTelemetry OTLP export pipeline for the Aspire local dashboard.
    /// </summary>
    /// <remarks>
    /// Application Insights owns telemetry in every environment except local Aspire.
    /// Aspire is the only thing that sets OTEL_EXPORTER_OTLP_ENDPOINT, so that env var
    /// is a precise signal — not a dev/prod check — that avoids adding a second export
    /// pipeline where there is no dashboard to send to.
    /// </remarks>
    internal static IHostApplicationBuilder AddAspireTelemetry(this IHostApplicationBuilder builder)
    {
        if (string.IsNullOrWhiteSpace(builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"]))
            return builder;

        builder.Logging.AddOpenTelemetry(logging =>
        {
            logging.IncludeFormattedMessage = true;
            logging.IncludeScopes = true;
        });

        builder.Services.AddOpenTelemetry()
            .WithTracing(tracing => tracing
                .AddAspNetCoreInstrumentation(options =>
                    options.Filter = ctx =>
                        !ctx.Request.Path.StartsWithSegments("/health"))
                .AddHttpClientInstrumentation())
            .WithMetrics(metrics => metrics
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddRuntimeInstrumentation())
            .UseOtlpExporter();

        return builder;
    }
}
