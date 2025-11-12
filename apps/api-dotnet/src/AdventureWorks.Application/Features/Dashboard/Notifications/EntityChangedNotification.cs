using MediatR;

namespace AdventureWorks.Application.Features.Dashboard.Notifications;

/// <summary>Published by command handlers after a successful entity mutation; consumed by SignalRBroadcastHandler and ActivityLogNotificationHandler.</summary>
public sealed class EntityChangedNotification : INotification
{
    public required string EntityType { get; init; }
    public required int EntityId { get; init; }
    public required string Action { get; init; }
    public required string UserName { get; init; }
    public required DateTime Timestamp { get; init; }
}
