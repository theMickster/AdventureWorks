using AdventureWorks.Application.Features.Sales.Saga.Models;
using AdventureWorks.Application.Features.Sales.Saga.Validators;
using FluentValidation.TestHelper;

namespace AdventureWorks.UnitTests.Application.Features.Sales.Saga.Validators;

[ExcludeFromCodeCoverage]
public sealed class SalesOrderSagaInputValidatorTests : UnitTestBase
{
    private readonly SalesOrderSagaInputValidator _sut = new();

    private static SalesOrderSagaInput ValidInput(int? customerId = null, IReadOnlyList<SalesOrderSagaLineItem>? lines = null) => new()
    {
        SalesOrderId = 71774,
        CustomerId = customerId ?? 29825,
        OrderDate = new DateTimeOffset(2026, 7, 1, 0, 0, 0, TimeSpan.Zero),
        Lines = lines ?? [new SalesOrderSagaLineItem { ProductId = 776, OrderQty = 1, UnitPrice = 2024.994m }]
    };

    [Fact]
    public void Validator_error_messages_are_correct()
    {
        SalesOrderSagaInputValidator.MessageCustomerIdInvalid.Should().Be("CustomerId must be greater than 0");
        SalesOrderSagaInputValidator.MessageLinesEmpty.Should().Be("Order must contain at least one line item");
        SalesOrderSagaInputValidator.MessageProductIdInvalid.Should().Be("ProductId must be greater than 0");
        SalesOrderSagaInputValidator.MessageOrderQtyInvalid.Should().Be("OrderQty must be greater than 0");
    }

    [Fact]
    public async Task Validator_succeeds_when_all_data_is_validAsync()
    {
        var result = await _sut.TestValidateAsync(ValidInput());

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task Validator_should_have_customer_id_errorAsync(int customerId)
    {
        var input = ValidInput(customerId: customerId);

        var result = await _sut.TestValidateAsync(input);

        result.ShouldHaveValidationErrorFor(x => x.CustomerId)
            .WithErrorCode("Rule-01");
    }

    [Fact]
    public async Task Validator_should_have_lines_empty_errorAsync()
    {
        var input = ValidInput(lines: []);

        var result = await _sut.TestValidateAsync(input);

        result.ShouldHaveValidationErrorFor(x => x.Lines)
            .WithErrorCode("Rule-02");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task Validator_should_have_product_id_errorAsync(int productId)
    {
        var input = ValidInput(lines: [new SalesOrderSagaLineItem { ProductId = productId, OrderQty = 1, UnitPrice = 10m }]);

        var result = await _sut.TestValidateAsync(input);

        result.ShouldHaveValidationErrorFor("Lines[0].ProductId")
            .WithErrorCode("Rule-03");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task Validator_should_have_order_qty_errorAsync(short orderQty)
    {
        var input = ValidInput(lines: [new SalesOrderSagaLineItem { ProductId = 776, OrderQty = orderQty, UnitPrice = 10m }]);

        var result = await _sut.TestValidateAsync(input);

        result.ShouldHaveValidationErrorFor("Lines[0].OrderQty")
            .WithErrorCode("Rule-04");
    }
}
