using AdventureWorks.Application.PersistenceContracts.Repositories.Sales;
using AdventureWorks.Models.Features.Sales;

namespace AdventureWorks.Application.Features.Sales.Validators;

public sealed class CreateStoreValidator : StoreBaseModelValidator<StoreCreateModel>
{
    public CreateStoreValidator(ISalesPersonRepository salesPersonRepository) : base(salesPersonRepository)
    {
    }
}
