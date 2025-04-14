using AdventureWorks.Domain.Entities;

namespace AdventureWorks.Application.PersistenceContracts.Repositories.Person;
public interface IBusinessEntityRepository : IAsyncRepository<BusinessEntity>
{
}
