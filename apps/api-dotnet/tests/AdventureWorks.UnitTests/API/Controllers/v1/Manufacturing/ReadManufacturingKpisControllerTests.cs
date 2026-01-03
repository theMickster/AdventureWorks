using AdventureWorks.API.Controllers.v1.Manufacturing;
using AdventureWorks.Application.Features.Production.Queries;
using AdventureWorks.Models.Features.Production;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace AdventureWorks.UnitTests.API.Controllers.v1.Manufacturing;

[ExcludeFromCodeCoverage]
public sealed class ReadManufacturingKpisControllerTests : UnitTestBase
{
    private readonly Mock<IMediator> _mockMediator = new();
    private readonly ReadManufacturingKpisController _sut;

    public ReadManufacturingKpisControllerTests()
    {
        _sut = new ReadManufacturingKpisController(_mockMediator.Object);
    }

    [Fact]
    public void Controller_throws_correct_exception_when_mediator_is_null()
    {
        _ = ((Action)(() => _ = new ReadManufacturingKpisController(null!)))
            .Should().Throw<ArgumentNullException>()
            .And.ParamName.Should().Be("mediator");
    }

    [Fact]
    public async Task GetKpisAsync_returns_ok_with_model()
    {
        var testModel = new ManufacturingKpisModel { TotalWorkOrders = 2, TotalOrdered = 15, TotalStocked = 12, TotalScrapped = 3 };
        _mockMediator
            .Setup(x => x.Send(It.IsAny<ReadManufacturingKpisQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(testModel);

        var result = await _sut.GetKpisAsync(CancellationToken.None);
        var objectResult = result as OkObjectResult;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.OK);
            objectResult.Value.Should().BeOfType<ManufacturingKpisModel>();
            objectResult.Value.Should().BeSameAs(testModel);
        }
    }

    [Fact]
    public async Task GetKpisAsync_sends_correct_query()
    {
        _mockMediator
            .Setup(x => x.Send(It.IsAny<ReadManufacturingKpisQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ManufacturingKpisModel());

        await _sut.GetKpisAsync(CancellationToken.None);

        _mockMediator.Verify(
            x => x.Send(It.IsAny<ReadManufacturingKpisQuery>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
