using AdventureWorks.API.Controllers.v1.Purchasing;
using AdventureWorks.Application.Features.Purchasing.Queries;
using AdventureWorks.Models.Features.Purchasing;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace AdventureWorks.UnitTests.API.Controllers.v1.Purchasing;

[ExcludeFromCodeCoverage]
public sealed class ReadPurchasingAnalyticsControllerTests : UnitTestBase
{
    private readonly Mock<IMediator> _mockMediator = new();
    private readonly ReadPurchasingAnalyticsController _sut;

    public ReadPurchasingAnalyticsControllerTests()
    {
        _sut = new ReadPurchasingAnalyticsController(_mockMediator.Object);
    }

    [Fact]
    public void Controller_throws_correct_exceptions()
    {
        _ = ((Action)(() => _ = new ReadPurchasingAnalyticsController(null!)))
            .Should().Throw<ArgumentNullException>("because we expect a null argument exception.")
            .And.ParamName.Should().Be("mediator");
    }

    [Fact]
    public async Task GetAnalyticsAsync_returns_ok_Async()
    {
        var testModel = new PurchasingAnalyticsModel
        {
            ParetoData =
            [
                new VendorSpendModel { VendorId = 1576, VendorName = "Superior Bicycles", TotalSpend = 5034266.74m, CumulativePercent = 100m }
            ],
            PipelineSummary =
            [
                new PurchaseOrderPipelineStatusModel { StatusLabel = "Pending", PoCount = 225, TotalValue = 1000m },
                new PurchaseOrderPipelineStatusModel { StatusLabel = "Approved", PoCount = 12, TotalValue = 2000m },
                new PurchaseOrderPipelineStatusModel { StatusLabel = "Rejected", PoCount = 86, TotalValue = 3000m },
                new PurchaseOrderPipelineStatusModel { StatusLabel = "Complete", PoCount = 3689, TotalValue = 4000m }
            ]
        };

        _mockMediator
            .Setup(x => x.Send(It.IsAny<ReadPurchasingAnalyticsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(testModel);

        var result = await _sut.GetAnalyticsAsync(CancellationToken.None);
        var objectResult = result as OkObjectResult;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.OK);
            objectResult.Value.Should().BeOfType<PurchasingAnalyticsModel>();
            objectResult.Value.Should().BeSameAs(testModel);
        }
    }

    [Fact]
    public async Task GetAnalyticsAsync_sends_correct_query_Async()
    {
        _mockMediator
            .Setup(x => x.Send(It.IsAny<ReadPurchasingAnalyticsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PurchasingAnalyticsModel());

        await _sut.GetAnalyticsAsync(CancellationToken.None);

        _mockMediator.Verify(
            x => x.Send(It.IsAny<ReadPurchasingAnalyticsQuery>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
