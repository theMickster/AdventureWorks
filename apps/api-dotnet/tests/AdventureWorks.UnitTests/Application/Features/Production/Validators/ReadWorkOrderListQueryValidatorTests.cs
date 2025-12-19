using AdventureWorks.Application.Features.Production.Queries;
using AdventureWorks.Application.Features.Production.Validators;
using AdventureWorks.Common.Filtering;
using FluentAssertions;

namespace AdventureWorks.UnitTests.Application.Features.Production.Validators;

public sealed class ReadWorkOrderListQueryValidatorTests
{
    private readonly ReadWorkOrderListQueryValidator _sut = new();

    [Fact]
    public async Task Validation_succeeds_with_valid_query()
    {
        // Arrange
        var query = new ReadWorkOrderListQuery
        {
            Parameters = new WorkOrderParameter { PageNumber = 1, PageSize = 10 }
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
        var query = new ReadWorkOrderListQuery
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
        // Arrange
        var query = new ReadWorkOrderListQuery
        {
            Parameters = new WorkOrderParameter { PageNumber = 0, PageSize = 10 }
        };

        // Act
        var result = await _sut.ValidateAsync(query);

        // Assert
        result.IsValid.Should().BeTrue();
        query.Parameters.PageNumber.Should().Be(1);
    }

    [Fact]
    public async Task Validation_clamps_page_size_to_maximum_50()
    {
        // Arrange
        var query = new ReadWorkOrderListQuery
        {
            Parameters = new WorkOrderParameter { PageNumber = 1, PageSize = 100 }
        };

        // Act
        var result = await _sut.ValidateAsync(query);

        // Assert
        result.IsValid.Should().BeTrue();
        query.Parameters.PageSize.Should().Be(50);
    }

    [Fact]
    public async Task Parameters_default_page_size_is_25()
    {
        // Arrange
        var parameters = new WorkOrderParameter();

        // Assert
        parameters.PageSize.Should().Be(25);
    }

    [Fact]
    public async Task Validation_fails_when_start_date_greater_than_end_date()
    {
        // Arrange
        var query = new ReadWorkOrderListQuery
        {
            Parameters = new WorkOrderParameter { PageNumber = 1, PageSize = 10 },
            SearchModel = new WorkOrderSearchModel
            {
                StartDate = new DateTime(2014, 6, 30),
                EndDate = new DateTime(2014, 1, 1)
            }
        };

        // Act
        var result = await _sut.ValidateAsync(query);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.ErrorCode == "Rule-02");
    }

    [Fact]
    public async Task Validation_succeeds_when_start_date_equals_end_date()
    {
        // Arrange
        var sameDate = new DateTime(2014, 1, 1);
        var query = new ReadWorkOrderListQuery
        {
            Parameters = new WorkOrderParameter { PageNumber = 1, PageSize = 10 },
            SearchModel = new WorkOrderSearchModel
            {
                StartDate = sameDate,
                EndDate = sameDate
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
    public async Task Validation_fails_when_product_id_invalid(int invalidProductId)
    {
        // Arrange
        var query = new ReadWorkOrderListQuery
        {
            Parameters = new WorkOrderParameter { PageNumber = 1, PageSize = 10 },
            SearchModel = new WorkOrderSearchModel { ProductId = invalidProductId }
        };

        // Act
        var result = await _sut.ValidateAsync(query);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.ErrorCode == "Rule-03");
    }

    [Fact]
    public async Task Validation_succeeds_when_valid_product_id()
    {
        // Arrange
        var query = new ReadWorkOrderListQuery
        {
            Parameters = new WorkOrderParameter { PageNumber = 1, PageSize = 10 },
            SearchModel = new WorkOrderSearchModel { ProductId = 747 }
        };

        // Act
        var result = await _sut.ValidateAsync(query);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData((short)0)]
    [InlineData((short)-1)]
    public async Task Validation_fails_when_scrap_reason_id_invalid(short invalidScrapReasonId)
    {
        // Arrange
        var query = new ReadWorkOrderListQuery
        {
            Parameters = new WorkOrderParameter { PageNumber = 1, PageSize = 10 },
            SearchModel = new WorkOrderSearchModel { ScrapReasonId = invalidScrapReasonId }
        };

        // Act
        var result = await _sut.ValidateAsync(query);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.ErrorCode == "Rule-04");
    }

    [Fact]
    public async Task Validation_succeeds_when_valid_scrap_reason_id()
    {
        // Arrange
        var query = new ReadWorkOrderListQuery
        {
            Parameters = new WorkOrderParameter { PageNumber = 1, PageSize = 10 },
            SearchModel = new WorkOrderSearchModel { ScrapReasonId = 7 }
        };

        // Act
        var result = await _sut.ValidateAsync(query);

        // Assert
        result.IsValid.Should().BeTrue();
    }
}
