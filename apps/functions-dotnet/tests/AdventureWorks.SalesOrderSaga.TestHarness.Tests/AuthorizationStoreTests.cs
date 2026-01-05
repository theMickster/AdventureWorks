using AdventureWorks.SalesOrderSaga.Contracts;
using AdventureWorks.SalesOrderSaga.TestHarness;

namespace AdventureWorks.SalesOrderSaga.TestHarness.Tests;

public sealed class AuthorizationStoreTests
{
    private static PaymentAuthorizationRequest Request(int id = 900000) => new(
        id, 10m, "USD", SagaIds.InstanceIdFor(id), [new SagaOrderLine(776, 1, 10m)]);

    [Fact]
    public void Receive_requires_matching_idempotency_key()
    {
        var store = new AuthorizationStore();
        Assert.Throws<ArgumentException>(() => store.Receive(Request(), "wrong"));
    }

    [Fact]
    public void Receive_rejects_an_amount_that_does_not_match_lines()
    {
        var store = new AuthorizationStore();
        var request = Request() with { Amount = 11m };
        Assert.Throws<ArgumentException>(() => store.Receive(request, request.IdempotencyKey));
    }

    [Fact]
    public void Duplicate_request_returns_stable_acknowledgement()
    {
        var store = new AuthorizationStore();
        var request = Request();
        Assert.Equal(store.Receive(request, request.IdempotencyKey), store.Receive(request, request.IdempotencyKey));
        Assert.Single(store.List());
    }

    [Fact]
    public void Identical_decision_is_idempotent_but_conflicting_decision_is_rejected()
    {
        var store = new AuthorizationStore();
        var request = Request();
        store.Receive(request, request.IdempotencyKey);
        var first = store.Decide(request.SalesOrderId, AuthorizationState.Approved, "local-approved");
        Assert.Equal(first, store.Decide(request.SalesOrderId, AuthorizationState.Approved, "ignored"));
        Assert.Throws<HarnessConflictException>(() => store.Decide(request.SalesOrderId, AuthorizationState.Declined, null));
    }

    [Fact]
    public void Decision_requires_pending_authorization() =>
        Assert.Throws<HarnessNotReadyException>(() => new AuthorizationStore().Decide(900000, AuthorizationState.Approved, null));

    [Fact]
    public void Event_identifiers_are_deterministic()
    {
        Assert.Equal("sales-order-saga-900001", SagaIds.InstanceIdFor(900001));
        Assert.Equal("PaymentApproved:sales-order-saga-900001", SagaIds.MessageId(SagaEventNames.PaymentApproved, 900001));
    }

    [Fact]
    public void Order_ids_are_monotonic_even_if_database_fixtures_are_removed()
    {
        var allocator = new MonotonicOrderIdAllocator();
        Assert.Equal(900000, allocator.Next(75123));
        Assert.Equal(900001, allocator.Next(0));
        Assert.Equal(910001, allocator.Next(910000));
    }
}
