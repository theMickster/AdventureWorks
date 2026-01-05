using System.Collections.Concurrent;
using AdventureWorks.SalesOrderSaga.Contracts;

namespace AdventureWorks.SalesOrderSaga.TestHarness;

public interface IAuthorizationStore
{
    PaymentAuthorizationAcknowledgement Receive(PaymentAuthorizationRequest request, string? headerKey);
    IReadOnlyCollection<SimulatedAuthorization> List();
    SimulatedAuthorization Decide(int salesOrderId, AuthorizationState decision, string? detail);
    SimulatedAuthorization? Find(int salesOrderId);
}

public sealed class AuthorizationStore : IAuthorizationStore
{
    private readonly ConcurrentDictionary<int, SimulatedAuthorization> _items = new();

    public PaymentAuthorizationAcknowledgement Receive(PaymentAuthorizationRequest request, string? headerKey)
    {
        if (request.SalesOrderId <= 0 || request.Amount <= 0 || string.IsNullOrWhiteSpace(request.Currency) || request.Lines is null || request.Lines.Count == 0)
            throw new ArgumentException("SalesOrderId, currency, a positive amount, and at least one line are required.");
        if (request.Lines.Any(line => line.ProductId <= 0 || line.OrderQty <= 0 || line.UnitPrice < 0) ||
            request.Amount != request.Lines.Sum(line => line.OrderQty * line.UnitPrice))
            throw new ArgumentException("Authorization lines must be valid and add up to the request amount.");
        if (string.IsNullOrWhiteSpace(headerKey) || !string.Equals(headerKey, request.IdempotencyKey, StringComparison.Ordinal))
            throw new ArgumentException("Idempotency-Key must be present and match the request body.");
        if (!string.Equals(request.IdempotencyKey, SagaIds.InstanceIdFor(request.SalesOrderId), StringComparison.Ordinal))
            throw new ArgumentException("The idempotency key must be the deterministic saga instance ID.");

        var received = new SimulatedAuthorization(
            request.SalesOrderId, request.IdempotencyKey, $"local-auth-{request.SalesOrderId}", request.Amount,
            request.Currency, request.Lines.ToArray(), AuthorizationState.Pending, null, null, DateTimeOffset.UtcNow, null);
        var stored = _items.AddOrUpdate(request.SalesOrderId, received, (_, existing) =>
        {
            if (existing.IdempotencyKey != request.IdempotencyKey || existing.Amount != request.Amount || existing.Currency != request.Currency ||
                !SameLines(existing, request))
                throw new HarnessConflictException("A different authorization request already exists for this sales order.");
            return existing;
        });
        return new PaymentAuthorizationAcknowledgement(stored.AuthorizationRequestId);
    }

    private static bool SameLines(SimulatedAuthorization existing, PaymentAuthorizationRequest request) =>
        existing.Lines.SequenceEqual(request.Lines);

    public IReadOnlyCollection<SimulatedAuthorization> List() => _items.Values.OrderBy(x => x.SalesOrderId).ToArray();
    public SimulatedAuthorization? Find(int salesOrderId) => _items.GetValueOrDefault(salesOrderId);

    public SimulatedAuthorization Decide(int salesOrderId, AuthorizationState decision, string? detail)
    {
        if (decision == AuthorizationState.Pending) throw new ArgumentOutOfRangeException(nameof(decision));
        while (true)
        {
            if (!_items.TryGetValue(salesOrderId, out var current))
                throw new HarnessNotReadyException("No pending authorization has been received for this sales order.");
            if (current.State != AuthorizationState.Pending)
            {
                if (current.State == decision) return current;
                throw new HarnessConflictException($"Authorization is already {current.State}; the decision cannot be changed.");
            }
            var updated = current with
            {
                State = decision,
                AuthorizationCode = decision == AuthorizationState.Approved ? detail ?? "local-approved" : null,
                DeclineReason = decision == AuthorizationState.Declined ? detail ?? "manual smoke decline" : null,
                DecidedAt = DateTimeOffset.UtcNow
            };
            if (_items.TryUpdate(salesOrderId, updated, current)) return updated;
        }
    }
}
