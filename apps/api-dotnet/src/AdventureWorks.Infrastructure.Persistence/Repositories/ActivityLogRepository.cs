using AdventureWorks.Application.PersistenceContracts.Repositories;
using AdventureWorks.Common.Attributes;
using AdventureWorks.Domain.Entities.Dashboard;
using AdventureWorks.Infrastructure.Persistence.DbContexts;

namespace AdventureWorks.Infrastructure.Persistence.Repositories;

[ServiceLifetimeScoped]
public sealed class ActivityLogRepository(AdventureWorksDbContext context) : IActivityLogRepository
{
    private readonly AdventureWorksDbContext _context = context ?? throw new ArgumentNullException(nameof(context));

    public async Task AddAsync(ActivityLogEntity entry, CancellationToken cancellationToken = default)
    {
        await _context.ActivityLogs.AddAsync(entry, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
