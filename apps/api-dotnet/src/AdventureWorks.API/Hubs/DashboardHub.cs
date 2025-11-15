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

    /// <inheritdoc/>
    public override async Task OnConnectedAsync()
    {
        // Auto-subscribe every authenticated connection to the Dashboard group.
        // withAutomaticReconnect() on the JS client creates a new ConnectionId on each reconnect,
        // so OnConnectedAsync fires again and re-subscribes without any client-side invoke needed.
        await Groups.AddToGroupAsync(Context.ConnectionId, DashboardHubConstants.DashboardGroup, Context.ConnectionAborted);
        _logger.LogInformation("SignalR connected: ConnectionId={ConnectionId}, User={User}.",
            Context.ConnectionId, Context.User?.Identity?.Name);
        await base.OnConnectedAsync();
    }

    /// <inheritdoc/>
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, DashboardHubConstants.DashboardGroup, Context.ConnectionAborted);
        _logger.LogInformation("SignalR disconnected: ConnectionId={ConnectionId}, User={User}.",
            Context.ConnectionId, Context.User?.Identity?.Name);
        await base.OnDisconnectedAsync(exception);
    }
}
