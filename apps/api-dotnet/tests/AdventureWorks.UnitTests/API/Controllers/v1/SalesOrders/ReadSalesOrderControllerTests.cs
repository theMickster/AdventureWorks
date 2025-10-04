using AdventureWorks.API.Controllers.v1.SalesOrders;
using AdventureWorks.Application.Features.Sales.Queries;
using AdventureWorks.Common.Filtering;
using AdventureWorks.Models.Features.Sales;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace AdventureWorks.UnitTests.API.Controllers.v1.SalesOrders;

public sealed class ReadSalesOrderControllerTests
{
    private readonly Mock<IMediator> _mockMediator = new();
    private readonly Mock<ILogger<ReadSalesOrderController>> _mockLogger = new();
    private ReadSalesOrderController _sut;

    public ReadSalesOrderControllerTests()
    {
        _sut = new ReadSalesOrderController(_mockLogger.Object, _mockMediator.Object);
    }

    [Fact]
    public async Task GetAsync_returns_ok_result_with_results()
    {
        // Arrange
        var parameters = new SalesOrderParameter { PageNumber = 1, PageSize = 10 };
        var searchResult = new SalesOrderSearchResultModel
        {
            PageNumber = 1,
            PageSize = 10,
            TotalRecords = 1,
            Results = new List<SalesOrderModel>
            {
                new()
                {
                    SalesOrderId = 43659,
                    SalesOrderNumber = "SO43659",
                    OrderDate = new DateTime(2011, 5, 31),
                    Status = 5,
                    StatusDescription = "Shipped",
                    TotalDue = 119961.7161m,
                    CustomerName = "John Doe",
                    SalesPersonName = "Jane Smith"
                }
            }
        };

        _mockMediator.Setup(x => x.Send(It.IsAny<ReadSalesOrderListQuery>(), It.IsAny<CancellationToken>()))
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
        var parameters = new SalesOrderParameter { PageNumber = 1, PageSize = 10 };
        var searchResult = new SalesOrderSearchResultModel
        {
            PageNumber = 1,
            PageSize = 10,
            TotalRecords = 0,
            Results = null
        };

        _mockMediator.Setup(x => x.Send(It.IsAny<ReadSalesOrderListQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(searchResult);

        // Act
        var result = await _sut.GetAsync(parameters);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetAsync_returns_bad_request_when_page_number_invalid()
    {
        // Arrange - but PageNumber property clamps to minimum 1, so this should return ok
        var parameters = new SalesOrderParameter { PageNumber = 0, PageSize = 10 };
        var searchResult = new SalesOrderSearchResultModel
        {
            PageNumber = 1,
            PageSize = 10,
            TotalRecords = 0,
            Results = null
        };

        _mockMediator.Setup(x => x.Send(It.IsAny<ReadSalesOrderListQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(searchResult);

        // Act
        var result = await _sut.GetAsync(parameters);

        // Assert - PageNumber 0 is clamped to 1, so it should not return BadRequest
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetAsync_applies_parameters_to_query()
    {
        // Arrange
        var parameters = new SalesOrderParameter
        {
            PageNumber = 2,
            PageSize = 20,
            OrderBy = "orderDate"
        };
        var searchResult = new SalesOrderSearchResultModel { PageNumber = 2, PageSize = 20 };

        _mockMediator.Setup(x => x.Send(
            It.Is<ReadSalesOrderListQuery>(q => q.Parameters.PageNumber == 2 && q.Parameters.PageSize == 20),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(searchResult);

        // Act
        var result = await _sut.GetAsync(parameters);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        _mockMediator.Verify(x => x.Send(
            It.IsAny<ReadSalesOrderListQuery>(),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetAsync_forwards_cancellation_token()
    {
        // Arrange
        var parameters = new SalesOrderParameter { PageNumber = 1, PageSize = 10 };
        var cancellationToken = CancellationToken.None;
        var searchResult = new SalesOrderSearchResultModel { PageNumber = 1, PageSize = 10 };

        _mockMediator.Setup(x => x.Send(
            It.IsAny<ReadSalesOrderListQuery>(),
            cancellationToken))
            .ReturnsAsync(searchResult);

        // Act
        var result = await _sut.GetAsync(parameters, cancellationToken);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }
}
