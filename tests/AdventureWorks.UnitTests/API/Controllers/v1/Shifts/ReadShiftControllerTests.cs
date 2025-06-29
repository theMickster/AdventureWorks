using AdventureWorks.API.Controllers.v1.Shifts;
using AdventureWorks.Application.Features.HumanResources.Queries;
using AdventureWorks.Models.Features.HumanResources;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Net;

namespace AdventureWorks.UnitTests.API.Controllers.v1.Shifts;

[ExcludeFromCodeCoverage]
public sealed class ReadShiftControllerTests : UnitTestBase
{
    private readonly Mock<ILogger<ReadShiftController>> _mockLogger = new();
    private readonly Mock<IMediator> _mockMediator = new();
    private readonly ReadShiftController _sut;

    public ReadShiftControllerTests()
    {
        _sut = new ReadShiftController(_mockLogger.Object, _mockMediator.Object);
    }

    [Fact]
    public void Controller_throws_correct_exceptions()
    {
        using (new AssertionScope())
        {
            _ = ((Action)(() => _ = new ReadShiftController(null!, _mockMediator.Object)))
                .Should().Throw<ArgumentNullException>("because we expect a null argument exception.")
                .And.ParamName.Should().Be("logger");

            _ = ((Action)(() => _ = new ReadShiftController(_mockLogger.Object, null!)))
                .Should().Throw<ArgumentNullException>("because we expect a null argument exception.")
                .And.ParamName.Should().Be("mediator");
        }
    }

    [Fact]
    public async Task GetById_returns_ok_Async()
    {
        _mockMediator.Setup(
                x => x.Send(It.IsAny<ReadShiftQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ShiftModel { Id = 1, Name = "Day" });

        var result = await _sut.GetByIdAsync(1);
        var objectResult = result as OkObjectResult;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.OK);
        }
    }

    [Fact]
    public async Task GetById_returns_not_found_Async()
    {
        _mockMediator.Setup(x => x.Send(It.IsAny<ReadShiftQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ShiftModel)null!);

        var result = await _sut.GetByIdAsync(99);
        var objectResult = result as NotFoundObjectResult;
        var outputModel = objectResult!.Value! as string;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.NotFound);

            outputModel.Should().NotBeNull();
            outputModel!.Should().Be("Unable to locate the shift.");
        }
    }

    [Fact]
    public async Task GetById_returns_bad_request_Async()
    {
        var result = await _sut.GetByIdAsync(0);
        var objectResult = result as BadRequestObjectResult;
        var outputModel = objectResult!.Value! as string;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);

            outputModel.Should().NotBeNull();
            outputModel!.Should().Be("A valid shift id must be specified.");
        }
    }

    [Fact]
    public async Task GetList_returns_ok_Async()
    {
        _mockMediator.Setup(
                x => x.Send(It.IsAny<ReadShiftListQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(
            [
                new() { Id = 1, Name = "Day" },
                new() { Id = 2, Name = "Evening" },
                new() { Id = 3, Name = "Night" }
            ]);

        var result = await _sut.GetListAsync();

        var objectResult = result as OkObjectResult;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.OK);
        }
    }

    [Fact]
    public async Task GetList_returns_not_found_Async()
    {
        _mockMediator.Setup(
                x => x.Send(It.IsAny<ReadShiftListQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ShiftModel>());

        var result = await _sut.GetListAsync();
        var objectResult = result as NotFoundObjectResult;
        var outputModel = objectResult!.Value! as string;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.NotFound);

            outputModel.Should().NotBeNull();
            outputModel!.Should().Be("Unable to locate records in the shift list.");
        }
    }
}
