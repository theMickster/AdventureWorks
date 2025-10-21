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

public sealed class SearchSalesOrderControllerTests
{
    private readonly Mock<IMediator> _mockMediator = new();
    private readonly Mock<ILogger<ReadSalesOrderController>> _mockLogger = new();
    private readonly ReadSalesOrderController _sut;

    public SearchSalesOrderControllerTests()
    {
        _sut = new ReadSalesOrderController(_mockLogger.Object, _mockMediator.Object);
    }

    [Fact]
    public async Task SearchAsync_WithValidSearchModel_Returns200()
    {
        // Arrange
        var parameters = new SalesOrderParameter { PageNumber = 1, PageSize = 10 };
        var searchModel = new SalesOrderSearchModel { AccountNumber = "10-4020-000676", Status = 5 };
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
        var result = await _sut.SearchAsync(parameters, searchModel);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = (OkObjectResult)result;
        okResult.Value.Should().BeEquivalentTo(searchResult);
    }

    [Fact]
    public async Task SearchAsync_WithNullSearchModel_Returns200()
    {
        // Arrange
        var parameters = new SalesOrderParameter { PageNumber = 1, PageSize = 10 };
        var searchResult = new SalesOrderSearchResultModel
        {
            PageNumber = 1,
            PageSize = 10,
            TotalRecords = 0,
            Results = new List<SalesOrderModel>()
        };

        _mockMediator.Setup(x => x.Send(It.IsAny<ReadSalesOrderListQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(searchResult);

        // Act
        var result = await _sut.SearchAsync(parameters, null);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task SearchAsync_ForwardsCancellationToken()
    {
        // Arrange
        var parameters = new SalesOrderParameter { PageNumber = 1, PageSize = 10 };
        var searchModel = new SalesOrderSearchModel { AccountNumber = "10-4020-000676" };
        using var cts = new CancellationTokenSource();
        var token = cts.Token;

        CancellationToken capturedToken = default;

        _mockMediator
            .Setup(x => x.Send(It.IsAny<ReadSalesOrderListQuery>(), It.IsAny<CancellationToken>()))
            .Callback<IRequest<SalesOrderSearchResultModel>, CancellationToken>((_, ct) => capturedToken = ct)
            .ReturnsAsync(new SalesOrderSearchResultModel { PageNumber = 1, PageSize = 10 });

        // Act
        await _sut.SearchAsync(parameters, searchModel, token);

        // Assert
        capturedToken.Should().Be(token);
    }
}
