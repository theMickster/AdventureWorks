using AdventureWorks.API.Controllers.v1.WorkOrders;
using AdventureWorks.Application.Features.Production.Queries;
using AdventureWorks.Common.Filtering;
using AdventureWorks.Models.Features.Production;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace AdventureWorks.UnitTests.API.Controllers.v1.WorkOrders;

public sealed class ReadWorkOrderControllerTests
{
    private readonly Mock<IMediator> _mockMediator = new();
    private readonly ReadWorkOrderController _sut;

    public ReadWorkOrderControllerTests()
    {
        _sut = new ReadWorkOrderController(_mockMediator.Object);
    }

    [Fact]
    public async Task GetAsync_returns_ok_result_with_results()
    {
        // Arrange
        var parameters = new WorkOrderParameter { PageNumber = 1, PageSize = 10 };
        var searchResult = new WorkOrderSearchResultModel
        {
            PageNumber = 1,
            PageSize = 10,
            TotalRecords = 1,
            Results = new List<WorkOrderModel>
            {
                new()
                {
                    WorkOrderId = 13,
                    ProductId = 747,
                    ProductName = "HL Mountain Frame - Black, 38",
                    OrderedQty = 4,
                    StockedQty = 4,
                    ScrappedQty = 0,
                    YieldRate = 100m,
                    StartDate = new DateTime(2011, 6, 3),
                    EndDate = new DateTime(2011, 6, 19),
                    DueDate = new DateTime(2011, 6, 14),
                    IsCompletedLate = true
                }
            }
        };

        _mockMediator.Setup(x => x.Send(It.IsAny<ReadWorkOrderListQuery>(), It.IsAny<CancellationToken>()))
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
        var parameters = new WorkOrderParameter { PageNumber = 1, PageSize = 10 };
        var searchResult = new WorkOrderSearchResultModel
        {
            PageNumber = 1,
            PageSize = 10,
            TotalRecords = 0,
            Results = null
        };

        _mockMediator.Setup(x => x.Send(It.IsAny<ReadWorkOrderListQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(searchResult);

        // Act
        var result = await _sut.GetAsync(parameters);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetAsync_does_not_build_search_model_when_no_filters_provided()
    {
        // Arrange
        var parameters = new WorkOrderParameter { PageNumber = 1, PageSize = 10 };
        var searchResult = new WorkOrderSearchResultModel { PageNumber = 1, PageSize = 10 };

        _mockMediator.Setup(x => x.Send(It.IsAny<ReadWorkOrderListQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(searchResult);

        // Act
        await _sut.GetAsync(parameters);

        // Assert
        _mockMediator.Verify(x => x.Send(
            It.Is<ReadWorkOrderListQuery>(q => q.SearchModel == null),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetAsync_builds_search_model_when_product_id_filter_provided()
    {
        // Arrange
        var parameters = new WorkOrderParameter { PageNumber = 1, PageSize = 10, ProductId = 747 };
        var searchResult = new WorkOrderSearchResultModel { PageNumber = 1, PageSize = 10 };

        _mockMediator.Setup(x => x.Send(It.IsAny<ReadWorkOrderListQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(searchResult);

        // Act
        await _sut.GetAsync(parameters);

        // Assert
        _mockMediator.Verify(x => x.Send(
            It.Is<ReadWorkOrderListQuery>(q => q.SearchModel != null && q.SearchModel.ProductId == 747),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetAsync_applies_parameters_to_query()
    {
        // Arrange
        var parameters = new WorkOrderParameter { PageNumber = 2, PageSize = 20 };
        var searchResult = new WorkOrderSearchResultModel { PageNumber = 2, PageSize = 20 };

        _mockMediator.Setup(x => x.Send(
            It.Is<ReadWorkOrderListQuery>(q => q.Parameters.PageNumber == 2 && q.Parameters.PageSize == 20),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(searchResult);

        // Act
        var result = await _sut.GetAsync(parameters);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        _mockMediator.Verify(x => x.Send(
            It.IsAny<ReadWorkOrderListQuery>(),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetAsync_forwards_cancellation_token()
    {
        // Arrange
        var parameters = new WorkOrderParameter { PageNumber = 1, PageSize = 10 };
        var cancellationToken = CancellationToken.None;
        var searchResult = new WorkOrderSearchResultModel { PageNumber = 1, PageSize = 10 };

        _mockMediator.Setup(x => x.Send(
            It.IsAny<ReadWorkOrderListQuery>(),
            cancellationToken))
            .ReturnsAsync(searchResult);

        // Act
        var result = await _sut.GetAsync(parameters, cancellationToken);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }
}
