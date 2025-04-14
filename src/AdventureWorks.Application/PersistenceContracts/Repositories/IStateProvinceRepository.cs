using AdventureWorks.Domain.Entities;

namespace AdventureWorks.Application.PersistenceContracts.Repositories;

public interface IStateProvinceRepository : IReadOnlyAsyncRepository<StateProvinceEntity>
{
}