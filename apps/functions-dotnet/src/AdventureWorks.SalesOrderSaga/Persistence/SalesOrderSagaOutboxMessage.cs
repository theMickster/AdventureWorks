namespace AdventureWorks.SalesOrderSaga.Persistence;

/// <summary>A durable terminal-event record awaiting independent Service Bus delivery.</summary>
public sealed class SalesOrderSagaOutboxMessage
{
    public string MessageId { get; set; } = string.Empty;
    public string EventName { get; set; } = string.Empty;
    public string Payload { get; set; } = string.Empty;
    public DateTime OccurredAt { get; set; }
    public DateTime? DispatchedAt { get; set; }
    public int DispatchAttemptCount { get; set; }
    public string? LastDispatchError { get; set; }
}
