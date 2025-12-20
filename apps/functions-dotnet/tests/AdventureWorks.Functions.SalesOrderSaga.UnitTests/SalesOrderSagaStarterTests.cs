using System.Text.Json;
using AdventureWorks.Application.Features.Sales.Saga.Models;
using AdventureWorks.Functions.SalesOrderSaga.Functions;
using Microsoft.DurableTask;
using Microsoft.DurableTask.Client;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace AdventureWorks.Functions.SalesOrderSaga.UnitTests;

public class SalesOrderSagaStarterTests
{
    private static SalesOrderSagaInput SampleInput() => new()
    {
        SalesOrderId = 71774,
        CustomerId = 29825,
        OrderDate = new DateTimeOffset(2026, 7, 1, 0, 0, 0, TimeSpan.Zero),
        Lines =
        [
            new SalesOrderSagaLineItem { ProductId = 776, OrderQty = 1, UnitPrice = 2024.994m }
        ]
    };

    [Fact]
    public void BuildInstanceId_IsDeterministic_ForSameSalesOrderId()
    {
        var first = SalesOrderSagaStarter.BuildInstanceId(71774);
        var second = SalesOrderSagaStarter.BuildInstanceId(71774);

        Assert.Equal(first, second);
        Assert.Equal("sales-order-saga-71774", first);
    }

    [Fact]
    public void BuildInstanceId_Differs_ForDifferentSalesOrderIds()
    {
        Assert.NotEqual(SalesOrderSagaStarter.BuildInstanceId(1), SalesOrderSagaStarter.BuildInstanceId(2));
    }

    [Fact]
    public void OrderCreatedMessage_DeserializesInto_SalesOrderSagaInput()
    {
        var input = SampleInput();
        var json = JsonSerializer.Serialize(input);

        var deserialized = JsonSerializer.Deserialize<SalesOrderSagaInput>(json);

        Assert.NotNull(deserialized);
        Assert.Equal(input.SalesOrderId, deserialized!.SalesOrderId);
        Assert.Equal(input.CustomerId, deserialized.CustomerId);
        Assert.Equal(input.OrderDate, deserialized.OrderDate);
        Assert.Single(deserialized.Lines);
        Assert.Equal(input.Lines[0].ProductId, deserialized.Lines[0].ProductId);
        Assert.Equal(input.Lines[0].OrderQty, deserialized.Lines[0].OrderQty);
        Assert.Equal(input.Lines[0].UnitPrice, deserialized.Lines[0].UnitPrice);
    }

    [Fact]
    public async Task RunAsync_StartsNewOrchestration_WhenNoInstanceExists()
    {
        var input = SampleInput();
        var instanceId = SalesOrderSagaStarter.BuildInstanceId(input.SalesOrderId);
        var client = new Mock<DurableTaskClient>(MockBehavior.Strict, "test");

        client
            .Setup(c => c.GetInstanceAsync(instanceId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync((OrchestrationMetadata?)null);
        client
            .Setup(c => c.ScheduleNewOrchestrationInstanceAsync(
                nameof(SalesOrderSaga.Functions.SalesOrderSagaOrchestrator),
                It.Is<SalesOrderSagaInput>(i => i.SalesOrderId == input.SalesOrderId),
                It.Is<StartOrchestrationOptions>(o => o.InstanceId == instanceId),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(instanceId);

        var starter = new SalesOrderSagaStarter(NullLogger<SalesOrderSagaStarter>.Instance);

        await starter.RunAsync(JsonSerializer.Serialize(input), client.Object, CancellationToken.None);

        client.VerifyAll();
    }

    [Fact]
    public async Task RunAsync_SkipsDuplicate_WhenInstanceAlreadyRunning()
    {
        var input = SampleInput();
        var instanceId = SalesOrderSagaStarter.BuildInstanceId(input.SalesOrderId);
        var client = new Mock<DurableTaskClient>(MockBehavior.Strict, "test");

        var runningMetadata = new OrchestrationMetadata(nameof(SalesOrderSaga.Functions.SalesOrderSagaOrchestrator), instanceId)
        {
            RuntimeStatus = OrchestrationRuntimeStatus.Running
        };
        client
            .Setup(c => c.GetInstanceAsync(instanceId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(runningMetadata);

        var starter = new SalesOrderSagaStarter(NullLogger<SalesOrderSagaStarter>.Instance);

        await starter.RunAsync(JsonSerializer.Serialize(input), client.Object, CancellationToken.None);

        client.Verify(
            c => c.ScheduleNewOrchestrationInstanceAsync(
                It.IsAny<TaskName>(),
                It.IsAny<object>(),
                It.IsAny<StartOrchestrationOptions>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
