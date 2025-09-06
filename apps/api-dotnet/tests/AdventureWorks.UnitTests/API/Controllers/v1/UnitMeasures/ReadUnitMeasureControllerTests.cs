using AdventureWorks.API.Controllers.v1.UnitMeasures;
using AdventureWorks.Application.Features.Production.Queries;
using AdventureWorks.Models.Features.Production;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Net;

namespace AdventureWorks.UnitTests.API.Controllers.v1.UnitMeasures;

[ExcludeFromCodeCoverage]
public sealed class ReadUnitMeasureControllerTests : UnitTestBase
{
    private readonly Mock<ILogger<ReadUnitMeasureController>> _mockLogger = new();
    private readonly Mock<IMediator> _mockMediator = new();
    private readonly ReadUnitMeasureController _sut;

    public ReadUnitMeasureControllerTests()
    {
        _sut = new ReadUnitMeasureController(_mockLogger.Object, _mockMediator.Object);
    }

    [Fact]
    public void Constructor_throws_when_logger_is_null()
    {
        _ = ((Action)(() => _ = new ReadUnitMeasureController(null!, _mockMediator.Object)))
            .Should().Throw<ArgumentNullException>("because we expect a null argument exception.")
            .And.ParamName.Should().Be("logger");
    }

    [Fact]
    public void Constructor_throws_when_mediator_is_null()
    {
        _ = ((Action)(() => _ = new ReadUnitMeasureController(_mockLogger.Object, null!)))
            .Should().Throw<ArgumentNullException>("because we expect a null argument exception.")
            .And.ParamName.Should().Be("mediator");
    }

    [Fact]
    public void Controller_has_authorize_attribute()
    {
        typeof(ReadUnitMeasureController)
            .IsDefined(typeof(AuthorizeAttribute), true)
            .Should().BeTrue();
    }

    [Fact]
    public async Task GetByCodeAsync_returns_bad_request_when_code_is_empty()
    {
        var result = await _sut.GetByCodeAsync(string.Empty);
        var objectResult = result as BadRequestObjectResult;
        var outputModel = objectResult!.Value as string;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
            outputModel.Should().Be("A valid unit measure code must be specified.");
        }
    }

    [Fact]
    public async Task GetByCodeAsync_returns_bad_request_when_code_is_whitespace()
    {
        var result = await _sut.GetByCodeAsync("   ");
        var objectResult = result as BadRequestObjectResult;
        var outputModel = objectResult!.Value as string;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
            outputModel.Should().Be("A valid unit measure code must be specified.");
        }
    }

    [Fact]
    public async Task GetByCodeAsync_returns_not_found_when_model_is_null()
    {
        _mockMediator.Setup(x => x.Send(It.IsAny<ReadUnitMeasureQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((UnitMeasureModel)null!);

        var result = await _sut.GetByCodeAsync("ZZZ");
        var objectResult = result as NotFoundObjectResult;
        var outputModel = objectResult!.Value as string;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.NotFound);
            outputModel.Should().Be("Unable to locate the unit measure.");
        }
    }

    [Fact]
    public async Task GetByCodeAsync_returns_ok_with_model()
    {
        _mockMediator.Setup(
                x => x.Send(It.Is<ReadUnitMeasureQuery>(q => q.Code == "EA"), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UnitMeasureModel
            {
                UnitMeasureCode = "EA",
                Name = "Each",
                ModifiedDate = new DateTime(2026, 5, 4, 0, 0, 0, DateTimeKind.Utc)
            });

        var result = await _sut.GetByCodeAsync("EA");
        var objectResult = result as OkObjectResult;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.OK);
            objectResult.Value.Should().BeOfType<UnitMeasureModel>();
        }
    }

    [Fact]
    public async Task GetListAsync_returns_not_found_when_list_is_empty()
    {
        _mockMediator.Setup(
                x => x.Send(It.IsAny<ReadUnitMeasureListQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<UnitMeasureModel>());

        var result = await _sut.GetListAsync();
        var objectResult = result as NotFoundObjectResult;
        var outputModel = objectResult!.Value as string;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.NotFound);
            outputModel.Should().Be("Unable to locate records in the unit measure list.");
        }
    }

    [Fact]
    public async Task GetListAsync_returns_ok_with_models()
    {
        _mockMediator.Setup(
                x => x.Send(It.IsAny<ReadUnitMeasureListQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<UnitMeasureModel>
            {
                new() { UnitMeasureCode = "EA", Name = "Each", ModifiedDate = new DateTime(2026, 5, 4, 0, 0, 0, DateTimeKind.Utc) },
                new() { UnitMeasureCode = "LB", Name = "Pound", ModifiedDate = new DateTime(2026, 5, 4, 0, 0, 0, DateTimeKind.Utc) }
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
