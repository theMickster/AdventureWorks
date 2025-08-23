using AdventureWorks.API.Controllers.v1.Products;
using AdventureWorks.Application.Features.Production.Queries;
using AdventureWorks.Models.Features.Production;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Net;

namespace AdventureWorks.UnitTests.API.Controllers.v1.Products;

[ExcludeFromCodeCoverage]
public sealed class ReadProductControllerTests : UnitTestBase
{
    private readonly Mock<ILogger<ReadProductController>> _mockLogger = new();
    private readonly Mock<IMediator> _mockMediator = new();
    private readonly ReadProductController _sut;

    public ReadProductControllerTests()
    {
        _sut = new ReadProductController(_mockLogger.Object, _mockMediator.Object);
    }

    [Fact]
    public void Controller_throws_correct_exceptions()
    {
        using (new AssertionScope())
        {
            _ = ((Action)(() => _ = new ReadProductController(null!, _mockMediator.Object)))
                .Should().Throw<ArgumentNullException>()
                .And.ParamName.Should().Be("logger");

            _ = ((Action)(() => _ = new ReadProductController(_mockLogger.Object, null!)))
                .Should().Throw<ArgumentNullException>()
                .And.ParamName.Should().Be("mediator");
        }
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task GetByIdAsync_invalid_id_returns_bad_requestAsync(int id)
    {
        var result = await _sut.GetByIdAsync(id);
        var objectResult = result as ObjectResult;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
        }
    }

    [Fact]
    public async Task GetByIdAsync_not_found_returns_404Async()
    {
        _mockMediator.Setup(x => x.Send(It.IsAny<ReadProductQuery>(), CancellationToken.None))
            .ReturnsAsync((ProductDetailModel?)null);

        var result = await _sut.GetByIdAsync(999);
        var objectResult = result as ObjectResult;

        objectResult!.StatusCode.Should().Be((int)HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetByIdAsync_valid_id_returns_okAsync()
    {
        var model = new ProductDetailModel { Id = 1, Name = "Mountain Bike", ProductNumber = "BK-M82S-38" };

        _mockMediator.Setup(x => x.Send(It.IsAny<ReadProductQuery>(), CancellationToken.None))
            .ReturnsAsync(model);

        var result = await _sut.GetByIdAsync(1);
        var okResult = result as OkObjectResult;

        using (new AssertionScope())
        {
            okResult.Should().NotBeNull();
            okResult!.StatusCode.Should().Be((int)HttpStatusCode.OK);
            var returnedModel = okResult.Value as ProductDetailModel;
            returnedModel.Should().NotBeNull();
            returnedModel!.Id.Should().Be(1);
        }
    }
}
