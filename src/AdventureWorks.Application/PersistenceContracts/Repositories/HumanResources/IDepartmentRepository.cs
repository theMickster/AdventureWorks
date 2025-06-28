using AdventureWorks.Domain.Entities;

namespace AdventureWorks.Application.PersistenceContracts.Repositories.HumanResources;

public interface IDepartmentRepository : IReadOnlyAsyncRepository<DepartmentEntity>
{

}
