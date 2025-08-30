using AdventureWorks.API.Controllers.v1.ShipMethod;
using AdventureWorks.Application.Features.Sales.Queries;
using AdventureWorks.Models.Features.Sales;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Net;

namespace AdventureWorks.UnitTests.API.Controllers.v1.ShipMethod;

[ExcludeFromCodeCoverage]
public sealed class ReadShipMethodControllerTests : UnitTestBase
{
    private readonly Mock<ILogger<ReadShipMethodController>> _mockLogger = new();
    private readonly Mock<IMediator> _mockMediator = new();
    private readonly ReadShipMethodController _sut;

    public ReadShipMethodControllerTests()
    {
        _sut = new ReadShipMethodController(_mockLogger.Object, _mockMediator.Object);
    }

    [Fact]
    public void Controller_throws_correct_exceptions()
    {
        using (new AssertionScope())
        {
            _ = ((Action)(() => _ = new ReadShipMethodController(null!, _mockMediator.Object)))
                .Should().Throw<ArgumentNullException>("because we expect a null argument exception.")
                .And.ParamName.Should().Be("logger");

            _ = ((Action)(() => _ = new ReadShipMethodController(_mockLogger.Object, null!)))
                .Should().Throw<ArgumentNullException>("because we expect a null argument exception.")
                .And.ParamName.Should().Be("mediator");
        }
    }

    [Fact]
    public void Controller_has_authorize_attribute()
    {
        typeof(ReadShipMethodController)
            .IsDefined(typeof(AuthorizeAttribute), true)
            .Should().BeTrue();
    }

    [Fact]
    public async Task GetById_returns_ok_Async()
    {
        _mockMediator.Setup(
                x => x.Send(It.Is<ReadShipMethodQuery>(q => q.Id == 1), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ShipMethodModel
            {
                ShipMethodId = 1,
                Name = "CARGO TRANSPORT 5",
                ShipBase = 3.95m,
                ShipRate = 1.25m,
                ModifiedDate = new DateTime(2026, 5, 4, 0, 0, 0, DateTimeKind.Utc)
            });

        var result = await _sut.GetByIdAsync(1);
        var objectResult = result as OkObjectResult;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.OK);
            objectResult.Value.Should().BeOfType<ShipMethodModel>();
        }
    }

    [Fact]
    public async Task GetById_returns_not_found_Async()
    {
        _mockMediator.Setup(x => x.Send(It.IsAny<ReadShipMethodQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ShipMethodModel)null!);

        var result = await _sut.GetByIdAsync(999);
        var objectResult = result as NotFoundObjectResult;
        var outputModel = objectResult!.Value as string;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.NotFound);
            outputModel.Should().Be("Unable to locate the ship method.");
        }
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task GetById_returns_bad_request_Async(int id)
    {
        var result = await _sut.GetByIdAsync(id);
        var objectResult = result as BadRequestObjectResult;
        var outputModel = objectResult!.Value as string;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
            outputModel.Should().Be("A valid ship method id must be specified.");
        }
    }

    [Fact]
    public async Task GetList_returns_ok_Async()
    {
        _mockMediator.Setup(
                x => x.Send(It.IsAny<ReadShipMethodListQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                new List<ShipMethodModel>
                {
                    new()
                    {
                        ShipMethodId = 1,
                        Name = "CARGO TRANSPORT 5",
                        ShipBase = 3.95m,
                        ShipRate = 1.25m,
                        ModifiedDate = new DateTime(2026, 5, 4, 0, 0, 0, DateTimeKind.Utc)
                    },
                    new()
                    {
                        ShipMethodId = 2,
                        Name = "ZY - EXPRESS",
                        ShipBase = 2.95m,
                        ShipRate = 1.50m,
                        ModifiedDate = new DateTime(2026, 5, 4, 0, 0, 0, DateTimeKind.Utc)
                    }
                });

        var result = await _sut.GetListAsync();
        var objectResult = result as OkObjectResult;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.OK);
        }
    }

    [Fact]
    public async Task GetList_returns_not_found_with_empty_list_Async()
    {
        _mockMediator.Setup(
                x => x.Send(It.IsAny<ReadShipMethodListQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ShipMethodModel>());

        var result = await _sut.GetListAsync();
        var objectResult = result as NotFoundObjectResult;
        var outputModel = objectResult!.Value as string;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.NotFound);
            outputModel.Should().Be("Unable to locate records in the ship method list.");
        }
    }
}
