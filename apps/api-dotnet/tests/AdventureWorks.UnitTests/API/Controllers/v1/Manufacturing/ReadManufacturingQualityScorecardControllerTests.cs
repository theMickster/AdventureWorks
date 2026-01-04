using AdventureWorks.API.Controllers.v1.Manufacturing;
using AdventureWorks.Application.Features.Production.Queries;
using AdventureWorks.Models.Features.Production;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace AdventureWorks.UnitTests.API.Controllers.v1.Manufacturing;

[ExcludeFromCodeCoverage]
public sealed class ReadManufacturingQualityScorecardControllerTests : UnitTestBase
{
    private readonly Mock<IMediator> _mockMediator = new();
    private readonly ReadManufacturingQualityScorecardController _sut;

    public ReadManufacturingQualityScorecardControllerTests()
    {
        _sut = new ReadManufacturingQualityScorecardController(_mockMediator.Object);
    }

    [Fact]
    public void Controller_throws_correct_exception_when_mediator_is_null()
    {
        _ = ((Action)(() => _ = new ReadManufacturingQualityScorecardController(null!)))
            .Should().Throw<ArgumentNullException>()
            .And.ParamName.Should().Be("mediator");
    }

    [Fact]
    public async Task GetQualityScorecardAsync_returns_ok_with_model()
    {
        var testModel = new ManufacturingQualityScorecardModel();
        _mockMediator
            .Setup(mediator => mediator.Send(
                It.IsAny<ReadManufacturingQualityScorecardQuery>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(testModel);

        var result = await _sut.GetQualityScorecardAsync(CancellationToken.None);
        var objectResult = result as OkObjectResult;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.OK);
            objectResult.Value.Should().BeOfType<ManufacturingQualityScorecardModel>();
            objectResult.Value.Should().BeSameAs(testModel);
        }
    }

    [Fact]
    public async Task GetQualityScorecardAsync_sends_correct_query()
    {
        _mockMediator
            .Setup(mediator => mediator.Send(
                It.IsAny<ReadManufacturingQualityScorecardQuery>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ManufacturingQualityScorecardModel());

        await _sut.GetQualityScorecardAsync(CancellationToken.None);

        _mockMediator.Verify(
            mediator => mediator.Send(
                It.IsAny<ReadManufacturingQualityScorecardQuery>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
