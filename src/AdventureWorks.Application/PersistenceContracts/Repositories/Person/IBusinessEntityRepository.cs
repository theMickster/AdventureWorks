using AdventureWorks.Domain.Entities;
using AdventureWorks.Domain.Entities.Person;

namespace AdventureWorks.Application.PersistenceContracts.Repositories.Person;
public interface IBusinessEntityRepository : IAsyncRepository<BusinessEntity>
{
}
