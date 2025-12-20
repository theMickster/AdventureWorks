using AdventureWorks.API.Controllers.v1.Vendors;
using AdventureWorks.Application.Features.Purchasing.Queries;
using AdventureWorks.Common.Filtering;
using AdventureWorks.Models.Features.Purchasing;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace AdventureWorks.UnitTests.API.Controllers.v1.Vendors;

public sealed class ReadVendorControllerTests
{
    private readonly Mock<IMediator> _mockMediator = new();
    private readonly ReadVendorController _sut;

    public ReadVendorControllerTests()
    {
        _sut = new ReadVendorController(_mockMediator.Object);
    }

    [Fact]
    public async Task GetAsync_returns_ok_result_with_results()
    {
        // Arrange
        var parameters = new VendorParameter { PageNumber = 1, PageSize = 25 };
        var searchResult = new VendorSearchResultModel
        {
            PageNumber = 1,
            PageSize = 25,
            TotalRecords = 1,
            Results = new List<VendorModel>
            {
                new()
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
                }
            }
        };

        _mockMediator.Setup(x => x.Send(It.IsAny<ReadVendorListQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(searchResult);

        // Act
        var result = await _sut.GetAsync(parameters);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = (OkObjectResult)result;
        okResult.Value.Should().BeEquivalentTo(searchResult);
    }

    [Fact]
    public async Task GetAsync_returns_ok_result_with_empty_results()
    {
        // Arrange
        var parameters = new VendorParameter { PageNumber = 1, PageSize = 25 };
        var searchResult = new VendorSearchResultModel
        {
            PageNumber = 1,
            PageSize = 25,
            TotalRecords = 0,
            Results = null
        };

        _mockMediator.Setup(x => x.Send(It.IsAny<ReadVendorListQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(searchResult);

        // Act
        var result = await _sut.GetAsync(parameters);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetAsync_applies_parameters_to_query()
    {
        // Arrange
        var parameters = new VendorParameter { PageNumber = 2, PageSize = 20, CreditRating = 4 };
        var searchResult = new VendorSearchResultModel { PageNumber = 2, PageSize = 20 };

        _mockMediator.Setup(x => x.Send(
            It.Is<ReadVendorListQuery>(q => q.Parameters.PageNumber == 2 && q.Parameters.PageSize == 20 && q.Parameters.CreditRating == 4),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(searchResult);

        // Act
        var result = await _sut.GetAsync(parameters);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        _mockMediator.Verify(x => x.Send(
            It.IsAny<ReadVendorListQuery>(),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetAsync_forwards_cancellation_token()
    {
        // Arrange
        var parameters = new VendorParameter { PageNumber = 1, PageSize = 25 };
        var cancellationToken = CancellationToken.None;
        var searchResult = new VendorSearchResultModel { PageNumber = 1, PageSize = 25 };

        _mockMediator.Setup(x => x.Send(
            It.IsAny<ReadVendorListQuery>(),
            cancellationToken))
            .ReturnsAsync(searchResult);

        // Act
        var result = await _sut.GetAsync(parameters, cancellationToken);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }
}
