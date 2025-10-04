using AdventureWorks.Application.Features.Sales.Queries;
using AdventureWorks.Application.Features.Sales.Validators;
using AdventureWorks.Common.Filtering;
using FluentAssertions;

namespace AdventureWorks.UnitTests.Application.Features.Sales.Validators;

public sealed class ReadSalesOrderListQueryValidatorTests
{
    private readonly ReadSalesOrderListQueryValidator _sut = new();

    [Fact]
    public async Task Validation_succeeds_with_valid_query()
    {
        // Arrange
        var query = new ReadSalesOrderListQuery
        {
            Parameters = new SalesOrderParameter { PageNumber = 1, PageSize = 10 }
        };

        // Act
        var result = await _sut.ValidateAsync(query);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validation_fails_when_parameters_null()
    {
        // Arrange
        var query = new ReadSalesOrderListQuery
        {
            Parameters = null!
        };

        // Act
        var result = await _sut.ValidateAsync(query);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.ErrorCode == "Rule-01");
    }

    [Fact]
    public async Task Validation_clamps_page_number_to_minimum_1()
    {
        // Arrange - PageNumber property is clamped in the init, so 0 becomes 1
        var query = new ReadSalesOrderListQuery
        {
            Parameters = new SalesOrderParameter { PageNumber = 0, PageSize = 10 }
        };

        // Act
        var result = await _sut.ValidateAsync(query);

        // Assert - should be valid because PageNumber is clamped to 1
        result.IsValid.Should().BeTrue();
        query.Parameters.PageNumber.Should().Be(1);
    }

    [Fact]
    public async Task Validation_clamps_page_size_to_maximum_50()
    {
        // Arrange - PageSize property is clamped in the init
        var query = new ReadSalesOrderListQuery
        {
            Parameters = new SalesOrderParameter { PageNumber = 1, PageSize = 100 }
        };

        // Act
        var result = await _sut.ValidateAsync(query);

        // Assert - should be valid because PageSize is clamped to 50
        result.IsValid.Should().BeTrue();
        query.Parameters.PageSize.Should().Be(50);
    }

    [Fact]
    public async Task Validation_fails_when_order_date_from_greater_than_to()
    {
        // Arrange
        var query = new ReadSalesOrderListQuery
        {
            Parameters = new SalesOrderParameter { PageNumber = 1, PageSize = 10 },
            SearchModel = new SalesOrderSearchModel
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

    [Fact]
    public async Task Validation_succeeds_when_order_date_from_equals_to()
    {
        // Arrange
        var sameDate = new DateTime(2014, 1, 1);
        var query = new ReadSalesOrderListQuery
        {
            Parameters = new SalesOrderParameter { PageNumber = 1, PageSize = 10 },
            SearchModel = new SalesOrderSearchModel
            {
                OrderDateFrom = sameDate,
                OrderDateTo = sameDate
            }
        };

        // Act
        var result = await _sut.ValidateAsync(query);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task Validation_fails_when_sales_person_id_invalid(int invalidSalesPersonId)
    {
        // Arrange
        var query = new ReadSalesOrderListQuery
        {
            Parameters = new SalesOrderParameter { PageNumber = 1, PageSize = 10 },
            SearchModel = new SalesOrderSearchModel { SalesPersonId = invalidSalesPersonId }
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
        var query = new ReadSalesOrderListQuery
        {
            Parameters = new SalesOrderParameter { PageNumber = 1, PageSize = 10 },
            SearchModel = new SalesOrderSearchModel { TerritoryId = invalidTerritoryId }
        };

        // Act
        var result = await _sut.ValidateAsync(query);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.ErrorCode == "Rule-06");
    }

    [Fact]
    public async Task Validation_succeeds_when_valid_sales_person_id()
    {
        // Arrange
        var query = new ReadSalesOrderListQuery
        {
            Parameters = new SalesOrderParameter { PageNumber = 1, PageSize = 10 },
            SearchModel = new SalesOrderSearchModel { SalesPersonId = 275 }
        };

        // Act
        var result = await _sut.ValidateAsync(query);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validation_succeeds_when_valid_territory_id()
    {
        // Arrange
        var query = new ReadSalesOrderListQuery
        {
            Parameters = new SalesOrderParameter { PageNumber = 1, PageSize = 10 },
            SearchModel = new SalesOrderSearchModel { TerritoryId = 1 }
        };

        // Act
        var result = await _sut.ValidateAsync(query);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(7)]
    [InlineData(255)]
    public async Task Validation_fails_when_status_is_out_of_range(byte invalidStatus)
    {
        // Arrange
        var query = new ReadSalesOrderListQuery
        {
            Parameters = new SalesOrderParameter { PageNumber = 1, PageSize = 10 },
            SearchModel = new SalesOrderSearchModel { Status = invalidStatus }
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
    public async Task Validation_succeeds_when_status_is_in_valid_range(byte validStatus)
    {
        // Arrange
        var query = new ReadSalesOrderListQuery
        {
            Parameters = new SalesOrderParameter { PageNumber = 1, PageSize = 10 },
            SearchModel = new SalesOrderSearchModel { Status = validStatus }
        };

        // Act
        var result = await _sut.ValidateAsync(query);

        // Assert
        result.IsValid.Should().BeTrue();
    }
}
