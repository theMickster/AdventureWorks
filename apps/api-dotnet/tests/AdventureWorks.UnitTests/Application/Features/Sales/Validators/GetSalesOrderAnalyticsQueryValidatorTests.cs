using AdventureWorks.Application.Features.Sales.Queries;
using AdventureWorks.Application.Features.Sales.Validators;
using AdventureWorks.Common.Filtering;

namespace AdventureWorks.UnitTests.Application.Features.Sales.Validators;

public sealed class GetSalesOrderAnalyticsQueryValidatorTests
{
    private readonly GetSalesOrderAnalyticsQueryValidator _sut = new();

    [Fact]
    public async Task Validation_succeeds_when_filter_is_null()
    {
        // Arrange
        var query = new GetSalesOrderAnalyticsQuery { Filter = null };

        // Act
        var result = await _sut.ValidateAsync(query);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validation_succeeds_with_valid_filter()
    {
        // Arrange
        var query = new GetSalesOrderAnalyticsQuery
        {
            Filter = new SalesOrderSearchModel
            {
                OrderDateFrom = new DateTime(2013, 1, 1),
                OrderDateTo = new DateTime(2013, 12, 31),
                SalesPersonId = 275,
                TerritoryId = 2,
                Status = 5
            }
        };

        // Act
        var result = await _sut.ValidateAsync(query);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validation_fails_when_order_date_from_greater_than_to()
    {
        // Arrange
        var query = new GetSalesOrderAnalyticsQuery
        {
            Filter = new SalesOrderSearchModel
            {
                OrderDateFrom = new DateTime(2014, 6, 30),
                OrderDateTo = new DateTime(2014, 1, 1)
            }
        };

        // Act
        var result = await _sut.ValidateAsync(query);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.ErrorCode == "Rule-04");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task Validation_fails_when_sales_person_id_invalid(int invalidSalesPersonId)
    {
        // Arrange
        var query = new GetSalesOrderAnalyticsQuery
        {
            Filter = new SalesOrderSearchModel { SalesPersonId = invalidSalesPersonId }
        };

        // Act
        var result = await _sut.ValidateAsync(query);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.ErrorCode == "Rule-05");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task Validation_fails_when_territory_id_invalid(int invalidTerritoryId)
    {
        // Arrange
        var query = new GetSalesOrderAnalyticsQuery
        {
            Filter = new SalesOrderSearchModel { TerritoryId = invalidTerritoryId }
        };

        // Act
        var result = await _sut.ValidateAsync(query);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.ErrorCode == "Rule-06");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(7)]
    public async Task Validation_fails_when_status_is_out_of_range(byte invalidStatus)
    {
        // Arrange
        var query = new GetSalesOrderAnalyticsQuery
        {
            Filter = new SalesOrderSearchModel { Status = invalidStatus }
        };

        // Act
        var result = await _sut.ValidateAsync(query);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.ErrorCode == "Rule-07");
    }

    [Theory]
    [InlineData(1)]
    [InlineData(3)]
    [InlineData(6)]
    public async Task Validation_succeeds_when_status_is_valid(byte validStatus)
    {
        // Arrange
        var query = new GetSalesOrderAnalyticsQuery
        {
            Filter = new SalesOrderSearchModel { Status = validStatus }
        };

        // Act
        var result = await _sut.ValidateAsync(query);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validation_fails_when_account_number_exceeds_15_chars()
    {
        // Arrange
        var query = new GetSalesOrderAnalyticsQuery
        {
            Filter = new SalesOrderSearchModel { AccountNumber = new string('A', 16) }
        };

        // Act
        var result = await _sut.ValidateAsync(query);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.ErrorCode == "Rule-08");
    }

    [Fact]
    public async Task Validation_succeeds_when_account_number_is_15_chars()
    {
        // Arrange
        var query = new GetSalesOrderAnalyticsQuery
        {
            Filter = new SalesOrderSearchModel { AccountNumber = new string('A', 15) }
        };

        // Act
        var result = await _sut.ValidateAsync(query);

        // Assert
        result.IsValid.Should().BeTrue();
    }
}
