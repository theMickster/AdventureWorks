using AdventureWorks.Domain.Entities.HumanResources;

namespace AdventureWorks.Application.PersistenceContracts.Repositories.HumanResources;

public interface IDepartmentRepository : IReadOnlyAsyncRepository<DepartmentEntity>
{

}
