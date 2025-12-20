using AdventureWorks.Application.Features.Sales.Saga.Models;
using AdventureWorks.Application.Features.Sales.Saga.Validators;
using AdventureWorks.Functions.SalesOrderSaga.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask;

namespace AdventureWorks.Functions.SalesOrderSaga.Activities;

/// <summary>
/// Core validation logic for the first sales order saga step — no SQL access, no I/O. Runs
/// <see cref="SalesOrderSagaInputValidator"/> (shared package) against the saga input. Kept as
/// a plain <see cref="TaskActivity{TInput,TResult}"/> so it can be driven directly by
/// <c>Microsoft.DurableTask.InProcessTestHost</c> in addition to Moq.
/// </summary>
public sealed class ValidateOrderActivityCore : TaskActivity<SalesOrderSagaInput, ValidateOrderResult>
{
    private static readonly SalesOrderSagaInputValidator Validator = new();

    public override async Task<ValidateOrderResult> RunAsync(TaskActivityContext context, SalesOrderSagaInput? input)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(input);

        var validationResult = await Validator.ValidateAsync(input);

        return validationResult.IsValid
            ? ValidateOrderResult.Success()
            : ValidateOrderResult.Failure(validationResult.Errors.Select(e => e.ErrorMessage).ToList());
    }
}

/// <summary>
/// Thin <c>[Function]</c> adapter for <see cref="ValidateOrderActivityCore"/> — the sales order
/// saga's first step. Called by <c>SalesOrderSagaOrchestrator</c> before any inventory check or
/// stock reservation is attempted.
/// </summary>
public static class ValidateOrderActivity
{
    [Function(nameof(ValidateOrderActivity))]
    public static Task<ValidateOrderResult> RunAsync([ActivityTrigger] SalesOrderSagaInput input) =>
        new ValidateOrderActivityCore().RunAsync(new FunctionsTaskActivityContext(nameof(ValidateOrderActivity)), input);
}
