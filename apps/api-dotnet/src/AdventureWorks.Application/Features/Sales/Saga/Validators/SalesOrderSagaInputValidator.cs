using AdventureWorks.Application.Features.Sales.Saga.Models;
using FluentValidation;

namespace AdventureWorks.Application.Features.Sales.Saga.Validators;

/// <summary>
/// Validates a <see cref="SalesOrderSagaInput"/> before the sales order saga orchestrator
/// (<c>apps/functions-dotnet</c>) checks inventory or reserves stock. Run by
/// <c>ValidateOrderActivity</c> as the first saga step; a failed validation short-circuits the
/// orchestration before any stock is touched.
/// </summary>
public sealed class SalesOrderSagaInputValidator : AbstractValidator<SalesOrderSagaInput>
{
    public SalesOrderSagaInputValidator()
    {
        RuleFor(x => x.CustomerId)
            .GreaterThan(0)
            .WithErrorCode("Rule-01").WithMessage(MessageCustomerIdInvalid);

        RuleFor(x => x.Lines)
            .NotEmpty()
            .WithErrorCode("Rule-02").WithMessage(MessageLinesEmpty);

        RuleForEach(x => x.Lines).ChildRules(line =>
        {
            line.RuleFor(x => x.ProductId)
                .GreaterThan(0)
                .WithErrorCode("Rule-03").WithMessage(MessageProductIdInvalid);

            line.RuleFor(x => x.OrderQty)
                .GreaterThan((short)0)
                .WithErrorCode("Rule-04").WithMessage(MessageOrderQtyInvalid);
        });
    }

    public static string MessageCustomerIdInvalid => "CustomerId must be greater than 0";

    public static string MessageLinesEmpty => "Order must contain at least one line item";

    public static string MessageProductIdInvalid => "ProductId must be greater than 0";

    public static string MessageOrderQtyInvalid => "OrderQty must be greater than 0";
}
