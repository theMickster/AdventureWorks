namespace AdventureWorks.SalesOrderSaga.Models;

/// <summary>Serialized event queued for independent Service Bus delivery.</summary>
public sealed record SagaEventPublication(string EventName, string MessageId, string Payload);

/// <summary>Input to the replay-safe compensation activity.</summary>
public sealed record ReleaseStockRequest(int SalesOrderId, string SagaInstanceId, SalesOrderSagaStatus FailureStatus, string? FailureReason);

/// <summary>Input used to confirm an order and atomically enqueue its approval notification.</summary>
public sealed record ConfirmOrderRequest(int SalesOrderId, string SagaInstanceId);

/// <summary>Result of a compensation attempt.</summary>
public sealed record ReleaseStockResult(bool AlreadyCompensated, int ReleasedAllocationCount);
