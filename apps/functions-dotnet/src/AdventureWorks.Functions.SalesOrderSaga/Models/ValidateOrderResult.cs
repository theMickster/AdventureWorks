namespace AdventureWorks.Functions.SalesOrderSaga.Models;

/// <summary>
/// Output of <c>ValidateOrderActivity</c> — the outcome of running
/// <c>SalesOrderSagaInputValidator</c> against the saga input. <see cref="Errors"/> is empty
/// when <see cref="IsValid"/> is <see langword="true"/>.
/// </summary>
public sealed record ValidateOrderResult(bool IsValid, IReadOnlyList<string> Errors)
{
    public static ValidateOrderResult Success() => new(true, []);

    public static ValidateOrderResult Failure(IReadOnlyList<string> errors) => new(false, errors);
}
