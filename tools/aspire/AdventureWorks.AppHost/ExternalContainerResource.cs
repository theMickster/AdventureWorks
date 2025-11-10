using Aspire.Hosting.Lifecycle;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace AdventureWorks.AppHost;

// Represents a Docker container whose lifecycle is managed externally (e.g. OrbStack).
// Aspire shows it as a resource tile but never starts, stops, or creates it.
internal sealed class ExternalContainerResource(string name) : Resource(name);

// On startup, transitions all ExternalContainerResource tiles to Running state so the
// health check badge is displayed (Aspire only polls health for Running resources).
internal sealed class ExternalContainerLifecycleHook(ResourceNotificationService notifications)
    : IDistributedApplicationLifecycleHook
{
    async Task IDistributedApplicationLifecycleHook.AfterResourcesCreatedAsync(
        DistributedApplicationModel model,
        CancellationToken cancellationToken)
    {
        foreach (var resource in model.Resources.OfType<ExternalContainerResource>())
        {
            await notifications.PublishUpdateAsync(resource, state => state with
            {
                State = new ResourceStateSnapshot(KnownResourceStates.Running, null)
            });
        }
    }
}

// Probes a TCP port to determine whether the container is accepting connections.
// Healthy = port open (container running), Unhealthy = port closed/refused.
internal sealed class TcpPortHealthCheck(string host, int port) : IHealthCheck
{
    async Task<HealthCheckResult> IHealthCheck.CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken)
    {
        try
        {
            using var client = new System.Net.Sockets.TcpClient();
            await client.ConnectAsync(host, port, cancellationToken);
            return HealthCheckResult.Healthy($"Reachable at {host}:{port}");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy($"Not reachable at {host}:{port}", ex);
        }
    }
}
