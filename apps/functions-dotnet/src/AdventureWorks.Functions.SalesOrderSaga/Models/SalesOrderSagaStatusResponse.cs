namespace AdventureWorks.Functions.SalesOrderSaga.Models;

/// <summary>
/// JSON shape returned by <c>SalesOrderSagaStatusFunction</c> — the durable instance's runtime
/// status plus its terminal <see cref="SalesOrderSagaResult"/> once completed (<see langword="null"/>
/// while the saga is still running).
/// </summary>
public sealed record SalesOrderSagaStatusResponse(string InstanceId, string RuntimeStatus, SalesOrderSagaResult? Result);
