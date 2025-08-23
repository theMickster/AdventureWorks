using AdventureWorks.API.Controllers.v1.Products;
using AdventureWorks.Application.Features.Production.Queries;
using AdventureWorks.Models.Features.Production;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Net;

namespace AdventureWorks.UnitTests.API.Controllers.v1.Products;

[ExcludeFromCodeCoverage]
public sealed class ReadProductPriceHistoryControllerTests : UnitTestBase
{
    private readonly Mock<ILogger<ReadProductPriceHistoryController>> _mockLogger = new();
    private readonly Mock<IMediator> _mockMediator = new();
    private readonly ReadProductPriceHistoryController _sut;

    public ReadProductPriceHistoryControllerTests()
    {
        _sut = new ReadProductPriceHistoryController(_mockLogger.Object, _mockMediator.Object);
    }

    [Fact]
    public void Controller_throws_correct_exceptions()
    {
        using (new AssertionScope())
        {
            _ = ((Action)(() => _ = new ReadProductPriceHistoryController(null!, _mockMediator.Object)))
                .Should().Throw<ArgumentNullException>()
                .And.ParamName.Should().Be("logger");

            _ = ((Action)(() => _ = new ReadProductPriceHistoryController(_mockLogger.Object, null!)))
                .Should().Throw<ArgumentNullException>()
                .And.ParamName.Should().Be("mediator");
        }
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-999)]
    public async Task GetPriceHistoryAsync_invalid_id_returns_bad_requestAsync(int id)
    {
        var result = await _sut.GetPriceHistoryAsync(id);

        var objectResult = result as BadRequestObjectResult;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
            objectResult!.Value!.ToString().Should().Be("A valid product id must be specified.");
        }
    }

    [Fact]
    public async Task GetPriceHistoryAsync_valid_id_returns_ok_Async()
    {
        var history = new List<ProductPriceHistoryModel>
        {
            new() { ProductId = 1, StartDate = new DateTime(2011, 5, 31), EndDate = new DateTime(2012, 5, 29), Price = 3399.99m, Type = "list" },
            new() { ProductId = 1, StartDate = new DateTime(2011, 5, 31), EndDate = new DateTime(2012, 5, 29), Price = 1912.15m, Type = "cost" }
        };

        _mockMediator.Setup(x => x.Send(It.IsAny<ReadProductPriceHistoryQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(history);

        var result = await _sut.GetPriceHistoryAsync(1);
        var objectResult = result as OkObjectResult;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.OK);

            var returnedList = objectResult.Value as List<ProductPriceHistoryModel>;
            returnedList.Should().NotBeNull();
            returnedList!.Count.Should().Be(2);
        }
    }
}
