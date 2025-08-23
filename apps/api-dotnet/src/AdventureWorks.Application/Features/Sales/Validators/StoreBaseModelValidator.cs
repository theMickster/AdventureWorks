using AdventureWorks.Application.PersistenceContracts.Repositories.Sales;
using AdventureWorks.Models.Features.Sales;
using FluentValidation;

namespace AdventureWorks.Application.Features.Sales.Validators;

public class StoreBaseModelValidator<T> : AbstractValidator<T> where T : StoreBaseModel
{
    public StoreBaseModelValidator(ISalesPersonRepository salesPersonRepository)
    {
        ArgumentNullException.ThrowIfNull(salesPersonRepository);

        RuleFor(a => a.Name)
            .NotEmpty()
            .WithErrorCode("Rule-01").WithMessage(MessageStoreNameEmpty)
            .MaximumLength(50)
            .WithErrorCode("Rule-02").WithMessage(MessageStoreNameLength);

        RuleFor(a => a.SalesPersonId)
            .MustAsync(async (id, ct) => id == null || await salesPersonRepository.ExistsAsync(id.Value, ct))
            .WithErrorCode("Rule-03").WithMessage(MessageSalesPersonIdInvalid);
    }

    public static string MessageStoreNameEmpty => "Store name cannot be null, empty, or whitespace";
    public static string MessageStoreNameLength => "Store name cannot be greater than 50 characters";
    public static string MessageSalesPersonIdInvalid => "The specified sales person does not exist.";
}
