using AdventureWorks.SalesOrderSaga.Contracts;

namespace AdventureWorks.SalesOrderSaga.TestHarness;

public enum AuthorizationState { Pending, Approved, Declined }

public sealed record SimulatedAuthorization(
    int SalesOrderId,
    string IdempotencyKey,
    string AuthorizationRequestId,
    decimal Amount,
    string Currency,
    IReadOnlyList<SagaOrderLine> Lines,
    AuthorizationState State,
    string? AuthorizationCode,
    string? DeclineReason,
    DateTimeOffset ReceivedAt,
    DateTimeOffset? DecidedAt);

public sealed record CreateOrderRequest(short Quantity = 1, decimal UnitPrice = 2024.994m);
public sealed record PaymentDecisionRequest(string? AuthorizationCode = null, string? Reason = null);
public sealed record FixtureOrder(int SalesOrderId, int CustomerId, DateTimeOffset OrderDate, int ProductId, short Quantity, decimal UnitPrice, byte InitialStatus, byte InitialRevision, int InitialInventory, string Comment)
{
    public OrderCreatedEvent ToEvent() => new(
        SalesOrderId, CustomerId, OrderDate, [new SagaOrderLine(ProductId, Quantity, UnitPrice)]);
}

public sealed record SagaEvidence(
    int SalesOrderId,
    byte? OrderStatus,
    byte? RevisionNumber,
    int Inventory,
    int AllocationCount,
    int ReversedAllocationCount,
    string? OutboxEventName,
    string? OutboxPayload,
    DateTime? OutboxDispatchedAt,
    int DispatchAttemptCount);

public sealed record SagaStatusDocument(string RuntimeStatus, string? SagaStatus, string? FailureReason);
public sealed record ScenarioAssertion(string Name, bool Passed, string Expected, string Actual);
public sealed record ScenarioReport(int SalesOrderId, string Scenario, bool Passed, string? FailureStage, long ElapsedMilliseconds, IReadOnlyList<ScenarioAssertion> Assertions, SagaStatusDocument? DurableStatus, SagaEvidence? Evidence);

public sealed class HarnessConflictException(string message) : Exception(message);
public sealed class HarnessNotReadyException(string message) : Exception(message);
