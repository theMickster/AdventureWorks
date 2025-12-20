using AdventureWorks.Application.Features.Purchasing.Queries;
using AdventureWorks.Application.PersistenceContracts.Repositories.Purchasing;
using AdventureWorks.Common.Filtering;
using AdventureWorks.Models.Features.Purchasing;
using AdventureWorks.UnitTests.Setup.Fakes;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Moq;

namespace AdventureWorks.UnitTests.Application.Features.Purchasing.Queries;

public sealed class ReadVendorListQueryHandlerTests : UnitTestBase
{
    private readonly Mock<IVendorRepository> _mockVendorRepository = new();
    private readonly Mock<IValidator<ReadVendorListQuery>> _mockValidator = new();
    private readonly ReadVendorListQueryHandler _sut;

    public ReadVendorListQueryHandlerTests()
    {
        _mockValidator
            .Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<ReadVendorListQuery>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());
        _sut = new ReadVendorListQueryHandler(_mockVendorRepository.Object, _mockValidator.Object);
    }

    [Fact]
    public async Task Handle_returns_success_with_results()
    {
        // Arrange
        var parameters = new VendorParameter { PageNumber = 1, PageSize = 25 };
        var query = new ReadVendorListQuery { Parameters = parameters };

        var vendor = new VendorModel
        {
            VendorId = 1576,
            Name = "Superior Bicycles",
            AccountNumber = "SUPERIOR0001",
            CreditRatingLabel = "Superior",
            PreferredVendorStatus = true,
            ActiveFlag = true,
            TotalSpend = 5034266.74m,
            PoCount = 50,
            IsHighRisk = false
        };

        _mockVendorRepository.Setup(x =>
            x.GetVendorsAsync(It.IsAny<VendorParameter>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((new[] { vendor }.ToList().AsReadOnly(), 1));

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.PageNumber.Should().Be(1);
        result.PageSize.Should().Be(25);
        result.TotalRecords.Should().Be(1);
        result.Results.Should().HaveCount(1);
        result.Results![0].Name.Should().Be("Superior Bicycles");
        result.Results[0].CreditRatingLabel.Should().Be("Superior");
    }

    [Fact]
    public async Task Handle_returns_success_with_empty_results()
    {
        // Arrange
        var parameters = new VendorParameter { PageNumber = 1, PageSize = 25 };
        var query = new ReadVendorListQuery { Parameters = parameters };

        _mockVendorRepository.Setup(x =>
            x.GetVendorsAsync(It.IsAny<VendorParameter>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<VendorModel>().AsReadOnly(), 0));

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.TotalRecords.Should().Be(0);
        result.Results.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_preserves_total_records_when_page_is_out_of_range()
    {
        // Arrange — repo returns empty list but with a non-zero total (e.g. page beyond the last page)
        var parameters = new VendorParameter { PageNumber = 99, PageSize = 25 };
        var query = new ReadVendorListQuery { Parameters = parameters };

        _mockVendorRepository.Setup(x =>
            x.GetVendorsAsync(It.IsAny<VendorParameter>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<VendorModel>().AsReadOnly(), 104));

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.TotalRecords.Should().Be(104);
        result.Results.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_uses_default_page_size_of_25()
    {
        // Arrange
        var parameters = new VendorParameter { PageNumber = 1 };
        var query = new ReadVendorListQuery { Parameters = parameters };

        _mockVendorRepository.Setup(x =>
            x.GetVendorsAsync(It.IsAny<VendorParameter>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<VendorModel>().AsReadOnly(), 0));

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.PageSize.Should().Be(25);
    }

    [Fact]
    public async Task Handle_throws_validation_exception_when_query_is_invalid()
    {
        // Arrange — use FakeFailureValidator to simulate a validation failure (established pattern in this project)
        var handler = new ReadVendorListQueryHandler(
            _mockVendorRepository.Object,
            new FakeFailureValidator<ReadVendorListQuery>("Parameters", "Parameters cannot be null"));

        var query = new ReadVendorListQuery { Parameters = new VendorParameter { PageNumber = 1, PageSize = 25 } };

        // Act
        var act = async () => await handler.Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ValidationException>();
    }
}
