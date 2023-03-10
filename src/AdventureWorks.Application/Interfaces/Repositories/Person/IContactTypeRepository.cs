using AdventureWorks.Domain.Entities.Person;

namespace AdventureWorks.Application.Interfaces.Repositories.Person;

public interface IContactTypeRepository : IReadOnlyAsyncRepository<ContactTypeEntity>
{

}
