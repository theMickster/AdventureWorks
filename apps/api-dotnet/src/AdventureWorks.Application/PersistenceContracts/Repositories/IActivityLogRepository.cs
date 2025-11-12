using AdventureWorks.Domain.Entities.Dashboard;

namespace AdventureWorks.Application.PersistenceContracts.Repositories;

public interface IActivityLogRepository
{
    Task AddAsync(ActivityLogEntity entry, CancellationToken cancellationToken = default);
}
