using AdventureWorks.Application.Features.Sales.Saga.Models;
using AdventureWorks.Functions.SalesOrderSaga.Activities;
using AdventureWorks.Functions.SalesOrderSaga.Functions;
using AdventureWorks.Functions.SalesOrderSaga.Models;
using Microsoft.DurableTask;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace AdventureWorks.Functions.SalesOrderSaga.UnitTests.Functions;

public class SalesOrderSagaOrchestratorTests
{
    private static SalesOrderSagaInput SampleInput() => new()
    {
        SalesOrderId = 71774,
        CustomerId = 29825,
        OrderDate = new DateTimeOffset(2026, 7, 1, 0, 0, 0, TimeSpan.Zero),
        Lines = [new SalesOrderSagaLineItem { ProductId = 776, OrderQty = 1, UnitPrice = 2024.994m }]
    };

    private static Mock<TaskOrchestrationContext> CreateContextMock()
    {
        var context = new Mock<TaskOrchestrationContext>(MockBehavior.Strict);
        context.Setup(c => c.CreateReplaySafeLogger(It.IsAny<string>())).Returns(NullLogger.Instance);
        return context;
    }

    [Fact]
    public async Task RunAsync_ShortCircuits_WhenValidationFails()
    {
        var input = SampleInput();
        var context = CreateContextMock();
        context
            .Setup(c => c.CallActivityAsync<ValidateOrderResult>(nameof(ValidateOrderActivity), input, null))
            .ReturnsAsync(ValidateOrderResult.Failure(["CustomerId must be greater than 0"]));

        var result = await new SalesOrderSagaOrchestratorCore().RunAsync(context.Object, input);

        Assert.Equal(SalesOrderSagaStatus.ValidationFailed, result.Status);
        Assert.NotNull(result.ValidationErrors);
        context.Verify(
            c => c.CallSubOrchestratorAsync<CheckInventoryResult>(It.IsAny<TaskName>(), It.IsAny<object>(), It.IsAny<TaskOptions>()),
            Times.Never);
    }

    [Fact]
    public async Task RunAsync_ShortCircuits_WhenInventoryIsInsufficient()
    {
        var input = SampleInput();
        var context = CreateContextMock();
        context
            .Setup(c => c.CallActivityAsync<ValidateOrderResult>(nameof(ValidateOrderActivity), input, null))
            .ReturnsAsync(ValidateOrderResult.Success());
        context
            .Setup(c => c.CallSubOrchestratorAsync<CheckInventoryResult>(nameof(CheckInventorySubOrchestrator), input, null))
            .ReturnsAsync(new CheckInventoryResult([new LineItemAvailability(776, 1, 0)]));

        var result = await new SalesOrderSagaOrchestratorCore().RunAsync(context.Object, input);

        Assert.Equal(SalesOrderSagaStatus.InsufficientStock, result.Status);
        Assert.NotNull(result.UnavailableLines);
        Assert.Single(result.UnavailableLines);
        context.Verify(
            c => c.CallActivityAsync<ReservationReceipt>(It.IsAny<TaskName>(), It.IsAny<object>(), It.IsAny<TaskOptions>()),
            Times.Never);
    }

    [Fact]
    public async Task RunAsync_ReturnsReservedReceipt_WhenEverythingSucceeds()
    {
        var input = SampleInput();
        var context = CreateContextMock();
        var receipt = new ReservationReceipt(input.SalesOrderId, [776], DateTimeOffset.UtcNow);
        context
            .Setup(c => c.CallActivityAsync<ValidateOrderResult>(nameof(ValidateOrderActivity), input, null))
            .ReturnsAsync(ValidateOrderResult.Success());
        context
            .Setup(c => c.CallSubOrchestratorAsync<CheckInventoryResult>(nameof(CheckInventorySubOrchestrator), input, null))
            .ReturnsAsync(new CheckInventoryResult([new LineItemAvailability(776, 1, 100)]));
        context
            .Setup(c => c.CallActivityAsync<ReservationReceipt>(nameof(ReserveStockActivity), input, null))
            .ReturnsAsync(receipt);

        var result = await new SalesOrderSagaOrchestratorCore().RunAsync(context.Object, input);

        Assert.Equal(SalesOrderSagaStatus.Reserved, result.Status);
        Assert.Same(receipt, result.Receipt);
    }

    [Fact]
    public async Task OrchestratorAdapter_Throws_WhenInputIsNull()
    {
        var context = new Mock<TaskOrchestrationContext>(MockBehavior.Strict);
        context.Setup(c => c.GetInput<SalesOrderSagaInput>()).Returns((SalesOrderSagaInput?)null);

        await Assert.ThrowsAsync<InvalidOperationException>(() => SalesOrderSagaOrchestrator.RunAsync(context.Object));
    }
}
