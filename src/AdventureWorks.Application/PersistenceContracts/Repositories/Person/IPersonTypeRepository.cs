using AdventureWorks.Domain.Entities.Person;

namespace AdventureWorks.Application.PersistenceContracts.Repositories.Person;

public  interface IPersonTypeRepository : IReadOnlyAsyncRepository<PersonTypeEntity>
{

}
