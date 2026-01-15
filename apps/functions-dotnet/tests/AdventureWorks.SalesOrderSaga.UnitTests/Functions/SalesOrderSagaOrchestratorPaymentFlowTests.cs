using AdventureWorks.Application.Features.Sales.Saga.Models;
using AdventureWorks.SalesOrderSaga.Activities;
using AdventureWorks.SalesOrderSaga.Contracts;
using AdventureWorks.SalesOrderSaga.Functions;
using AdventureWorks.SalesOrderSaga.Models;
using Microsoft.DurableTask;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace AdventureWorks.SalesOrderSaga.UnitTests.Functions;

/// <summary>
/// Orchestrator branches after stock is reserved: payment approval, decline, timeout, and the
/// confirmation/compensation failure paths. The payment wait is simulated by completing the
/// task sources the mocked context hands back, so no real 30-minute timer is involved.
/// </summary>
public sealed class SalesOrderSagaOrchestratorPaymentFlowTests
{
    private const int SalesOrderId = 71774;
    private static readonly string InstanceId = SagaIds.InstanceIdFor(SalesOrderId);
    private static readonly DateTime Now = new(2026, 7, 1, 12, 0, 0, DateTimeKind.Utc);

    private static readonly SalesOrderSagaInput Input = new()
    {
        SalesOrderId = SalesOrderId,
        CustomerId = 29825,
        OrderDate = new DateTimeOffset(2026, 7, 1, 0, 0, 0, TimeSpan.Zero),
        Lines = [new SalesOrderSagaLineItem { ProductId = 776, OrderQty = 2, UnitPrice = 10m }]
    };

    private static readonly ReservationReceipt Receipt = new(SalesOrderId, [776], DateTimeOffset.UtcNow);

    private sealed class Harness
    {
        public Mock<TaskOrchestrationContext> Context { get; } = new(MockBehavior.Strict);
        public TaskCompletionSource<PaymentResultEvent> Approved { get; } = new();
        public TaskCompletionSource<PaymentResultEvent> Declined { get; } = new();
        public TaskCompletionSource Timer { get; } = new();
        public DateTime? TimerFireAt { get; private set; }
        public TaskOptions? ReserveOptions { get; private set; }
        public TaskOptions? ConfirmOptions { get; private set; }
        public TaskOptions? ReleaseOptions { get; private set; }
        public ReleaseStockRequest? ReleaseRequest { get; private set; }
        public SagaEventPublication? Published { get; private set; }

        public Harness()
        {
            Context.Setup(c => c.CreateReplaySafeLogger(It.IsAny<string>())).Returns(NullLogger.Instance);
            Context.SetupGet(c => c.InstanceId).Returns(InstanceId);
            Context.SetupGet(c => c.CurrentUtcDateTime).Returns(Now);
            Context.Setup(c => c.CallActivityAsync<ValidateOrderResult>(nameof(ValidateOrderActivity), Input, null))
                .ReturnsAsync(ValidateOrderResult.Success());
            Context.Setup(c => c.CallSubOrchestratorAsync<CheckInventoryResult>(nameof(CheckInventorySubOrchestrator), Input, null))
                .ReturnsAsync(new CheckInventoryResult([new LineItemAvailability(776, 2, 100)]));
            Context.Setup(c => c.CallActivityAsync<ReservationReceipt>(nameof(ReserveStockActivity), Input, It.IsAny<TaskOptions>()))
                .Callback<TaskName, object?, TaskOptions?>((_, _, o) => ReserveOptions = o)
                .ReturnsAsync(Receipt);
            Context.Setup(c => c.CallActivityAsync<PaymentAuthorizationAcknowledgement>(nameof(PaymentAuthorizationActivity), It.IsAny<PaymentAuthorizationRequest>(), null))
                .ReturnsAsync(new PaymentAuthorizationAcknowledgement("auth-1"));
            Context.Setup(c => c.WaitForExternalEvent<PaymentResultEvent>(SagaEventNames.PaymentApproved, It.IsAny<CancellationToken>()))
                .Returns(Approved.Task);
            Context.Setup(c => c.WaitForExternalEvent<PaymentResultEvent>(SagaEventNames.PaymentDeclined, It.IsAny<CancellationToken>()))
                .Returns(Declined.Task);
            Context.Setup(c => c.CreateTimer(It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
                .Callback<DateTime, CancellationToken>((fireAt, _) => TimerFireAt = fireAt)
                .Returns(Timer.Task);
        }

        public Harness WithRelease(Func<ReleaseStockResult> result)
        {
            Context.Setup(c => c.CallActivityAsync<ReleaseStockResult>(nameof(ReleaseStockActivity), It.IsAny<ReleaseStockRequest>(), It.IsAny<TaskOptions>()))
                .Callback<TaskName, object?, TaskOptions?>((_, request, o) => { ReleaseRequest = (ReleaseStockRequest)request!; ReleaseOptions = o; })
                .Returns(() => Task.FromResult(result()));
            return this;
        }

        public Harness WithConfirm(Func<Task<OrderApprovedEvent>> result)
        {
            Context.Setup(c => c.CallActivityAsync<OrderApprovedEvent>(nameof(ConfirmOrderActivity), It.IsAny<ConfirmOrderRequest>(), It.IsAny<TaskOptions>()))
                .Callback<TaskName, object?, TaskOptions?>((_, _, o) => ConfirmOptions = o)
                .Returns(result);
            return this;
        }

        public Harness WithEnqueue()
        {
            Context.Setup(c => c.CallActivityAsync<bool>(nameof(EnqueueSagaEventActivity), It.IsAny<SagaEventPublication>(), null))
                .Callback<TaskName, object?, TaskOptions?>((_, p, _) => Published = (SagaEventPublication)p!)
                .ReturnsAsync(true);
            return this;
        }

        public Task<SalesOrderSagaResult> RunAsync() => new SalesOrderSagaOrchestratorCore().RunAsync(Context.Object, Input);
    }

    private static OrderApprovedEvent ApprovedEvent() => new(
        SalesOrderId, [new OrderApprovedLine(776, 2, 10m)], new OrderShippingAddress(9, "1 Main St", null, "Reno", "89501"), new OrderShipMethod(5, "Overnight"));

    [Fact]
    public async Task RunAsync_WhenPaymentApproved_ConfirmsOrderWithoutReleasingStock()
    {
        var harness = new Harness().WithConfirm(() => Task.FromResult(ApprovedEvent()));
        harness.Approved.SetResult(new PaymentResultEvent(SalesOrderId, InstanceId, "code"));

        var result = await harness.RunAsync();

        Assert.Equal(SalesOrderSagaStatus.Approved, result.Status);
        Assert.Same(Receipt, result.Receipt);
        harness.Context.Verify(c => c.CallActivityAsync<ReleaseStockResult>(It.IsAny<TaskName>(), It.IsAny<object?>(), It.IsAny<TaskOptions?>()), Times.Never);
    }

    [Fact]
    public async Task RunAsync_WhenPaymentDeclined_ReleasesStockWithDeclineReason()
    {
        var harness = new Harness().WithRelease(() => new ReleaseStockResult(false, 1));
        harness.Declined.SetResult(new PaymentResultEvent(SalesOrderId, InstanceId, Reason: "Card Declined"));

        var result = await harness.RunAsync();

        Assert.Equal(SalesOrderSagaStatus.PaymentDeclined, result.Status);
        Assert.Equal("Card Declined", result.FailureReason);
        Assert.Equal(SalesOrderSagaStatus.PaymentDeclined, harness.ReleaseRequest!.FailureStatus);
        Assert.Equal(InstanceId, harness.ReleaseRequest.SagaInstanceId);
        Assert.Equal("Card Declined", harness.ReleaseRequest.FailureReason);
    }

    [Fact]
    public async Task RunAsync_WhenTimerFires_PublishesTimeoutEventAndCompensates()
    {
        var harness = new Harness().WithEnqueue().WithRelease(() => new ReleaseStockResult(false, 1));
        harness.Timer.SetResult();

        var result = await harness.RunAsync();

        Assert.Equal(SalesOrderSagaStatus.PaymentTimedOut, result.Status);
        Assert.Equal(Now.AddMinutes(30), harness.TimerFireAt);
        Assert.Equal(SagaEventNames.PaymentTimedOut, harness.Published!.EventName);
        Assert.Equal($"{SagaEventNames.PaymentTimedOut}:{InstanceId}", harness.Published.MessageId);
        Assert.Equal(SalesOrderSagaStatus.PaymentTimedOut, harness.ReleaseRequest!.FailureStatus);
    }

    [Fact]
    public async Task RunAsync_WhenConfirmationFails_DoesNotReleaseStock()
    {
        var harness = new Harness().WithConfirm(() => Task.FromException<OrderApprovedEvent>(new InvalidOperationException("sql down")));
        harness.Approved.SetResult(new PaymentResultEvent(SalesOrderId, InstanceId));

        var result = await harness.RunAsync();

        Assert.Equal(SalesOrderSagaStatus.ConfirmationFailed, result.Status);
        Assert.Equal("sql down", result.FailureReason);
        harness.Context.Verify(c => c.CallActivityAsync<ReleaseStockResult>(It.IsAny<TaskName>(), It.IsAny<object?>(), It.IsAny<TaskOptions?>()), Times.Never);
    }

    [Fact]
    public async Task RunAsync_WhenCompensationFails_ReturnsCompensationFailedWithReason()
    {
        var harness = new Harness().WithRelease(() => throw new InvalidOperationException("release exploded"));
        harness.Declined.SetResult(new PaymentResultEvent(SalesOrderId, InstanceId, Reason: "Card Declined"));

        var result = await harness.RunAsync();

        Assert.Equal(SalesOrderSagaStatus.CompensationFailed, result.Status);
        Assert.Contains("release exploded", result.FailureReason);
    }

    [Fact]
    public async Task RunAsync_UsesSqlRetryPolicyForReserveConfirmAndRelease()
    {
        var approved = new Harness().WithConfirm(() => Task.FromResult(ApprovedEvent()));
        approved.Approved.SetResult(new PaymentResultEvent(SalesOrderId, InstanceId));
        await approved.RunAsync();

        var declined = new Harness().WithRelease(() => new ReleaseStockResult(false, 1));
        declined.Declined.SetResult(new PaymentResultEvent(SalesOrderId, InstanceId));
        await declined.RunAsync();

        // Four attempts: the initial call, a 5s retry, then two 2-minute-capped retries.
        foreach (var options in new[] { approved.ReserveOptions, approved.ConfirmOptions, declined.ReleaseOptions })
        {
            var policy = Assert.IsType<TaskOptions>(options).Retry!.Policy;
            Assert.NotNull(policy);
            Assert.Equal(4, policy.MaxNumberOfAttempts);
            Assert.Equal(TimeSpan.FromSeconds(5), policy.FirstRetryInterval);
            Assert.Equal(TimeSpan.FromMinutes(2), policy.MaxRetryInterval);
        }
    }

    [Fact]
    public async Task RunAsync_BuildsPaymentRequestFromLineTotals()
    {
        PaymentAuthorizationRequest? sent = null;
        var harness = new Harness().WithRelease(() => new ReleaseStockResult(false, 1));
        harness.Context.Setup(c => c.CallActivityAsync<PaymentAuthorizationAcknowledgement>(nameof(PaymentAuthorizationActivity), It.IsAny<PaymentAuthorizationRequest>(), null))
            .Callback<TaskName, object?, TaskOptions?>((_, r, _) => sent = (PaymentAuthorizationRequest)r!)
            .ReturnsAsync(new PaymentAuthorizationAcknowledgement("auth-1"));
        harness.Declined.SetResult(new PaymentResultEvent(SalesOrderId, InstanceId));

        await harness.RunAsync();

        Assert.Equal(20m, sent!.Amount);
        Assert.Equal("USD", sent.Currency);
        Assert.Equal(InstanceId, sent.IdempotencyKey);
        Assert.Single(sent.Lines);
    }
}
