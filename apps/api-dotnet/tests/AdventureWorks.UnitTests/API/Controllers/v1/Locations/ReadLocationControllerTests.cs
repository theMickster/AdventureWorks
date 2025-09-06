using AdventureWorks.API.Controllers.v1.Locations;
using AdventureWorks.Application.Features.Production.Queries;
using AdventureWorks.Models.Features.Production;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Net;

namespace AdventureWorks.UnitTests.API.Controllers.v1.Locations;

[ExcludeFromCodeCoverage]
public sealed class ReadLocationControllerTests : UnitTestBase
{
    private readonly Mock<ILogger<ReadLocationController>> _mockLogger = new();
    private readonly Mock<IMediator> _mockMediator = new();
    private readonly ReadLocationController _sut;

    public ReadLocationControllerTests()
    {
        _sut = new ReadLocationController(_mockLogger.Object, _mockMediator.Object);
    }

    [Fact]
    public void Constructor_throws_when_logger_is_null()
    {
        _ = ((Action)(() => _ = new ReadLocationController(null!, _mockMediator.Object)))
            .Should().Throw<ArgumentNullException>("because we expect a null argument exception.")
            .And.ParamName.Should().Be("logger");
    }

    [Fact]
    public void Constructor_throws_when_mediator_is_null()
    {
        _ = ((Action)(() => _ = new ReadLocationController(_mockLogger.Object, null!)))
            .Should().Throw<ArgumentNullException>("because we expect a null argument exception.")
            .And.ParamName.Should().Be("mediator");
    }

    [Fact]
    public void Controller_has_authorize_attribute()
    {
        typeof(ReadLocationController)
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
            outputModel.Should().Be("A valid location id must be specified.");
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
            outputModel.Should().Be("A valid location id must be specified.");
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
            outputModel.Should().Be("A valid location id must be specified.");
        }
    }

    [Fact]
    public async Task GetByIdAsync_returns_not_found_when_model_is_null()
    {
        _mockMediator.Setup(x => x.Send(It.IsAny<ReadLocationQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((LocationModel)null!);

        var result = await _sut.GetByIdAsync(999);
        var objectResult = result as NotFoundObjectResult;
        var outputModel = objectResult!.Value as string;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.NotFound);
            outputModel.Should().Be("Unable to locate the location.");
        }
    }

    [Fact]
    public async Task GetByIdAsync_returns_ok_with_model()
    {
        _mockMediator.Setup(
                x => x.Send(It.Is<ReadLocationQuery>(q => q.Id == 1), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new LocationModel
            {
                LocationId = (short)1,
                Name = "Tool Crib",
                CostRate = 0.00m,
                Availability = 96.46m,
                ModifiedDate = new DateTime(2026, 5, 4, 0, 0, 0, DateTimeKind.Utc)
            });

        var result = await _sut.GetByIdAsync(1);
        var objectResult = result as OkObjectResult;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.OK);
            objectResult.Value.Should().BeOfType<LocationModel>();
        }
    }

    [Fact]
    public async Task GetListAsync_returns_not_found_when_list_is_empty()
    {
        _mockMediator.Setup(
                x => x.Send(It.IsAny<ReadLocationListQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<LocationModel>());

        var result = await _sut.GetListAsync();
        var objectResult = result as NotFoundObjectResult;
        var outputModel = objectResult!.Value as string;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.NotFound);
            outputModel.Should().Be("Unable to locate records in the location list.");
        }
    }

    [Fact]
    public async Task GetListAsync_returns_ok_with_models()
    {
        _mockMediator.Setup(
                x => x.Send(It.IsAny<ReadLocationListQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<LocationModel>
            {
                new() { LocationId = (short)1, Name = "Tool Crib", CostRate = 0.00m, Availability = 96.46m, ModifiedDate = new DateTime(2026, 5, 4, 0, 0, 0, DateTimeKind.Utc) },
                new() { LocationId = (short)2, Name = "Sheet Metal Racks", CostRate = 0.00m, Availability = 0.00m, ModifiedDate = new DateTime(2026, 5, 4, 0, 0, 0, DateTimeKind.Utc) }
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
