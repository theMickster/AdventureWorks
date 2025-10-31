using AdventureWorks.API.Controllers.v1.Sales;
using AdventureWorks.Application.Features.Sales.Queries;
using AdventureWorks.Models.Features.Sales;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace AdventureWorks.UnitTests.API.Controllers.v1.Sales;

[ExcludeFromCodeCoverage]
public sealed class ReadSalesDashboardControllerTests : UnitTestBase
{
    private readonly Mock<IMediator> _mockMediator = new();
    private readonly ReadSalesDashboardController _sut;

    public ReadSalesDashboardControllerTests()
    {
        _sut = new ReadSalesDashboardController(_mockMediator.Object);
    }

    [Fact]
    public void Controller_throws_correct_exceptions()
    {
        _ = ((Action)(() => _ = new ReadSalesDashboardController(null!)))
            .Should().Throw<ArgumentNullException>("because we expect a null argument exception.")
            .And.ParamName.Should().Be("mediator");
    }

    [Fact]
    public async Task GetDashboardAsync_returns_ok_Async()
    {
        var testModel = new SalesDashboardModel
        {
            TotalRevenue = 100000m,
            OrderCount = 500,
            AverageOrderValue = 200m,
            TopPerformers = [],
            TerritoryBreakdown = [],
            MonthlySalesTrend = []
        };

        _mockMediator
            .Setup(x => x.Send(It.IsAny<ReadSalesDashboardQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(testModel);

        var result = await _sut.GetDashboardAsync(CancellationToken.None);
        var objectResult = result as OkObjectResult;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.OK);
            objectResult.Value.Should().BeOfType<SalesDashboardModel>();
        }
    }

    [Fact]
    public async Task GetDashboardAsync_sends_correct_query_Async()
    {
        _mockMediator
            .Setup(x => x.Send(It.IsAny<ReadSalesDashboardQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SalesDashboardModel());

        await _sut.GetDashboardAsync(CancellationToken.None);

        _mockMediator.Verify(
            x => x.Send(It.IsAny<ReadSalesDashboardQuery>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
