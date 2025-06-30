using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Runtime.CompilerServices;

namespace AdventureWorks.Application.HealthChecks;

public class DefaultHealthCheck : IHealthCheck
{
    private readonly Func<(HealthStatus status, Dictionary<string, object> deps)> _statusFunc;

    /// <summary>
    /// default constructor.  When used no status function is set and the status will always be healthy.
    /// </summary>
    public DefaultHealthCheck() =>
        _statusFunc = (Func<(HealthStatus, Dictionary<string, object>)>)
                        (() => (HealthStatus.Healthy, new Dictionary<string, object>()));

    /// <summary>
    /// constructor to use when you want to add code to perform a health check.
    /// </summary>
    /// <param name="statusFunc"></param>
    public DefaultHealthCheck(Func<(HealthStatus status, Dictionary<string, object> deps)> statusFunc)
    {
        _statusFunc = statusFunc ?? throw new ArgumentNullException(nameof(statusFunc));
    }

    /// <summary>Default Implementation</summary>
    /// <param name="context">HealthCheckContext</param>
    /// <param name="cancellationToken">CancellationToken</param>
    /// <returns>HealthCheckResult</returns>
    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var dictionary1 = MetadataAccessor.DictionaryFromType(MetadataAccessor.ProgramMetadata);

        var (healthStatus, dictionary2) = _statusFunc();

        var dictionary3 = new Dictionary<string, object>
        {
            {
                "Info",
                dictionary1
            },
            {
                "Dependencies",
                dictionary2
            }
        };

        var status = (int)healthStatus;

        var interpolatedStringHandler = new DefaultInterpolatedStringHandler(4, 2);

        interpolatedStringHandler.AppendFormatted(MetadataAccessor.ProgramMetadata.Product);
        interpolatedStringHandler.AppendLiteral(" is ");
        interpolatedStringHandler.AppendFormatted(healthStatus);

        var stringAndClear = interpolatedStringHandler.ToStringAndClear();

        return Task.FromResult(new HealthCheckResult((HealthStatus)status, stringAndClear, data: dictionary3));
    }
}