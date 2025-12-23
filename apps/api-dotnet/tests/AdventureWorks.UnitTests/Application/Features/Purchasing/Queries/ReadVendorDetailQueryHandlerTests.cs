using AdventureWorks.Application.Features.Purchasing.Queries;
using AdventureWorks.Application.PersistenceContracts.Repositories.Purchasing;
using AdventureWorks.Models.Features.Purchasing;
using FluentAssertions;
using Moq;

namespace AdventureWorks.UnitTests.Application.Features.Purchasing.Queries;

public sealed class ReadVendorDetailQueryHandlerTests
{
    private readonly Mock<IVendorRepository> _mockVendorRepository = new();
    private readonly ReadVendorDetailQueryHandler _sut;

    public ReadVendorDetailQueryHandlerTests()
    {
        _sut = new ReadVendorDetailQueryHandler(_mockVendorRepository.Object);
    }

    [Fact]
    public async Task Handle_returns_vendor_detail_when_found()
    {
        // Arrange
        var detail = new VendorDetailModel
        {
            VendorId = 1496,
            Name = "Advanced Bicycles",
            AccountNumber = "ADVANCED0001",
            CreditRatingLabel = "Superior",
            PreferredVendorStatus = true,
            ActiveFlag = true,
            TotalSpend = 762.94m,
            PoCount = 2,
            AvgPoValue = 381.47m
        };

        _mockVendorRepository
            .Setup(x => x.GetVendorDetailAsync(1496, It.IsAny<CancellationToken>()))
            .ReturnsAsync(detail);

        // Act
        var result = await _sut.Handle(new ReadVendorDetailQuery { VendorId = 1496 }, CancellationToken.None);

        // Assert
        result.Should().BeSameAs(detail);
    }

    [Fact]
    public async Task Handle_throws_key_not_found_exception_when_vendor_does_not_exist()
    {
        // Arrange
        _mockVendorRepository
            .Setup(x => x.GetVendorDetailAsync(9999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((VendorDetailModel?)null);

        // Act
        var act = async () => await _sut.Handle(new ReadVendorDetailQuery { VendorId = 9999 }, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}
