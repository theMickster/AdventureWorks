using AdventureWorks.API.Controllers.v1.ScrapReasons;
using AdventureWorks.Application.Features.Production.Queries;
using AdventureWorks.Models.Features.Production;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Net;

namespace AdventureWorks.UnitTests.API.Controllers.v1.ScrapReasons;

[ExcludeFromCodeCoverage]
public sealed class ReadScrapReasonControllerTests : UnitTestBase
{
    private readonly Mock<ILogger<ReadScrapReasonController>> _mockLogger = new();
    private readonly Mock<IMediator> _mockMediator = new();
    private readonly ReadScrapReasonController _sut;

    public ReadScrapReasonControllerTests()
    {
        _sut = new ReadScrapReasonController(_mockLogger.Object, _mockMediator.Object);
    }

    [Fact]
    public void Constructor_throws_when_logger_is_null()
    {
        _ = ((Action)(() => _ = new ReadScrapReasonController(null!, _mockMediator.Object)))
            .Should().Throw<ArgumentNullException>("because we expect a null argument exception.")
            .And.ParamName.Should().Be("logger");
    }

    [Fact]
    public void Constructor_throws_when_mediator_is_null()
    {
        _ = ((Action)(() => _ = new ReadScrapReasonController(_mockLogger.Object, null!)))
            .Should().Throw<ArgumentNullException>("because we expect a null argument exception.")
            .And.ParamName.Should().Be("mediator");
    }

    [Fact]
    public void Controller_has_authorize_attribute()
    {
        typeof(ReadScrapReasonController)
            .IsDefined(typeof(AuthorizeAttribute), true)
            .Should().BeTrue();
    }

    [Fact]
    public async Task GetByIdAsync_returns_bad_request_when_id_is_zero()
    {
        var result = await _sut.GetByIdAsync(0);
        var objectResult = result as BadRequestObjectResult;
        var outputModel = objectResult!.Value as string;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
            outputModel.Should().Be("A valid scrap reason id must be specified.");
        }
    }

    [Fact]
    public async Task GetByIdAsync_returns_bad_request_when_id_is_negative()
    {
        var result = await _sut.GetByIdAsync(-1);
        var objectResult = result as BadRequestObjectResult;
        var outputModel = objectResult!.Value as string;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
            outputModel.Should().Be("A valid scrap reason id must be specified.");
        }
    }

    [Fact]
    public async Task GetByIdAsync_returns_bad_request_when_id_exceeds_short_max()
    {
        var result = await _sut.GetByIdAsync((int)short.MaxValue + 1);
        var objectResult = result as BadRequestObjectResult;
        var outputModel = objectResult!.Value as string;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
            outputModel.Should().Be("A valid scrap reason id must be specified.");
        }
    }

    [Fact]
    public async Task GetByIdAsync_returns_not_found_when_model_is_null()
    {
        _mockMediator.Setup(x => x.Send(It.IsAny<ReadScrapReasonQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ScrapReasonModel)null!);

        var result = await _sut.GetByIdAsync(999);
        var objectResult = result as NotFoundObjectResult;
        var outputModel = objectResult!.Value as string;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.NotFound);
            outputModel.Should().Be("Unable to locate the scrap reason.");
        }
    }

    [Fact]
    public async Task GetByIdAsync_returns_ok_with_model()
    {
        _mockMediator.Setup(
                x => x.Send(It.Is<ReadScrapReasonQuery>(q => q.Id == 1), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ScrapReasonModel
            {
                ScrapReasonId = (short)1,
                Name = "Brake assembly not as ordered",
                ModifiedDate = new DateTime(2026, 5, 4, 0, 0, 0, DateTimeKind.Utc)
            });

        var result = await _sut.GetByIdAsync(1);
        var objectResult = result as OkObjectResult;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.OK);
            objectResult.Value.Should().BeOfType<ScrapReasonModel>();
        }
    }

    [Fact]
    public async Task GetListAsync_returns_not_found_when_list_is_empty()
    {
        _mockMediator.Setup(
                x => x.Send(It.IsAny<ReadScrapReasonListQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ScrapReasonModel>());

        var result = await _sut.GetListAsync();
        var objectResult = result as NotFoundObjectResult;
        var outputModel = objectResult!.Value as string;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.NotFound);
            outputModel.Should().Be("Unable to locate records in the scrap reason list.");
        }
    }

    [Fact]
    public async Task GetListAsync_returns_ok_with_models()
    {
        _mockMediator.Setup(
                x => x.Send(It.IsAny<ReadScrapReasonListQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ScrapReasonModel>
            {
                new() { ScrapReasonId = (short)1, Name = "Brake assembly not as ordered", ModifiedDate = new DateTime(2026, 5, 4, 0, 0, 0, DateTimeKind.Utc) },
                new() { ScrapReasonId = (short)2, Name = "Color incorrect", ModifiedDate = new DateTime(2026, 5, 4, 0, 0, 0, DateTimeKind.Utc) }
            });

        var result = await _sut.GetListAsync();
        var objectResult = result as OkObjectResult;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.OK);
        }
    }
}
