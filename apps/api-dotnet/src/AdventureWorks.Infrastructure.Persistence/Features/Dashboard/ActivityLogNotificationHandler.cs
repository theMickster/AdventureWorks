using AdventureWorks.Application.Features.Dashboard.Notifications;
using AdventureWorks.Application.PersistenceContracts.Repositories;
using AdventureWorks.Domain.Entities.Dashboard;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AdventureWorks.Infrastructure.Persistence.Features.Dashboard;

/// <summary>Persists EntityChangedNotification events to the ActivityLog table; errors are logged and swallowed so a DB failure never blocks the command pipeline.</summary>
public sealed class ActivityLogNotificationHandler(
    IActivityLogRepository repository,
    ILogger<ActivityLogNotificationHandler> logger)
    : INotificationHandler<EntityChangedNotification>
{
    private readonly IActivityLogRepository _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    private readonly ILogger<ActivityLogNotificationHandler> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    /// <summary>Maps the notification to an ActivityLogEntity and persists it via IActivityLogRepository.</summary>
    public async Task Handle(EntityChangedNotification notification, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(notification);

        try
        {
            var entry = new ActivityLogEntity
            {
                EntityType = notification.EntityType.Length > 100
                    ? notification.EntityType[..100]
                    : notification.EntityType,
                EntityId = notification.EntityId,
                Action = notification.Action.Length > 50
                    ? notification.Action[..50]
                    : notification.Action,
                UserName = notification.UserName.Length > 256
                    ? notification.UserName[..256]
                    : notification.UserName,
                Timestamp = notification.Timestamp
            };

            await _repository.AddAsync(entry, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to write ActivityLog entry for {EntityType} {EntityId} {Action}.",
                notification.EntityType, notification.EntityId, notification.Action);
        }
    }
}
