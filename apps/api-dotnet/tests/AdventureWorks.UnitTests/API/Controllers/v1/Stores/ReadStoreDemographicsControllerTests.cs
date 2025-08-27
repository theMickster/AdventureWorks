using AdventureWorks.API.Controllers.v1.Stores;
using AdventureWorks.Application.Features.Sales.Queries;
using AdventureWorks.Models.Features.Sales;
using AdventureWorks.UnitTests._TestHelpers;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Net;

namespace AdventureWorks.UnitTests.API.Controllers.v1.Stores;

[ExcludeFromCodeCoverage]
public sealed class ReadStoreDemographicsControllerTests : UnitTestBase
{
    private readonly Mock<ILogger<ReadStoreDemographicsController>> _mockLogger = new();
    private readonly Mock<IMediator> _mockMediator = new();
    private readonly ReadStoreDemographicsController _sut;

    public ReadStoreDemographicsControllerTests()
    {
        _sut = new ReadStoreDemographicsController(_mockLogger.Object, _mockMediator.Object);
    }

    [Fact]
    public void Constructor_throws_correct_exceptions()
    {
        using (new AssertionScope())
        {
            _ = ((Action)(() => _ = new ReadStoreDemographicsController(null!, _mockMediator.Object)))
                .Should().Throw<ArgumentNullException>("because we expect a null argument exception.")
                .And.ParamName.Should().Be("logger");

            _ = ((Action)(() => _ = new ReadStoreDemographicsController(_mockLogger.Object, null!)))
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
        _mockMediator.Setup(x => x.Send(It.IsAny<ReadStoreDemographicsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((StoreDemographicsModel?)null);

        var result = await _sut.GetAsync(2534);

        var notFound = result as NotFoundObjectResult;

        using (new AssertionScope())
        {
            notFound.Should().NotBeNull();
            notFound!.StatusCode.Should().Be((int)HttpStatusCode.NotFound);
            notFound!.Value!.ToString().Should().Be("Unable to locate store demographics.");
        }
    }

    [Fact]
    public async Task GetAsync_returns_ok_with_modelAsync()
    {
        var output = new StoreDemographicsModel
        {
            StoreId = 2534,
            StoreName = "Bike World",
            AnnualSales = 1500000m,
            BankName = "Primary International",
            Internet = "DSL"
        };

        _mockMediator.Setup(x => x.Send(It.IsAny<ReadStoreDemographicsQuery>(), It.IsAny<CancellationToken>()))
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
    public async Task GetAsync_returns_ok_with_empty_survey_fields_when_demographics_xml_is_nullAsync()
    {
        // AC Scenario: "Store with null Demographics returns empty model" — the store exists,
        // so the controller must return 200 OK with a model whose survey fields are all null
        // (NOT 404). 404 is reserved for the store-does-not-exist case.
        var output = new StoreDemographicsModel
        {
            StoreId = 800,
            StoreName = "Catalog Store"
        };

        _mockMediator.Setup(x => x.Send(It.IsAny<ReadStoreDemographicsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(output);

        var result = await _sut.GetAsync(800);

        var okResult = result as OkObjectResult;

        using (new AssertionScope())
        {
            okResult.Should().NotBeNull();
            okResult!.StatusCode.Should().Be((int)HttpStatusCode.OK);
            okResult.Value.Should().BeSameAs(output);
            var model = okResult.Value as StoreDemographicsModel;
            model.Should().NotBeNull();
            model!.StoreId.Should().Be(800);
            model.StoreName.Should().Be("Catalog Store");
            StoreDemographicsAssertions.AssertSurveyFieldsNull(model);
        }
    }

    [Fact]
    public async Task GetAsync_sends_correct_storeId_to_mediatorAsync()
    {
        const int storeId = 2534;

        _mockMediator.Setup(x => x.Send(It.IsAny<ReadStoreDemographicsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((StoreDemographicsModel?)null);

        await _sut.GetAsync(storeId);

        _mockMediator.Verify(x => x.Send(
            It.Is<ReadStoreDemographicsQuery>(q => q.StoreId == storeId),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetAsync_forwards_cancellation_token_to_mediatorAsync()
    {
        const int storeId = 2534;

        using var cts = new CancellationTokenSource();

        _mockMediator.Setup(x => x.Send(It.IsAny<ReadStoreDemographicsQuery>(), cts.Token))
            .ReturnsAsync((StoreDemographicsModel?)null);

        await _sut.GetAsync(storeId, cts.Token);

        _mockMediator.Verify(x => x.Send(
            It.Is<ReadStoreDemographicsQuery>(q => q.StoreId == storeId),
            cts.Token), Times.Once);
    }
}
