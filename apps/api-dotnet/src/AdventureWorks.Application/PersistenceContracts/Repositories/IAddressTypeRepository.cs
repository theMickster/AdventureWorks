using AdventureWorks.Domain.Entities;
using AdventureWorks.Domain.Entities.Person;

namespace AdventureWorks.Application.PersistenceContracts.Repositories;

public interface IAddressTypeRepository : IReadOnlyAsyncRepository<AddressTypeEntity>
{

}
