using AdventureWorks.API.Controllers.v1.Stores;
using AdventureWorks.Application.Features.Sales.Queries;
using AdventureWorks.Models.Features.Sales;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Net;

namespace AdventureWorks.UnitTests.API.Controllers.v1.Stores;

[ExcludeFromCodeCoverage]
public sealed class ReadStorePerformanceControllerTests : UnitTestBase
{
    private readonly Mock<ILogger<ReadStorePerformanceController>> _mockLogger = new();
    private readonly Mock<IMediator> _mockMediator = new();
    private readonly ReadStorePerformanceController _sut;

    public ReadStorePerformanceControllerTests()
    {
        _sut = new ReadStorePerformanceController(_mockLogger.Object, _mockMediator.Object);
    }

    [Fact]
    public void Constructor_throws_correct_exceptions()
    {
        using (new AssertionScope())
        {
            _ = ((Action)(() => _ = new ReadStorePerformanceController(null!, _mockMediator.Object)))
                .Should().Throw<ArgumentNullException>("because we expect a null argument exception.")
                .And.ParamName.Should().Be("logger");

            _ = ((Action)(() => _ = new ReadStorePerformanceController(_mockLogger.Object, null!)))
                .Should().Throw<ArgumentNullException>("because we expect a null argument exception.")
                .And.ParamName.Should().Be("mediator");
        }
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-999)]
    public async Task GetAsync_invalid_storeId_returns_bad_request(int storeId)
    {
        var result = await _sut.GetAsync(storeId);
        var objectResult = result as BadRequestObjectResult;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
            objectResult!.Value!.ToString().Should().Be("A valid store id must be specified.");
        }
    }

    [Fact]
    public async Task GetAsync_returns_not_found_when_query_returns_nullAsync()
    {
        _mockMediator.Setup(x => x.Send(It.IsAny<ReadStorePerformanceQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((StorePerformanceModel?)null);

        var result = await _sut.GetAsync(2534);

        var notFound = result as NotFoundObjectResult;

        using (new AssertionScope())
        {
            notFound.Should().NotBeNull();
            notFound!.StatusCode.Should().Be((int)HttpStatusCode.NotFound);
            notFound!.Value!.ToString().Should().Be("Unable to locate store performance.");
        }
    }

    [Fact]
    public async Task GetAsync_returns_ok_with_modelAsync()
    {
        var output = new StorePerformanceModel
        {
            StoreId = 2534,
            StoreName = "Bike World",
            RevenueYtd = 25_000m,
            OrderCount = 5,
            AverageOrderValue = 5_000m,
            CustomerCount = 12,
            Year = DateTime.UtcNow.Year
        };

        _mockMediator.Setup(x => x.Send(It.IsAny<ReadStorePerformanceQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(output);

        var result = await _sut.GetAsync(2534);

        var okResult = result as OkObjectResult;

        using (new AssertionScope())
        {
            okResult.Should().NotBeNull();
            okResult!.StatusCode.Should().Be((int)HttpStatusCode.OK);
            okResult.Value.Should().Be(output);
        }
    }

    [Fact]
    public async Task GetAsync_sends_correct_storeId_to_mediatorAsync()
    {
        const int storeId = 2534;

        _mockMediator.Setup(x => x.Send(It.IsAny<ReadStorePerformanceQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((StorePerformanceModel?)null);

        await _sut.GetAsync(storeId);

        _mockMediator.Verify(x => x.Send(
            It.Is<ReadStorePerformanceQuery>(q => q.StoreId == storeId),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetAsync_forwards_cancellation_token_to_mediatorAsync()
    {
        const int storeId = 2534;

        using var cts = new CancellationTokenSource();

        _mockMediator.Setup(x => x.Send(It.IsAny<ReadStorePerformanceQuery>(), cts.Token))
            .ReturnsAsync((StorePerformanceModel?)null);

        await _sut.GetAsync(storeId, cts.Token);

        _mockMediator.Verify(x => x.Send(
            It.Is<ReadStorePerformanceQuery>(q => q.StoreId == storeId),
            cts.Token), Times.Once);
    }

    [Fact]
    public async Task GetAsync_returns_ok_with_zero_values_when_store_has_no_ordersAsync()
    {
        // AC Scenario: "Store with no orders returns zero values" — the store exists, so the
        // controller must return 200 OK with a populated (all-zero) model. 404 is reserved
        // strictly for the store-does-not-exist case, never for "store exists with empty data".
        var output = new StorePerformanceModel
        {
            StoreId = 999,
            StoreName = "Catalog Store",
            RevenueYtd = 0m,
            OrderCount = 0,
            AverageOrderValue = 0m,
            CustomerCount = 0,
            Year = DateTime.UtcNow.Year
        };

        _mockMediator.Setup(x => x.Send(It.IsAny<ReadStorePerformanceQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(output);

        var result = await _sut.GetAsync(999);

        var okResult = result as OkObjectResult;

        using (new AssertionScope())
        {
            okResult.Should().NotBeNull();
            okResult!.StatusCode.Should().Be((int)HttpStatusCode.OK);
            okResult.Value.Should().BeSameAs(output);
            var model = okResult.Value as StorePerformanceModel;
            model.Should().NotBeNull();
            model!.RevenueYtd.Should().Be(0m);
            model.OrderCount.Should().Be(0);
            model.AverageOrderValue.Should().Be(0m);
            model.CustomerCount.Should().Be(0);
        }
    }
}
