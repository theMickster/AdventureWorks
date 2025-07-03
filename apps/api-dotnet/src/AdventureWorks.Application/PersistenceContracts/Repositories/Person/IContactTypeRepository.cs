using AdventureWorks.Domain.Entities.Person;

namespace AdventureWorks.Application.PersistenceContracts.Repositories.Person;

public interface IContactTypeRepository : IReadOnlyAsyncRepository<ContactTypeEntity>
{

}
