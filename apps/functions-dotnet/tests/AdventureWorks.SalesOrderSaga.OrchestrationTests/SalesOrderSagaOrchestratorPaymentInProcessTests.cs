using AdventureWorks.Application.Features.Sales.Saga.Models;
using AdventureWorks.SalesOrderSaga.Activities;
using AdventureWorks.SalesOrderSaga.Contracts;
using AdventureWorks.SalesOrderSaga.Functions;
using AdventureWorks.SalesOrderSaga.Models;
using Microsoft.DurableTask;
using Microsoft.DurableTask.Client;
using Microsoft.DurableTask.Testing;

namespace AdventureWorks.SalesOrderSaga.OrchestrationTests;

/// <summary>
/// Post-reservation saga behavior driven through the real in-memory Durable Task engine: the
/// SQL retry policy against a flaky activity, and external payment events. Activities that touch
/// SQL or HTTP are replaced with counting stubs; the orchestrators and their options are real.
/// The 30-minute timer is never allowed to fire — it is cancelled when an event arrives.
/// </summary>
public sealed class SalesOrderSagaOrchestratorPaymentInProcessTests
{
    private const int SalesOrderId = 71774;
    private static readonly string InstanceId = SagaIds.InstanceIdFor(SalesOrderId);

    private static readonly SalesOrderSagaInput Input = new()
    {
        SalesOrderId = SalesOrderId,
        CustomerId = 29825,
        OrderDate = new DateTimeOffset(2026, 7, 1, 0, 0, 0, TimeSpan.Zero),
        Lines = [new SalesOrderSagaLineItem { ProductId = 776, OrderQty = 2, UnitPrice = 10m }]
    };

    private sealed class Calls
    {
        public int Reserve;
        public int Confirm;
        public int Release;
    }

    private static Task<DurableTaskTestHost> StartHostAsync(Calls calls, int reserveFailures) =>
        DurableTaskTestHost.StartAsync(registry =>
        {
            registry.AddOrchestrator(nameof(SalesOrderSagaOrchestrator), new SalesOrderSagaOrchestratorCore());
            registry.AddOrchestrator(nameof(CheckInventorySubOrchestrator), new CheckInventorySubOrchestratorCore());
            registry.AddActivity(nameof(ValidateOrderActivity), new ValidateOrderActivityCore());
            registry.AddActivityFunc<SalesOrderSagaLineItem, LineItemAvailability>(
                nameof(CheckInventoryActivity), (_, line) => new LineItemAvailability(line.ProductId, line.OrderQty, 100));
            registry.AddActivityFunc<SalesOrderSagaInput, ReservationReceipt>(nameof(ReserveStockActivity), (_, input) =>
            {
                if (Interlocked.Increment(ref calls.Reserve) <= reserveFailures)
                {
                    throw new InvalidOperationException("transient SQL timeout");
                }

                return new ReservationReceipt(input.SalesOrderId, [776], DateTimeOffset.UtcNow);
            });
            registry.AddActivityFunc<PaymentAuthorizationRequest, PaymentAuthorizationAcknowledgement>(
                nameof(PaymentAuthorizationActivity), (_, _) => new PaymentAuthorizationAcknowledgement("auth-1"));
            registry.AddActivityFunc<ConfirmOrderRequest, OrderApprovedEvent>(nameof(ConfirmOrderActivity), (_, request) =>
            {
                Interlocked.Increment(ref calls.Confirm);
                return new OrderApprovedEvent(
                    request.SalesOrderId, [], new OrderShippingAddress(9, "1 Main St", null, "Reno", "89501"), new OrderShipMethod(5, "Overnight"));
            });
            registry.AddActivityFunc<ReleaseStockRequest, ReleaseStockResult>(nameof(ReleaseStockActivity), (_, _) =>
            {
                Interlocked.Increment(ref calls.Release);
                return new ReleaseStockResult(false, 1);
            });
        });

    private static CancellationToken Timeout() => new CancellationTokenSource(TimeSpan.FromSeconds(60)).Token;

    [Fact]
    public async Task Orchestration_RetriesTransientReserveFailure_ThenApprovesOnPaymentEvent()
    {
        var calls = new Calls();
        await using var host = await StartHostAsync(calls, reserveFailures: 1);
        await host.Client.ScheduleNewOrchestrationInstanceAsync(
            nameof(SalesOrderSagaOrchestrator), Input, new StartOrchestrationOptions(InstanceId));

        await host.Client.RaiseEventAsync(InstanceId, SagaEventNames.PaymentApproved, new PaymentResultEvent(SalesOrderId, InstanceId));
        var metadata = await host.Client.WaitForInstanceCompletionAsync(InstanceId, getInputsAndOutputs: true, Timeout());

        Assert.Equal(OrchestrationRuntimeStatus.Completed, metadata.RuntimeStatus);
        Assert.Equal(SalesOrderSagaStatus.Approved, metadata.ReadOutputAs<SalesOrderSagaResult>()!.Status);
        Assert.Equal(2, calls.Reserve);
        Assert.Equal(1, calls.Confirm);
        Assert.Equal(0, calls.Release);
    }

    [Fact]
    public async Task Orchestration_CompensatesOnPaymentDeclinedEvent()
    {
        var calls = new Calls();
        await using var host = await StartHostAsync(calls, reserveFailures: 0);
        await host.Client.ScheduleNewOrchestrationInstanceAsync(
            nameof(SalesOrderSagaOrchestrator), Input, new StartOrchestrationOptions(InstanceId));

        await host.Client.RaiseEventAsync(
            InstanceId, SagaEventNames.PaymentDeclined, new PaymentResultEvent(SalesOrderId, InstanceId, Reason: "Card Declined"));
        var metadata = await host.Client.WaitForInstanceCompletionAsync(InstanceId, getInputsAndOutputs: true, Timeout());

        Assert.Equal(OrchestrationRuntimeStatus.Completed, metadata.RuntimeStatus);
        var result = metadata.ReadOutputAs<SalesOrderSagaResult>()!;
        Assert.Equal(SalesOrderSagaStatus.PaymentDeclined, result.Status);
        Assert.Equal("Card Declined", result.FailureReason);
        Assert.Equal(1, calls.Release);
        Assert.Equal(0, calls.Confirm);
    }
}
