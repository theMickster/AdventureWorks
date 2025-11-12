using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace AdventureWorks.API.Hubs;

/// <summary>
/// SignalR hub for real-time dashboard push events.
/// Clients subscribe to the "Dashboard" group to receive EntityChanged notifications.
/// </summary>
[Authorize(Policy = "DashboardAccess")]
public sealed class DashboardHub(ILogger<DashboardHub> logger) : Hub
{
    private readonly ILogger<DashboardHub> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    /// <summary>Adds the caller's connection to the Dashboard broadcast group.</summary>
    public async Task SubscribeToDashboard(CancellationToken cancellationToken = default)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, DashboardHubConstants.DashboardGroup, cancellationToken);
        _logger.LogInformation("Connection {ConnectionId} subscribed to Dashboard (User: {User}).",
            Context.ConnectionId, Context.User?.Identity?.Name);
    }

    /// <summary>Removes the caller's connection from the Dashboard broadcast group.</summary>
    public async Task UnsubscribeFromDashboard(CancellationToken cancellationToken = default)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, DashboardHubConstants.DashboardGroup, cancellationToken);
        _logger.LogInformation("Connection {ConnectionId} unsubscribed from Dashboard (User: {User}).",
            Context.ConnectionId, Context.User?.Identity?.Name);
    }

    /// <inheritdoc/>
    public override async Task OnConnectedAsync()
    {
        _logger.LogInformation("SignalR connected: ConnectionId={ConnectionId}, User={User}.",
            Context.ConnectionId, Context.User?.Identity?.Name);
        await base.OnConnectedAsync();
    }

    /// <inheritdoc/>
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, DashboardHubConstants.DashboardGroup);
        _logger.LogInformation("SignalR disconnected: ConnectionId={ConnectionId}, User={User}.",
            Context.ConnectionId, Context.User?.Identity?.Name);
        await base.OnDisconnectedAsync(exception);
    }
}
