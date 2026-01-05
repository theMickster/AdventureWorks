using AdventureWorks.Application.Features.Sales.Saga.Models;
using AdventureWorks.SalesOrderSaga.Activities;
using AdventureWorks.SalesOrderSaga.Models;
using AdventureWorks.SalesOrderSaga.Contracts;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace AdventureWorks.SalesOrderSaga.Functions;

/// <summary>
/// Durable sales-order saga: validates and reserves stock, requests payment, awaits a correlated
/// payment event for 30 minutes, then confirms the order or compensates its exact allocations.
/// All I/O occurs in activities; orchestration time and its idempotency key are replay-safe.
/// </summary>
public sealed class SalesOrderSagaOrchestratorCore : TaskOrchestrator<SalesOrderSagaInput, SalesOrderSagaResult>
{
    // Attempts: initial, then 5 seconds, then two 2-minute intervals. SQL activities are the
    // only calls using this policy because their failures are transient infrastructure failures.
    internal static readonly TaskOptions SqlRetryOptions = TaskOptions.FromRetryPolicy(
        new RetryPolicy(maxNumberOfAttempts: 4, firstRetryInterval: TimeSpan.FromSeconds(5), backoffCoefficient: 24, maxRetryInterval: TimeSpan.FromMinutes(2)));

    public override async Task<SalesOrderSagaResult> RunAsync(TaskOrchestrationContext context, SalesOrderSagaInput input)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(input);
        var logger = context.CreateReplaySafeLogger(nameof(SalesOrderSagaOrchestratorCore));

        var validation = await context.CallActivityAsync<ValidateOrderResult>(nameof(ValidateOrderActivity), input);
        if (!validation.IsValid)
        {
            return SalesOrderSagaResult.ValidationFailed(input.SalesOrderId, validation.Errors);
        }

        var inventory = await context.CallSubOrchestratorAsync<CheckInventoryResult>(nameof(CheckInventorySubOrchestrator), input);
        if (!inventory.AllAvailable)
        {
            return SalesOrderSagaResult.InsufficientStock(input.SalesOrderId, inventory.Lines);
        }

        var receipt = await context.CallActivityAsync<ReservationReceipt>(nameof(ReserveStockActivity), input, SqlRetryOptions);
        var paymentRequest = new PaymentAuthorizationRequest(
            input.SalesOrderId,
            input.Lines.Sum(line => line.OrderQty * line.UnitPrice),
            "USD",
            context.InstanceId,
            input.Lines.Select(line => new SagaOrderLine(line.ProductId, line.OrderQty, line.UnitPrice)).ToArray());

        try
        {
            await context.CallActivityAsync<PaymentAuthorizationAcknowledgement>(nameof(PaymentAuthorizationActivity), paymentRequest);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Payment authorization request failed for SalesOrderId {SalesOrderId}.", input.SalesOrderId);
            return await CompensateAsync(context, receipt, SalesOrderSagaStatus.PaymentAuthorizationFailed, exception.Message);
        }

        using var cancellation = new CancellationTokenSource();
        var approved = context.WaitForExternalEvent<PaymentResultEvent>(SagaEventNames.PaymentApproved, cancellation.Token);
        var declined = context.WaitForExternalEvent<PaymentResultEvent>(SagaEventNames.PaymentDeclined, cancellation.Token);
        var timedOut = context.CreateTimer(context.CurrentUtcDateTime.AddMinutes(30), cancellation.Token);
        var completed = await Task.WhenAny(approved, declined, timedOut);
        cancellation.Cancel();

        if (completed == approved)
        {
            try
            {
                await context.CallActivityAsync<OrderApprovedEvent>(nameof(ConfirmOrderActivity), new ConfirmOrderRequest(input.SalesOrderId, context.InstanceId), SqlRetryOptions);
                return SalesOrderSagaResult.Approved(input.SalesOrderId, receipt);
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Order confirmation failed for SalesOrderId {SalesOrderId}.", input.SalesOrderId);
                // Confirmation may have committed before an infrastructure failure reaches this
                // orchestration. Its idempotent activity/outbox retry owns recovery; never
                // release stock after the order could already be approved.
                return SalesOrderSagaResult.ConfirmationFailed(input.SalesOrderId, receipt, exception.Message);
            }
        }

        if (completed == declined)
        {
            var payment = await declined;
            return await CompensateAsync(context, receipt, SalesOrderSagaStatus.PaymentDeclined, payment.Reason);
        }

        var timeoutReason = "Payment result was not received within 30 minutes.";
        await context.CallActivityAsync<bool>(nameof(EnqueueSagaEventActivity), new SagaEventPublication(
            SagaEventNames.PaymentTimedOut,
            $"{SagaEventNames.PaymentTimedOut}:{context.InstanceId}",
            JsonSerializer.Serialize(new PaymentTimedOutEvent(input.SalesOrderId, context.InstanceId, context.CurrentUtcDateTime))));
        return await CompensateAsync(context, receipt, SalesOrderSagaStatus.PaymentTimedOut, timeoutReason);
    }

    /// <summary>Releases a successful reservation once, publishes failure, and preserves the terminal reason.</summary>
    private static async Task<SalesOrderSagaResult> CompensateAsync(
        TaskOrchestrationContext context,
        ReservationReceipt receipt,
        SalesOrderSagaStatus failureStatus,
        string? reason)
    {
        try
        {
            await context.CallActivityAsync<ReleaseStockResult>(
                nameof(ReleaseStockActivity), new ReleaseStockRequest(receipt.SalesOrderId, context.InstanceId, failureStatus, reason), SqlRetryOptions);
            var result = failureStatus switch
            {
                SalesOrderSagaStatus.PaymentDeclined => SalesOrderSagaResult.PaymentDeclined(receipt.SalesOrderId, receipt, reason),
                SalesOrderSagaStatus.PaymentTimedOut => SalesOrderSagaResult.PaymentTimedOut(receipt.SalesOrderId, receipt),
                SalesOrderSagaStatus.PaymentAuthorizationFailed => SalesOrderSagaResult.PaymentAuthorizationFailed(receipt.SalesOrderId, receipt, reason ?? "Payment authorization failed."),
                _ => SalesOrderSagaResult.CompensationFailed(receipt.SalesOrderId, receipt, reason ?? "Order confirmation failed.")
            };
            return result;
        }
        catch (Exception exception)
        {
            return SalesOrderSagaResult.CompensationFailed(receipt.SalesOrderId, receipt, $"Compensation failed: {exception.Message}");
        }
    }
}

/// <summary>Functions adapter for <see cref="SalesOrderSagaOrchestratorCore"/>.</summary>
public static class SalesOrderSagaOrchestrator
{
    [Function(nameof(SalesOrderSagaOrchestrator))]
    public static Task<SalesOrderSagaResult> RunAsync([OrchestrationTrigger] TaskOrchestrationContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        return new SalesOrderSagaOrchestratorCore().RunAsync(
            context,
            context.GetInput<SalesOrderSagaInput>() ?? throw new InvalidOperationException("Sales order saga orchestrator started without input."));
    }
}
