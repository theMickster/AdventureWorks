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

public sealed class ReadVendorPurchaseOrdersQueryHandlerTests
{
    private readonly Mock<IVendorRepository> _mockVendorRepository = new();
    private readonly Mock<IValidator<ReadVendorPurchaseOrdersQuery>> _mockValidator = new();
    private readonly ReadVendorPurchaseOrdersQueryHandler _sut;

    public ReadVendorPurchaseOrdersQueryHandlerTests()
    {
        _mockValidator
            .Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<ReadVendorPurchaseOrdersQuery>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());
        _sut = new ReadVendorPurchaseOrdersQueryHandler(_mockVendorRepository.Object, _mockValidator.Object);
    }

    [Fact]
    public async Task Handle_returns_success_with_results()
    {
        // Arrange
        var parameters = new VendorPurchaseOrderParameter { PageNumber = 1, PageSize = 25 };
        var query = new ReadVendorPurchaseOrdersQuery { VendorId = 1496, Parameters = parameters };

        var order = new PurchaseOrderSummaryModel
        {
            PurchaseOrderId = 3932,
            OrderDate = new DateTime(2014, 7, 30),
            DueDate = new DateTime(2014, 8, 13),
            Status = 1,
            StatusLabel = "Pending",
            TotalDue = 302.44m
        };

        _mockVendorRepository
            .Setup(x => x.GetVendorPurchaseOrdersAsync(1496, parameters, It.IsAny<CancellationToken>()))
            .ReturnsAsync((new[] { order }.ToList().AsReadOnly(), 1, true));

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.PageNumber.Should().Be(1);
        result.PageSize.Should().Be(25);
        result.TotalRecords.Should().Be(1);
        result.Results.Should().ContainSingle();
        result.Results![0].PurchaseOrderId.Should().Be(3932);
    }

    [Fact]
    public async Task Handle_throws_key_not_found_exception_when_vendor_does_not_exist()
    {
        // Arrange
        var parameters = new VendorPurchaseOrderParameter { PageNumber = 1 };
        var query = new ReadVendorPurchaseOrdersQuery { VendorId = 9999, Parameters = parameters };

        _mockVendorRepository
            .Setup(x => x.GetVendorPurchaseOrdersAsync(9999, parameters, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Array.Empty<PurchaseOrderSummaryModel>(), 0, false));

        // Act
        var act = async () => await _sut.Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task Handle_returns_empty_page_when_vendor_exists_with_no_matching_purchase_orders()
    {
        // Arrange
        var parameters = new VendorPurchaseOrderParameter { PageNumber = 1 };
        var query = new ReadVendorPurchaseOrdersQuery { VendorId = 1502, Parameters = parameters };

        _mockVendorRepository
            .Setup(x => x.GetVendorPurchaseOrdersAsync(1502, parameters, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Array.Empty<PurchaseOrderSummaryModel>(), 0, true));

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.TotalRecords.Should().Be(0);
        result.Results.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_throws_validation_exception_when_query_is_invalid()
    {
        // Arrange
        var handler = new ReadVendorPurchaseOrdersQueryHandler(
            _mockVendorRepository.Object,
            new FakeFailureValidator<ReadVendorPurchaseOrdersQuery>("Parameters", "Parameters cannot be null"));

        var query = new ReadVendorPurchaseOrdersQuery { VendorId = 1496, Parameters = new VendorPurchaseOrderParameter { PageNumber = 1 } };

        // Act
        var act = async () => await handler.Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ValidationException>();
    }
}
