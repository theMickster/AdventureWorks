using AdventureWorks.Application.Features.Purchasing.Queries;
using AdventureWorks.Application.Features.Purchasing.Validators;
using AdventureWorks.Common.Filtering;
using FluentAssertions;

namespace AdventureWorks.UnitTests.Application.Features.Purchasing.Validators;

public sealed class ReadVendorPurchaseOrdersQueryValidatorTests
{
    private readonly ReadVendorPurchaseOrdersQueryValidator _sut = new();

    [Fact]
    public async Task Validation_succeeds_with_valid_query()
    {
        var query = new ReadVendorPurchaseOrdersQuery
        {
            VendorId = 1496,
            Parameters = new VendorPurchaseOrderParameter { PageNumber = 1, PageSize = 25 }
        };

        var result = await _sut.ValidateAsync(query, cancellation: TestContext.Current.CancellationToken);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validation_fails_when_parameters_null()
    {
        var query = new ReadVendorPurchaseOrdersQuery { VendorId = 1496, Parameters = null! };

        var result = await _sut.ValidateAsync(query, cancellation: TestContext.Current.CancellationToken);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.ErrorCode == "Rule-01");
    }

    [Theory]
    [InlineData((byte)1)]
    [InlineData((byte)4)]
    public async Task Validation_succeeds_when_status_is_in_valid_range(byte validStatus)
    {
        var query = new ReadVendorPurchaseOrdersQuery
        {
            VendorId = 1496,
            Parameters = new VendorPurchaseOrderParameter { PageNumber = 1, Status = validStatus }
        };

        var result = await _sut.ValidateAsync(query, cancellation: TestContext.Current.CancellationToken);

        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData((byte)0)]
    [InlineData((byte)5)]
    [InlineData((byte)255)]
    public async Task Validation_fails_when_status_is_out_of_range(byte invalidStatus)
    {
        var query = new ReadVendorPurchaseOrdersQuery
        {
            VendorId = 1496,
            Parameters = new VendorPurchaseOrderParameter { PageNumber = 1, Status = invalidStatus }
        };

        var result = await _sut.ValidateAsync(query, cancellation: TestContext.Current.CancellationToken);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.ErrorCode == "Rule-02");
    }

    [Fact]
    public async Task Validation_succeeds_when_start_date_before_end_date()
    {
        var query = new ReadVendorPurchaseOrdersQuery
        {
            VendorId = 1496,
            Parameters = new VendorPurchaseOrderParameter
            {
                PageNumber = 1,
                StartDate = new DateTime(2014, 1, 1),
                EndDate = new DateTime(2014, 12, 31)
            }
        };

        var result = await _sut.ValidateAsync(query, cancellation: TestContext.Current.CancellationToken);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validation_fails_when_start_date_after_end_date()
    {
        var query = new ReadVendorPurchaseOrdersQuery
        {
            VendorId = 1496,
            Parameters = new VendorPurchaseOrderParameter
            {
                PageNumber = 1,
                StartDate = new DateTime(2014, 12, 31),
                EndDate = new DateTime(2014, 1, 1)
            }
        };

        var result = await _sut.ValidateAsync(query, cancellation: TestContext.Current.CancellationToken);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.ErrorCode == "Rule-03");
    }
}
