using AdventureWorks.API.Controllers.v1.Products;
using AdventureWorks.Application.Features.Production.Queries;
using AdventureWorks.Models.Features.Production;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Net;

namespace AdventureWorks.UnitTests.API.Controllers.v1.Products;

[ExcludeFromCodeCoverage]
public sealed class ReadProductInventoryControllerTests : UnitTestBase
{
    private readonly Mock<ILogger<ReadProductInventoryController>> _mockLogger = new();
    private readonly Mock<IMediator> _mockMediator = new();
    private readonly ReadProductInventoryController _sut;

    public ReadProductInventoryControllerTests()
    {
        _sut = new ReadProductInventoryController(_mockLogger.Object, _mockMediator.Object);
    }

    [Fact]
    public void Controller_throws_correct_exceptions()
    {
        using (new AssertionScope())
        {
            _ = ((Action)(() => _ = new ReadProductInventoryController(null!, _mockMediator.Object)))
                .Should().Throw<ArgumentNullException>()
                .And.ParamName.Should().Be("logger");

            _ = ((Action)(() => _ = new ReadProductInventoryController(_mockLogger.Object, null!)))
                .Should().Throw<ArgumentNullException>()
                .And.ParamName.Should().Be("mediator");
        }
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-999)]
    public async Task GetInventoryAsync_invalid_id_returns_bad_requestAsync(int id)
    {
        var result = await _sut.GetInventoryAsync(id);

        var objectResult = result as BadRequestObjectResult;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
            objectResult!.Value!.ToString().Should().Be("A valid product id must be specified.");
        }
    }

    [Fact]
    public async Task GetInventoryAsync_valid_id_returns_ok_Async()
    {
        var inventory = new List<ProductInventoryModel>
        {
            new() { LocationId = 1, LocationName = "Tool Crib", Shelf = "A", Bin = 1, Quantity = 50 },
            new() { LocationId = 6, LocationName = "Miscellaneous Storage", Shelf = "B", Bin = 5, Quantity = 35 }
        };

        _mockMediator.Setup(x => x.Send(It.IsAny<ReadProductInventoryQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(inventory);

        var result = await _sut.GetInventoryAsync(1);
        var objectResult = result as OkObjectResult;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.OK);

            var returnedList = objectResult.Value as List<ProductInventoryModel>;
            returnedList.Should().NotBeNull();
            returnedList!.Count.Should().Be(2);
        }
    }
}
