using AdventureWorks.Models.Features.Sales;
using FluentValidation;

namespace AdventureWorks.Application.Features.Sales.Validators;

public class StoreBaseModelValidator<T> : AbstractValidator<T> where T : StoreBaseModel
{
    public StoreBaseModelValidator()
    {
        RuleFor(a => a.Name)
            .NotEmpty()
            .WithErrorCode("Rule-01").WithMessage(MessageStoreNameEmpty)
            .MaximumLength(60)
            .WithErrorCode("Rule-02").WithMessage(MessageStoreNameLength);
    }

    public static string MessageStoreNameEmpty => "Store name cannot be null, empty, or whitespace";

    public static string MessageStoreNameLength => "Store name cannot be greater than 50 characters";
}
