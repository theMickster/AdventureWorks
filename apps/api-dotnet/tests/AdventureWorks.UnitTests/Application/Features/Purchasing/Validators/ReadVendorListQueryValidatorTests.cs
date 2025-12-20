using AdventureWorks.Application.Features.Purchasing.Queries;
using AdventureWorks.Application.Features.Purchasing.Validators;
using AdventureWorks.Common.Filtering;
using FluentAssertions;

namespace AdventureWorks.UnitTests.Application.Features.Purchasing.Validators;

public sealed class ReadVendorListQueryValidatorTests
{
    private readonly ReadVendorListQueryValidator _sut = new();

    [Fact]
    public async Task Validation_succeeds_with_valid_query()
    {
        // Arrange
        var query = new ReadVendorListQuery
        {
            Parameters = new VendorParameter { PageNumber = 1, PageSize = 25 }
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
        var query = new ReadVendorListQuery
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
        var query = new ReadVendorListQuery
        {
            Parameters = new VendorParameter { PageNumber = 0, PageSize = 25 }
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
        var query = new ReadVendorListQuery
        {
            Parameters = new VendorParameter { PageNumber = 1, PageSize = 100 }
        };

        // Act
        var result = await _sut.ValidateAsync(query);

        // Assert - should be valid because PageSize is clamped to 50
        result.IsValid.Should().BeTrue();
        query.Parameters.PageSize.Should().Be(50);
    }

    [Fact]
    public async Task Validation_defaults_page_size_to_25()
    {
        // Arrange
        var parameters = new VendorParameter { PageNumber = 1 };

        // Assert
        parameters.PageSize.Should().Be(25);
    }

    [Theory]
    [InlineData((byte)1)]
    [InlineData((byte)2)]
    [InlineData((byte)3)]
    [InlineData((byte)4)]
    [InlineData((byte)5)]
    public async Task Validation_succeeds_when_credit_rating_is_in_valid_range(byte validCreditRating)
    {
        // Arrange
        var query = new ReadVendorListQuery
        {
            Parameters = new VendorParameter { PageNumber = 1, PageSize = 25, CreditRating = validCreditRating }
        };

        // Act
        var result = await _sut.ValidateAsync(query);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData((byte)0)]
    [InlineData((byte)6)]
    [InlineData((byte)9)]
    [InlineData((byte)255)]
    public async Task Validation_fails_when_credit_rating_is_out_of_range(byte invalidCreditRating)
    {
        // Arrange
        var query = new ReadVendorListQuery
        {
            Parameters = new VendorParameter { PageNumber = 1, PageSize = 25, CreditRating = invalidCreditRating }
        };

        // Act
        var result = await _sut.ValidateAsync(query);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.ErrorCode == "Rule-02");
    }

    [Fact]
    public async Task Validation_succeeds_when_credit_rating_is_null()
    {
        // Arrange
        var query = new ReadVendorListQuery
        {
            Parameters = new VendorParameter { PageNumber = 1, PageSize = 25, CreditRating = null }
        };

        // Act
        var result = await _sut.ValidateAsync(query);

        // Assert
        result.IsValid.Should().BeTrue();
    }
}
