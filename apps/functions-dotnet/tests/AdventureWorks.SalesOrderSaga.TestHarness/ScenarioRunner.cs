using System.Diagnostics;
using AdventureWorks.SalesOrderSaga.Contracts;

namespace AdventureWorks.SalesOrderSaga.TestHarness;

public sealed class ScenarioRunner(
    IFixtureStore fixtures,
    ISagaEventPublisher publisher,
    IAuthorizationStore authorizations,
    ISagaStatusClient statusClient)
{
    private static readonly TimeSpan Timeout = TimeSpan.FromMinutes(2);

    public async Task<ScenarioReport> RunAsync(AuthorizationState decision, CancellationToken cancellationToken)
    {
        var watch = Stopwatch.StartNew();
        FixtureOrder? fixture = null;
        SagaStatusDocument? durable = null;
        SagaEvidence? evidence = null;
        var assertions = new List<ScenarioAssertion>();
        var stage = "create fixture";
        try
        {
            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeout.CancelAfter(Timeout);
            fixture = await fixtures.CreateAsync(new CreateOrderRequest(), timeout.Token);
            stage = "publish OrderCreated";
            await publisher.PublishOrderCreatedAsync(fixture, timeout.Token);
            stage = "wait for authorization";
            await PollAsync(() => Task.FromResult(authorizations.Find(fixture.SalesOrderId) is { State: AuthorizationState.Pending }), timeout.Token);
            stage = $"publish payment {decision}";
            var authorization = authorizations.Decide(fixture.SalesOrderId, decision, decision == AuthorizationState.Approved ? "local-approved" : "manual smoke decline");
            await publisher.PublishPaymentResultAsync(authorization, timeout.Token);
            stage = "wait for Durable completion";
            await PollAsync(async () =>
            {
                durable = await statusClient.GetAsync(fixture.SalesOrderId, timeout.Token);
                return string.Equals(durable?.RuntimeStatus, "Completed", StringComparison.OrdinalIgnoreCase);
            }, timeout.Token);
            stage = "wait for outbox dispatch";
            await PollAsync(async () =>
            {
                evidence = await fixtures.GetEvidenceAsync(fixture.SalesOrderId, timeout.Token);
                return evidence?.OutboxDispatchedAt is not null;
            }, timeout.Token);

            Add(assertions, "durable runtime", durable?.RuntimeStatus == "Completed", "Completed", durable?.RuntimeStatus);
            if (decision == AuthorizationState.Approved)
            {
                Add(assertions, "saga result", durable?.SagaStatus == "Approved", "Approved", durable?.SagaStatus);
                Add(assertions, "order status", evidence?.OrderStatus == 5, "5", evidence?.OrderStatus);
                Add(assertions, "revision increment", evidence?.RevisionNumber == fixture.InitialRevision + 1, (fixture.InitialRevision + 1).ToString(), evidence?.RevisionNumber);
                Add(assertions, "inventory reserved", evidence?.Inventory == fixture.InitialInventory - fixture.Quantity, (fixture.InitialInventory - fixture.Quantity).ToString(), evidence?.Inventory);
                Add(assertions, "allocation unreversed", evidence is { AllocationCount: > 0, ReversedAllocationCount: 0 }, ">0 allocations, 0 reversed", evidence is null ? null : $"{evidence.AllocationCount} allocations, {evidence.ReversedAllocationCount} reversed");
                Add(assertions, "approval outbox", evidence?.OutboxEventName == SagaEventNames.OrderApproved && PayloadHasApprovalData(evidence.OutboxPayload, fixture.SalesOrderId), "dispatched OrderApproved with order, line, address and ship method", evidence?.OutboxEventName);
            }
            else
            {
                Add(assertions, "saga result", durable?.SagaStatus == "PaymentDeclined", "PaymentDeclined", durable?.SagaStatus);
                Add(assertions, "order unchanged", evidence?.OrderStatus == fixture.InitialStatus && evidence?.RevisionNumber == fixture.InitialRevision, $"status {fixture.InitialStatus}, revision {fixture.InitialRevision}", evidence is null ? null : $"status {evidence.OrderStatus}, revision {evidence.RevisionNumber}");
                Add(assertions, "inventory restored", evidence?.Inventory == fixture.InitialInventory, fixture.InitialInventory.ToString(), evidence?.Inventory);
                Add(assertions, "allocations reversed once", evidence is { AllocationCount: > 0 } && evidence.AllocationCount == evidence.ReversedAllocationCount, "all allocations reversed", evidence is null ? null : $"{evidence.ReversedAllocationCount}/{evidence.AllocationCount}");
                Add(assertions, "failure outbox", evidence?.OutboxEventName == SagaEventNames.OrderFailed && evidence.OutboxPayload?.Contains("manual smoke decline", StringComparison.OrdinalIgnoreCase) == true && evidence.OutboxPayload.Contains("PaymentDeclined", StringComparison.Ordinal), "dispatched OrderFailed with PaymentDeclined and reason", evidence?.OutboxEventName);
            }
            return new(fixture.SalesOrderId, decision.ToString(), assertions.All(x => x.Passed), assertions.All(x => x.Passed) ? null : "assertions", watch.ElapsedMilliseconds, assertions, durable, evidence);
        }
        catch (Exception exception)
        {
            if (fixture is not null) evidence ??= await TryEvidenceAsync(fixture.SalesOrderId);
            assertions.Add(new(stage, false, "completed", exception.Message));
            return new(fixture?.SalesOrderId ?? 0, decision.ToString(), false, stage, watch.ElapsedMilliseconds, assertions, durable, evidence);
        }
    }

    private async Task<SagaEvidence?> TryEvidenceAsync(int id)
    {
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        try { return await fixtures.GetEvidenceAsync(id, timeout.Token); }
        catch { return null; }
    }

    private static async Task PollAsync(Func<Task<bool>> predicate, CancellationToken cancellationToken)
    {
        while (!await predicate()) await Task.Delay(500, cancellationToken);
    }

    private static bool PayloadHasApprovalData(string? payload, int id) =>
        payload?.Contains($"\"SalesOrderId\":{id}", StringComparison.OrdinalIgnoreCase) == true &&
        payload.Contains("\"Lines\"", StringComparison.OrdinalIgnoreCase) &&
        payload.Contains("\"ShipTo\"", StringComparison.OrdinalIgnoreCase) &&
        payload.Contains("\"ShipMethod\"", StringComparison.OrdinalIgnoreCase);

    private static void Add(List<ScenarioAssertion> assertions, string name, bool passed, string expected, object? actual) =>
        assertions.Add(new(name, passed, expected, actual?.ToString() ?? "null"));
}
