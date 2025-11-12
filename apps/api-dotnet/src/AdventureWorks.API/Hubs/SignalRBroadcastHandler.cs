using AdventureWorks.Application.Features.Dashboard.Notifications;
using MediatR;
using Microsoft.AspNetCore.SignalR;

namespace AdventureWorks.API.Hubs;

/// <summary>
/// Handles EntityChangedNotification by broadcasting an "EntityChanged" event
/// to all clients subscribed to the Dashboard SignalR group.
/// </summary>
public sealed class SignalRBroadcastHandler(
    IHubContext<DashboardHub> hubContext,
    ILogger<SignalRBroadcastHandler> logger)
    : INotificationHandler<EntityChangedNotification>
{
    private readonly IHubContext<DashboardHub> _hubContext = hubContext ?? throw new ArgumentNullException(nameof(hubContext));
    private readonly ILogger<SignalRBroadcastHandler> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    /// <summary>Broadcasts the notification to all Dashboard group subscribers; logs and swallows errors so a failed push never blocks the command pipeline.</summary>
    public async Task Handle(EntityChangedNotification notification, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(notification);

        try
        {
            // UserName is intentionally included in the broadcast payload to support
            // activity attribution on the dashboard. All dashboard subscribers are
            // internal employees of the same organization.
            await _hubContext.Clients
                .Group(DashboardHubConstants.DashboardGroup)
                .SendAsync(DashboardHubConstants.EntityChangedEvent, notification, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to broadcast {Event} for {EntityType} {EntityId}.",
                DashboardHubConstants.EntityChangedEvent, notification.EntityType, notification.EntityId);
        }
    }
}
