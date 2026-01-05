using AdventureWorks.SalesOrderSaga.Contracts;
using AdventureWorks.SalesOrderSaga.TestHarness;

namespace AdventureWorks.SalesOrderSaga.TestHarness.Tests;

public sealed class ScenarioRunnerTests
{
    [Theory]
    [InlineData(AuthorizationState.Approved)]
    [InlineData(AuthorizationState.Declined)]
    public async Task Complete_scenario_evaluates_every_required_assertion(AuthorizationState decision)
    {
        var authorizations = new AuthorizationStore();
        var state = new FakeState(authorizations);
        var runner = new ScenarioRunner(state, state, authorizations, state);

        var report = await runner.RunAsync(decision, TestContext.Current.CancellationToken);

        Assert.True(report.Passed);
        Assert.Null(report.FailureStage);
        Assert.Equal(decision == AuthorizationState.Approved ? 7 : 6, report.Assertions.Count);
        Assert.All(report.Assertions, assertion => Assert.True(assertion.Passed, assertion.Name));
    }

    private sealed class FakeState(IAuthorizationStore authorizations) : IFixtureStore, ISagaEventPublisher, ISagaStatusClient
    {
        private readonly FixtureOrder _fixture = new(900000, 29825, DateTimeOffset.UtcNow, 776, 1, 2024.994m, 1, 0, 100, "AW-SAGA-HARNESS:test");
        private AuthorizationState _state = AuthorizationState.Pending;

        public Task<FixtureOrder> CreateAsync(CreateOrderRequest request, CancellationToken cancellationToken) => Task.FromResult(_fixture);
        public Task<int> CleanupAsync(CancellationToken cancellationToken) => Task.FromResult(1);

        public Task PublishOrderCreatedAsync(FixtureOrder order, CancellationToken cancellationToken)
        {
            var request = new PaymentAuthorizationRequest(order.SalesOrderId, order.Quantity * order.UnitPrice, "USD", SagaIds.InstanceIdFor(order.SalesOrderId), order.ToEvent().Lines);
            authorizations.Receive(request, request.IdempotencyKey);
            return Task.CompletedTask;
        }

        public Task PublishPaymentResultAsync(SimulatedAuthorization authorization, CancellationToken cancellationToken)
        {
            _state = authorization.State;
            return Task.CompletedTask;
        }

        public Task<SagaStatusDocument?> GetAsync(int salesOrderId, CancellationToken cancellationToken) => Task.FromResult<SagaStatusDocument?>(
            new("Completed", _state == AuthorizationState.Approved ? "Approved" : "PaymentDeclined", _state == AuthorizationState.Declined ? "manual smoke decline" : null));

        public Task<SagaEvidence?> GetEvidenceAsync(int salesOrderId, CancellationToken cancellationToken)
        {
            var approved = _state == AuthorizationState.Approved;
            var payload = approved
                ? $"{{\"SalesOrderId\":{salesOrderId},\"Lines\":[{{}}],\"ShipTo\":{{}},\"ShipMethod\":{{}}}}"
                : $"{{\"SalesOrderId\":{salesOrderId},\"Status\":\"PaymentDeclined\",\"Reason\":\"manual smoke decline\"}}";
            return Task.FromResult<SagaEvidence?>(new(salesOrderId, approved ? (byte)5 : (byte)1, approved ? (byte)1 : (byte)0, approved ? 99 : 100, 1, approved ? 0 : 1, approved ? SagaEventNames.OrderApproved : SagaEventNames.OrderFailed, payload, DateTime.UtcNow, 1));
        }
    }
}
