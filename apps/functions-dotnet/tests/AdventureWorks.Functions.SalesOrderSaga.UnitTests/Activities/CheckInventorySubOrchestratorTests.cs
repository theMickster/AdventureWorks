using AdventureWorks.Application.Features.Sales.Saga.Models;
using AdventureWorks.Functions.SalesOrderSaga.Activities;
using AdventureWorks.Functions.SalesOrderSaga.Models;
using Microsoft.DurableTask;
using Moq;

namespace AdventureWorks.Functions.SalesOrderSaga.UnitTests.Activities;

public class CheckInventorySubOrchestratorTests
{
    private static SalesOrderSagaInput InputWithLines(int lineCount)
    {
        var lines = Enumerable.Range(1, lineCount)
            .Select(i => new SalesOrderSagaLineItem { ProductId = i, OrderQty = 1, UnitPrice = 10m })
            .ToArray();

        return new SalesOrderSagaInput
        {
            SalesOrderId = 71774,
            CustomerId = 29825,
            OrderDate = new DateTimeOffset(2026, 7, 1, 0, 0, 0, TimeSpan.Zero),
            Lines = lines
        };
    }

    [Fact]
    public async Task RunAsync_CallsCheckInventoryActivity_OncePerLineItem_InParallel()
    {
        const int lineCount = 5;
        var input = InputWithLines(lineCount);
        var context = new Mock<TaskOrchestrationContext>(MockBehavior.Strict);

        foreach (var line in input.Lines)
        {
            context
                .Setup(c => c.CallActivityAsync<LineItemAvailability>(
                    nameof(CheckInventoryActivity),
                    It.Is<SalesOrderSagaLineItem>(l => l.ProductId == line.ProductId),
                    null))
                .ReturnsAsync(new LineItemAvailability(line.ProductId, line.OrderQty, 100));
        }

        var result = await new CheckInventorySubOrchestratorCore().RunAsync(context.Object, input);

        Assert.Equal(lineCount, result.Lines.Count);
        Assert.True(result.AllAvailable);
        context.Verify(
            c => c.CallActivityAsync<LineItemAvailability>(nameof(CheckInventoryActivity), It.IsAny<SalesOrderSagaLineItem>(), null),
            Times.Exactly(lineCount));
    }

    [Fact]
    public async Task RunAsync_ReturnsNotAllAvailable_WhenAnyLineIsShort()
    {
        var input = InputWithLines(2);
        var context = new Mock<TaskOrchestrationContext>(MockBehavior.Strict);

        context
            .Setup(c => c.CallActivityAsync<LineItemAvailability>(
                nameof(CheckInventoryActivity),
                It.Is<SalesOrderSagaLineItem>(l => l.ProductId == 1),
                null))
            .ReturnsAsync(new LineItemAvailability(1, 1, 100));
        context
            .Setup(c => c.CallActivityAsync<LineItemAvailability>(
                nameof(CheckInventoryActivity),
                It.Is<SalesOrderSagaLineItem>(l => l.ProductId == 2),
                null))
            .ReturnsAsync(new LineItemAvailability(2, 1, 0));

        var result = await new CheckInventorySubOrchestratorCore().RunAsync(context.Object, input);

        Assert.False(result.AllAvailable);
    }

    [Fact]
    public async Task OrchestratorAdapter_Throws_WhenInputIsNull()
    {
        var context = new Mock<TaskOrchestrationContext>(MockBehavior.Strict);
        context.Setup(c => c.GetInput<SalesOrderSagaInput>()).Returns((SalesOrderSagaInput?)null);

        await Assert.ThrowsAsync<InvalidOperationException>(() => CheckInventorySubOrchestrator.RunAsync(context.Object));
    }
}
