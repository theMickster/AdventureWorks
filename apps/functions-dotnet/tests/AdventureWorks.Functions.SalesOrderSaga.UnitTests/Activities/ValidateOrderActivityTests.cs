using AdventureWorks.Application.Features.Sales.Saga.Models;
using AdventureWorks.Functions.SalesOrderSaga.Activities;
using Microsoft.DurableTask;
using Moq;

namespace AdventureWorks.Functions.SalesOrderSaga.UnitTests.Activities;

public class ValidateOrderActivityTests
{
    private static SalesOrderSagaInput ValidInput() => new()
    {
        SalesOrderId = 71774,
        CustomerId = 29825,
        OrderDate = new DateTimeOffset(2026, 7, 1, 0, 0, 0, TimeSpan.Zero),
        Lines = [new SalesOrderSagaLineItem { ProductId = 776, OrderQty = 1, UnitPrice = 2024.994m }]
    };

    [Fact]
    public async Task RunAsync_ReturnsSuccess_WhenInputIsValid()
    {
        var context = new Mock<TaskActivityContext>(MockBehavior.Strict);

        var result = await new ValidateOrderActivityCore().RunAsync(context.Object, ValidInput());

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public async Task RunAsync_ReturnsFailure_WhenCustomerIdIsInvalid()
    {
        var context = new Mock<TaskActivityContext>(MockBehavior.Strict);
        var input = ValidInput();
        var invalidInput = new SalesOrderSagaInput
        {
            SalesOrderId = input.SalesOrderId,
            CustomerId = 0,
            OrderDate = input.OrderDate,
            Lines = input.Lines
        };

        var result = await new ValidateOrderActivityCore().RunAsync(context.Object, invalidInput);

        Assert.False(result.IsValid);
        Assert.NotEmpty(result.Errors);
    }

    [Fact]
    public async Task RunAsync_ReturnsFailure_WhenLinesAreEmpty()
    {
        var context = new Mock<TaskActivityContext>(MockBehavior.Strict);
        var input = ValidInput();
        var invalidInput = new SalesOrderSagaInput
        {
            SalesOrderId = input.SalesOrderId,
            CustomerId = input.CustomerId,
            OrderDate = input.OrderDate,
            Lines = []
        };

        var result = await new ValidateOrderActivityCore().RunAsync(context.Object, invalidInput);

        Assert.False(result.IsValid);
        Assert.NotEmpty(result.Errors);
    }

    [Fact]
    public async Task ActivityAdapter_DelegatesTo_Core()
    {
        var result = await ValidateOrderActivity.RunAsync(ValidInput());

        Assert.True(result.IsValid);
    }
}
