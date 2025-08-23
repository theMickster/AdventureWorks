using AdventureWorks.Application.PersistenceContracts.Repositories.Sales;
using AdventureWorks.Models.Features.Sales;

namespace AdventureWorks.Application.Features.Sales.Validators;

public sealed class UpdateStoreValidator : StoreBaseModelValidator<StoreUpdateModel>
{
    public UpdateStoreValidator(ISalesPersonRepository salesPersonRepository) : base(salesPersonRepository)
    {
    }
}
